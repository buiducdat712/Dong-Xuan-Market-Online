using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;

        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(string Slug = "")
        {
            CategoryModel category = _dataContext.Categories.Where(c => c.Slug == Slug).FirstOrDefault();

            if (category == null) return RedirectToAction("Index");

            var fashionsByCategory = _dataContext.Products.Where(p => p.CategoryId == category.Id);

            return View(await fashionsByCategory.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
