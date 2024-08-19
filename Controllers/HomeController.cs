using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ProductController> _logger;

        public HomeController(DataContext dataContext, ILogger<ProductController> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _dataContext.Products
                .Include(p => p.Ratings)
                .ToListAsync();

            var viewModel = products.Select(product => new ProductDetailsViewModel
            {
                Product =product,
                Ratings = product.Ratings?.ToList() ?? new List<RatingModel>(),

            }).ToList();

            return View(viewModel);
        }
        public IActionResult details()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
