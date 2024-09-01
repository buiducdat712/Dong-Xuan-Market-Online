using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ProductController> _logger;

        public ProductController(DataContext dataContext, ILogger<ProductController> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IActionResult> IndexFashion(string slug, int pg = 1)
        {
            int pageSize = 12;
            // Retrieve the category based on slug
            var category = await _dataContext.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug);
            var totalProducts = await _dataContext.Products.CountAsync(p => p.Category.Slug == "thoi-trang" && p.IsApproved);
            var products = await _dataContext.Products
                .Where(p => p.Category.Slug == "thoi-trang" && p.IsApproved)
                .Skip((pg - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginateModel = new Paginate(totalProducts, pg, pageSize)
            {
                ActionName = "IndexFashion"
            };

            var viewModel = new ProductPaginateViewModel
            {
                Products = products,
                Paginate = paginateModel,
                SidebarProducts = await _dataContext.Products.Where(p => p.Category.Slug == "thoi-trang" && p.IsApproved).ToListAsync()
            };

            return View(viewModel);
        }
        public async Task<IActionResult> IndexHouse()
        {
            var products = await _dataContext.Products
                .Where(p => p.Category.Slug == "do-gia-dung" && p.IsApproved)
                .ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> IndexDevice()
        {
            var slug = "do-dien-tu"; // Define the slug for "do-dien-tu" category
            var products = await _dataContext.Products
                .Where(p => p.Category.Slug == slug && p.IsApproved)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> IndexWithCate(string slug, int pg = 1)
        {
            int pageSize = 12;

            // Lấy danh mục phụ dựa trên slug
            var categorySub = await _dataContext.CategorySubModels
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (categorySub == null)
            {
                // Xử lý trường hợp không tìm thấy categorySub
                return NotFound();
            }

            // Lấy tổng số sản phẩm cho categorySub
            var totalProducts = await _dataContext.Products.CountAsync(p => p.CategorySubId == categorySub.Id && p.IsApproved);

            // Lấy danh sách sản phẩm cho categorySub
            var products = await _dataContext.Products
                .Where(p => p.CategorySubId == categorySub.Id && p.IsApproved)
                .Skip((pg - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tạo model phân trang
            var paginateModel = new Paginate(totalProducts, pg, pageSize)
            {
                ActionName = "IndexWithCate"
            };

            // Tạo view model
            var viewModel = new ProductPaginateViewModel
            {
                Products = products,
                Paginate = paginateModel,
                SidebarProducts = await _dataContext.Products.Where(p => p.CategorySubId == categorySub.Id && p.IsApproved).ToListAsync(),
                SelectedCate = categorySub.Name
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
                .Where(p=>p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .ToListAsync();
            ViewBag.Keyword = searchTerm;
            return View(products);
        }
        public async Task<IActionResult> ProductDetails(string slug)
        {
            var product = await _dataContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Brand)
                .Include(p => p.Ratings) // Bao gồm Ratings
                    .ThenInclude(r => r.User) // Bao gồm User để có thể truy cập Email
                 .FirstOrDefaultAsync(p => p.Slug == slug);

            if (product == null)
            {
                return NotFound();
            }

            var sortedImages = product.ProductImages.OrderByDescending(img => img.IsDefault).ToList();
            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsApproved)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts ?? new List<ProductModel>();
            ViewBag.ActionName = product.Category?.Name switch
            {
                "Đồ Gia Dụng" => "IndexHouse",
                "Thời Trang" => "IndexFashion",
                "Đồ Điện Tử" => "IndexDevice",
                _ => "Index"
            };

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                SortedImages = sortedImages,
                Ratings = product.Ratings?.ToList() ?? new List<RatingModel>(), // Đảm bảo Ratings không null
                NewRating = new RatingModel() // Khởi tạo mô hình đánh giá mới
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitReview(ProductDetailsViewModel viewModel, string slug)
        {
            _logger.LogInformation($"Received review submission for product with slug {slug}");

            if (string.IsNullOrEmpty(slug))
            {
                _logger.LogWarning("Invalid product slug");
                return BadRequest("Invalid product slug");
            }

            var product = await _dataContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (product == null)
            {
                _logger.LogWarning($"Product not found for slug: {slug}");
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated");
                return Unauthorized("User not authenticated");
            }

            // Kiểm tra nếu người dùng đã mua sản phẩm này
            var hasPurchased = await _dataContext.Orders
                .AnyAsync(o => o.UserId == userId && o.OrderDetails.Any(oi => oi.ProductId == product.Id));

            if (!hasPurchased)
            {
                _logger.LogWarning("User has not purchased this product");
                TempData["error"] = "Bạn chỉ có thể đánh giá sản phẩm mà bạn đã mua.";
                return RedirectToAction("ProductDetails", new { slug = product.Slug });
            }

            var ratingModel = viewModel.NewRating;
            ratingModel.ProductId = product.Id;
            ratingModel.UserId = userId;

            // Xóa lỗi UserId nếu có
            ModelState.Remove("NewRating.UserId");

            _logger.LogInformation($"Rating details: ProductId={ratingModel.ProductId}, UserId={ratingModel.UserId}, Rating={ratingModel.Rating}, Comment={ratingModel.Comment}");

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("ModelState is valid, attempting to add rating");
                    _dataContext.Ratings.Add(ratingModel);
                    var result = await _dataContext.SaveChangesAsync();
                    _logger.LogInformation($"SaveChanges result: {result}");
                    if (result > 0)
                    {
                        _logger.LogInformation("Rating added successfully");
                        TempData["success"] = "Đánh giá của bạn đã được gửi thành công!";
                        return RedirectToAction("ProductDetails", new { slug = product.Slug });
                    }
                    else
                    {
                        _logger.LogWarning("SaveChanges returned 0");
                        ModelState.AddModelError("", "Không thể lưu đánh giá. Vui lòng thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving rating: {ex.Message}");
                    ModelState.AddModelError("", $"Lỗi khi lưu đánh giá: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("ModelState is invalid");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning($"ModelState error: {error.ErrorMessage}");
                    }
                }
            }

            viewModel.Product = product;
            viewModel.SortedImages = product.ProductImages?.OrderByDescending(img => img.IsDefault).ToList() ?? new List<ProductImages>();
            viewModel.RelatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsApproved)
                .OrderBy(p => p.Id)
                .Take(4)
                .ToListAsync();
            viewModel.ActionName = product.Category?.Name switch
            {
                "Đồ Gia Dụng" => "IndexHouse",
                "Thời Trang" => "IndexFashion",
                "Đồ Điện Tử" => "IndexDevice",
                _ => "Index"
            };
            viewModel.Ratings = await _dataContext.Ratings
                .Where(r => r.ProductId == product.Id)
                .Include(r => r.User)
                .ToListAsync();

            return View("ProductDetails", viewModel);
        }

    }
}
