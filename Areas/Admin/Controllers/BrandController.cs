using Diacritics.Extensions;
using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dong_Xuan_Market_Online.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Brand")]
    [Authorize(Roles = "Admin,Developer")]
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BrandController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        private string GenerateSlug(string name)
        {
            // Remove diacritics
            var normalizedString = name.RemoveDiacritics();

            // Replace spaces with hyphens and convert to lowercase
            var slug = normalizedString.Replace(" ", "-").ToLower();

            // Remove any characters that are not letters, digits, or hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");

            return slug;
        }
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            // Lấy danh sách các nhãn hiệu từ cơ sở dữ liệu
            List<BrandModel> brands = await _dataContext.Brands.OrderByDescending(p => p.Id).ToListAsync();

            // Thiết lập số lượng mục trên mỗi trang
            const int pageSize = 10;

            // Kiểm tra trang hiện tại có hợp lệ không
            if (pg < 1)
            {
                pg = 1;
            }

            // Đếm tổng số mục
            int recsCount = brands.Count;

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Tính toán số lượng mục cần bỏ qua
            int recSkip = (pg - 1) * pageSize;

            // Lấy dữ liệu cho trang hiện tại
            var data = brands.Skip(recSkip).Take(pager.PageSize).ToList();

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
        public async Task<IActionResult> Create(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = GenerateSlug(brand.Name);
                var slug = await _dataContext.Brands.FirstOrDefaultAsync(b => b.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thương hiệu đã có trong DataBase");
                    return View(brand);
                }
                else
                {
                    if (brand.ImageUpLoad != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/brand-logo");
                        string imageName = Guid.NewGuid().ToString() + "_" + brand.ImageUpLoad.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await brand.ImageUpLoad.CopyToAsync(fs);
                        }
                        brand.Cate = imageName;
                    }
                }
                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm thương hiệu thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Lỗi nhập dữ liệu, Vui lòng kiểm tra lại";
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
        [Route("Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _dataContext.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = GenerateSlug(brand.Name);

                var existingBrand = await _dataContext.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Slug == brand.Slug && b.Id != id);
                if (existingBrand != null)
                {
                    ModelState.AddModelError("", "Thương hiệu đã có trong DataBase");
                    return View(brand);
                }

                var oldBrand = await _dataContext.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                if (oldBrand == null)
                {
                    return NotFound();
                }

                if (brand.ImageUpLoad != null)
                {
                    // Xóa hình cũ nếu không phải là noimage.jpg
                    if (!string.Equals(oldBrand.Cate, "noimage.jpg"))
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/brand-logo");
                        string oldfileImage = Path.Combine(uploadsDir, oldBrand.Cate);
                        if (System.IO.File.Exists(oldfileImage))
                        {
                            System.IO.File.Delete(oldfileImage);
                        }
                    }

                    // Lưu hình mới
                    string newUploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/brand-logo");
                    string imageName = Guid.NewGuid().ToString() + "_" + brand.ImageUpLoad.FileName;
                    string filePath = Path.Combine(newUploadsDir, imageName);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await brand.ImageUpLoad.CopyToAsync(fs);
                    }
                    brand.Cate = imageName;
                }
                else
                {
                    brand.Cate = oldBrand.Cate; // Giữ nguyên hình cũ nếu không cập nhật hình mới
                }

                _dataContext.Entry(brand).State = EntityState.Modified;
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thương hiệu thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Lỗi nhập dữ liệu, Vui lòng kiểm tra lại";
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

        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(id);
            if (!string.Equals(brand.Cate, "noimage.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/brand-logo");
                string oldfileImage = Path.Combine(uploadsDir, brand.Cate);
                if (System.IO.File.Exists(oldfileImage))
                {
                    System.IO.File.Delete(oldfileImage);
                }
            }
            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Thương hiệu đã bị xóa";
            return RedirectToAction("Index");
        }
    }
}
