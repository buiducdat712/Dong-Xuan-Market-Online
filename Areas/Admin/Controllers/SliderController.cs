using Diacritics.Extensions;
using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Slider")]
    [Authorize(Roles = "Admin,Developer")]
    public class SliderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SliderController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            // Lấy danh sách các nhãn hiệu từ cơ sở dữ liệu
            List<SliderModel> sliders = await _dataContext.Sliders.OrderByDescending(p => p.Id).ToListAsync();

            // Thiết lập số lượng mục trên mỗi trang
            const int pageSize = 10;

            // Kiểm tra trang hiện tại có hợp lệ không
            if (pg < 1)
            {
                pg = 1;
            }

            // Đếm tổng số mục
            int recsCount = sliders.Count;

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Tính toán số lượng mục cần bỏ qua
            int recSkip = (pg - 1) * pageSize;

            // Lấy dữ liệu cho trang hiện tại
            var data = sliders.Skip(recSkip).Take(pager.PageSize).ToList();

            // Truyền đối tượng phân trang cho View
            ViewBag.Pager = pager;

            // Trả về view với dữ liệu phân trang
            return View(data);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(SliderModel slider)
        {
            if (ModelState.IsValid)
            {
                if (slider.ImageUpLoad != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/banner");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpLoad.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await slider.ImageUpLoad.CopyToAsync(fs);
                    }
                    slider.Image = imageName;
                }

                _dataContext.Add(slider);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm hình ảnh thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Lỗi nhập dữ liệu, vui lòng kiểm tra lại";
            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            string errorMessage = string.Join("\n", errors);
            return BadRequest(errorMessage);
        }

        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var slider = await _dataContext.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, SliderModel slider)
        {
            if (id != slider.Id)
            {
                return BadRequest("ID không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                var oldImage = await _dataContext.Sliders.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                if (oldImage == null)
                {
                    return NotFound();
                }

                if (slider.ImageUpLoad != null)
                {
                    // Xóa hình cũ nếu không phải là "noimage.jpg"
                    if (!string.Equals(oldImage.Image, "noimage.jpg", StringComparison.OrdinalIgnoreCase))
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/banner");
                        string oldFilePath = Path.Combine(uploadsDir, oldImage.Image);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu hình mới
                    string newUploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/banner");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpLoad.FileName;
                    string filePath = Path.Combine(newUploadsDir, imageName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await slider.ImageUpLoad.CopyToAsync(fs);
                    }

                    slider.Image = imageName;
                }
                else
                {
                    slider.Image = oldImage.Image; // Giữ nguyên hình cũ nếu không cập nhật hình mới
                }

                // Cập nhật các thuộc tính khác của hình ảnh
                _dataContext.Update(slider);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật hình ảnh thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Lỗi nhập dữ liệu, vui lòng kiểm tra lại";
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            var errorMessage = string.Join("\n", errors);
            return View(slider); // Trả lại view với lỗi để người dùng có thể sửa
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }

            if (!string.Equals(slider.Image, "noimage.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/banner");
                string oldfileImage = Path.Combine(uploadsDir, slider.Image);
                if (System.IO.File.Exists(oldfileImage))
                {
                    System.IO.File.Delete(oldfileImage);
                }
            }

            _dataContext.Sliders.Remove( slider);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thương hiệu đã bị xóa";
            return RedirectToAction("Index");
        }
    }
}