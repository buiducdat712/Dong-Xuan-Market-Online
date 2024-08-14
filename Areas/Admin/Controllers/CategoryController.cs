using Diacritics.Extensions;
using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dong_Xuan_Market_Online.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Category")]
    [Authorize(Roles = "Admin,Developer")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(DataContext context, IWebHostEnvironment webHostEnvironment)
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

        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<CategoryModel> category = _dataContext.Categories.ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = category.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = GenerateSlug(category.Name);

                var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thể loại đã có trong DataBase");
                    return View(category);
                }
                else
                {
                    if (category.ImageUpLoad != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/company-logo");
                        string imageName = Guid.NewGuid().ToString() + "_" + category.ImageUpLoad.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await category.ImageUpLoad.CopyToAsync(fs);
                        }
                        category.Logo= imageName;
                    }
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm thể loại thành công";
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
            var category = await _dataContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = GenerateSlug(category.Name);

                var existingCategory = await _dataContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == category.Slug && c.Id != id);
                if (existingCategory != null)
                {
                    ModelState.AddModelError("", "Thể loại đã có trong DataBase");
                    return View(category);
                }

                var oldCategory = await _dataContext.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                if (oldCategory == null)
                {
                    return NotFound();
                }

                if (category.ImageUpLoad != null)
                {
                    // Xóa hình cũ nếu không phải là noimage.jpg
                    if (!string.Equals(oldCategory.Logo, "noimage.jpg"))
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/company-logo");
                        string oldfileImage = Path.Combine(uploadsDir, oldCategory.Logo);
                        if (System.IO.File.Exists(oldfileImage))
                        {
                            System.IO.File.Delete(oldfileImage);
                        }
                    }

                    // Lưu hình mới
                    string newUploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/company-logo");
                    string imageName = Guid.NewGuid().ToString() + "_" + category.ImageUpLoad.FileName;
                    string filePath = Path.Combine(newUploadsDir, imageName);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await category.ImageUpLoad.CopyToAsync(fs);
                    }
                    category.Logo= imageName;
                }
                else
                {
                    category.Logo= oldCategory.Logo; // Giữ nguyên hình cũ nếu không cập nhật hình mới
                }

                _dataContext.Entry(category).State = EntityState.Modified;
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thể loại thành công";
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
            var category = await _dataContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            if (!string.Equals(category.Logo, "noimage.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/company-logo");
                string oldfileImage = Path.Combine(uploadsDir, category.Logo);
                if (System.IO.File.Exists(oldfileImage))
                {
                    System.IO.File.Delete(oldfileImage);
                }
            }

            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Thể loại đã bị xóa";
            return RedirectToAction("Index");
        }
    }
}
