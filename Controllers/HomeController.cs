using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ProductController> _logger;
        private readonly UserManager<AppUserModel> _userManager;
        public HomeController(DataContext dataContext, ILogger<ProductController> logger, UserManager<AppUserModel> userManager)
        {
            _dataContext = dataContext;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<AppUserModel> friends = new List<AppUserModel>();

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                friends = await GetFriendsList(user);
            }

            ViewBag.FriendsList = friends;

            var products = await _dataContext.Products
                .Include(p => p.Ratings)
                .ToListAsync();

            var productDetailsList = products.Select(product => new ProductDetailsViewModel
            {
                Product = product,
                Ratings = product.Ratings?.ToList() ?? new List<RatingModel>(),

            }).ToList();

            var homeImages = await _dataContext.HomeImages.ToListAsync();

            var viewModel = new IndexViewModel
            {
                ProductDetails = productDetailsList,
                SliderImages = await _dataContext.Sliders.ToListAsync() ?? new List<SliderModel>(),
                HomeImages = homeImages
            };

            return View(viewModel);
        }

        private async Task<List<AppUserModel>> GetFriendsList(AppUserModel user)
        {
            if (user == null)
            {
                return new List<AppUserModel>();
            }

            return await _dataContext.Friendships
                .Where(f => f.UserId == user.Id)
                .Select(f => f.Friend)
                .ToListAsync();
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
