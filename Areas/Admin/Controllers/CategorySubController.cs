using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dong_Xuan_Market_Online.Models;
using Microsoft.AspNetCore.Authorization;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Diacritics.Extensions;

namespace Dong_Xuan_Market_Online.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/CategorySub")]
    [Authorize(Roles = "Admin,Developer")]
    public class CategorySubController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategorySubController(DataContext context, IWebHostEnvironment webHostEnvironment)
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
            var categorySubs = await _dataContext.CategorySubModels.Include(c => c.Category).ToListAsync();

            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }

            int recsCount = categorySubs.Count();
            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = categorySubs.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;
            return View(data);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_dataContext.Categories, "Id", "Name");
            return View();
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategorySubModel categorySub)
        {
            if (ModelState.IsValid)
            {
                categorySub.Slug = GenerateSlug(categorySub.Name);

                var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == categorySub.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thể loại đã có trong DataBase");
                    return View(categorySub);
                }
                _dataContext.Add(categorySub);
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
            var categorySub = await _dataContext.CategorySubModels.FindAsync(id);
            if (categorySub == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_dataContext.Categories, "Id", "Name", categorySub.CategoryId);
            return View(categorySub);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategorySubModel categorySub)
        {
            if (id != categorySub.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    categorySub.Slug = GenerateSlug(categorySub.Name);

                    // Kiểm tra xem danh mục con với slug mới đã tồn tại chưa
                    var existingCategorySub = await _dataContext.CategorySubModels
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Slug == categorySub.Slug && c.Id != id);

                    if (existingCategorySub != null)
                    {
                        ModelState.AddModelError("", "Danh mục con đã có trong DataBase");
                        ViewData["CategoryId"] = new SelectList(_dataContext.Categories, "Id", "Name", categorySub.CategoryId);
                        return View(categorySub);
                    }

                    // Đặt trạng thái của entity là Modified
                    _dataContext.Entry(categorySub).State = EntityState.Modified;
                    await _dataContext.SaveChangesAsync();

                    TempData["success"] = "Cập nhật danh mục con thành công";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategorySubModelExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["CategoryId"] = new SelectList(_dataContext.Categories, "Id", "Name", categorySub.CategoryId);
            TempData["error"] = "Lỗi nhập dữ liệu, vui lòng kiểm tra lại";
            return View(categorySub);
        }

        private bool CategorySubModelExists(int id)
        {
            return _dataContext.CategorySubModels.Any(e => e.Id == id);
        }
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var categorysub = await _dataContext.CategorySubModels.FindAsync(id);
            if (categorysub == null)
            {
                return NotFound();
            }

            _dataContext.CategorySubModels.Remove(categorysub);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Thể loại đã bị xóa";
            return RedirectToAction("Index");
        }
    }
}
