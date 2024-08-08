using Diacritics.Extensions;
using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Route("Seller/Product")]
    [Authorize(Roles = "Seller")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }

        private string GenerateSlug(string name)
        {
            var normalizedString = name.RemoveDiacritics();
            var slug = normalizedString.Replace(" ", "-").ToLower();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");
            return slug;
        }
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10; // 10 items/trang

            if (pg < 1)
            {
                pg = 1; // page == 1
            }

            int recsCount = await _dataContext.Products.CountAsync(); // Đếm tổng số sản phẩm

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; // Số bản ghi cần bỏ qua

            var products = await _dataContext.Products
                                             .OrderByDescending(p => p.Id)
                                             .Include(p => p.Category)
                                             .Include(p => p.Brand)
                                             .Skip(recSkip) // Bỏ qua các bản ghi trước đó
                                             .Take(pager.PageSize) // Lấy số bản ghi cần thiết
                                             .ToListAsync();

            ViewBag.Pager = pager;

            return View(products);
        }
    }
}
