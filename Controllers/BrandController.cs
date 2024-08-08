using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;

        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(string[] slugs = null)
        {
            if (slugs == null || !slugs.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            // Get the brands based on the provided slugs
            var brands = await _dataContext.Brands
                .Where(c => slugs.Contains(c.Slug))
                .ToListAsync();

            var brandIds = brands.Select(b => b.Id).ToList();

            var fashionsByBrand = _dataContext.Products
                .Where(p => brandIds.Contains(p.BrandId));

            return View(await fashionsByBrand.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
