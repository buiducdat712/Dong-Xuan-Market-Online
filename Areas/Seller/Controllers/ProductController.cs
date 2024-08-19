using Diacritics.Extensions;
using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dong_Xuan_Market_Online.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Route("Seller/Product")]
    [Authorize(Roles = "Seller")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly ILogger<ProductController> _logger;

        public ProductController(DataContext dataContext, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager, ILogger<ProductController> logger)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _logger = logger;
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
            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }

            var sellerId = _userManager.GetUserId(User);

            int recsCount = await _dataContext.Products.CountAsync(p => p.SellerId == sellerId);

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var products = await _dataContext.Products
                                             .Where(p => p.SellerId == sellerId)
                                             .OrderByDescending(p => p.Id)
                                             .Include(p => p.Category)
                                             .Include(p => p.CategorySub)
                                             .Include(p => p.Brand)
                                             .Skip(recSkip)
                                             .Take(pager.PageSize)
                                             .ToListAsync();

            ViewBag.Pager = pager;

            return View(products);
        }
        [Route("Create")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            ViewBag.Categorysubs = new SelectList(_dataContext.CategorySubModels, "Id", "Name");
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product, List<IFormFile> ImageUpLoads, int DefaultImageIndex)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                product.Slug = GenerateSlug(product.Name);
                var slugExists = await _dataContext.Products.AnyAsync(p => p.Slug == product.Slug);
                if (slugExists)
                {
                    ModelState.AddModelError("", "Sản phẩm đã có trong DataBase");
                    return View(product);
                }

                var sellerId = _userManager.GetUserId(User);
                product.SellerId = sellerId;

                var productImages = new List<ProductImages>();
                string defaultImage = null;

                if (ImageUpLoads != null && ImageUpLoads.Count > 0)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/product");

                    for (int i = 0; i < ImageUpLoads.Count; i++)
                    {
                        var file = ImageUpLoads[i];
                        string imageName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fs);
                        }

                        var productImage = new ProductImages
                        {
                            ImageUrl = imageName,
                            IsDefault = (i == DefaultImageIndex) // Chỉ ảnh có chỉ số trùng với DefaultImageIndex được đánh dấu là mặc định
                        };

                        productImages.Add(productImage);

                        if (productImage.IsDefault)
                        {
                            defaultImage = imageName;
                        }
                    }
                }

                product.Image = defaultImage;

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();

                foreach (var image in productImages)
                {
                    image.ProductId = product.Id;
                    _dataContext.ProductImages.Add(image);
                }

                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Lỗi nhập dữ liệu, Vui lòng kiểm tra lại";
            return View(product);
        }


        public async Task<IActionResult> SetDefaultImage(int productId, int imageId)
        {
            var product = await _dataContext.Products
                                            .Include(p => p.ProductImages)
                                            .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            // Đặt tất cả các hình ảnh của sản phẩm này là không mặc định
            foreach (var image in product.ProductImages)
            {
                image.IsDefault = (image.Id == imageId);
            }

            // Lưu thay đổi
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Hình ảnh đại diện đã được cập nhật";
            return RedirectToAction("Index");
        }

        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var sellerId = _userManager.GetUserId(User); // Lấy ID của người bán hiện tại
            var product = await _dataContext.Products
                                            .Include(p => p.ProductImages)
                                            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Categorysubs = new SelectList(_dataContext.CategorySubModels, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            // Chuyển danh sách hình ảnh đến View
            var model = new ProductEditViewModel
            {
                Product = product,
                ProductImages = product.ProductImages.ToList()
            };

            return View(model);
        }
        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, ProductModel product, List<IFormFile> ImageUpLoads, int DefaultImageIndex, string RemovedImageIds)
        {
            var productToUpdate = await _dataContext.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == _userManager.GetUserId(User));

            if (productToUpdate == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Categorysubs = new SelectList(_dataContext.CategorySubModels, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin sản phẩm
                productToUpdate.Name = product.Name;
                productToUpdate.Description = product.Description;
                productToUpdate.Price = product.Price;
                productToUpdate.DiscountPercentage = product.DiscountPercentage;
                productToUpdate.CategoryId = product.CategoryId;
                productToUpdate.BrandId = product.BrandId;
                productToUpdate.Size = product.Size;
                productToUpdate.Color = product.Color;
                productToUpdate.StockQuantity = product.StockQuantity;
                productToUpdate.Dimensions = product.Dimensions;
                productToUpdate.Weight = product.Weight;
                productToUpdate.Material = product.Material;
                productToUpdate.Specifications = product.Specifications;
                productToUpdate.Express = product.Express;
                productToUpdate.Save = product.Save;
                productToUpdate.Fast = product.Fast;
                //productToUpdate.Cate = product.Cate;
                productToUpdate.CategorySubId = product.CategorySubId;
                productToUpdate.Slug = GenerateSlug(product.Name);

                // Kiểm tra trùng lặp slug
                var slugExists = await _dataContext.Products.AnyAsync(p => p.Slug == productToUpdate.Slug && p.Id != id);
                if (slugExists)
                {
                    ModelState.AddModelError("", "Sản phẩm đã có trong DataBase");
                    return View("Edit", product);
                }

                // Xử lý xóa ảnh
                /*if (!string.IsNullOrEmpty(RemovedImageIds))
                {
                    var removedIds = RemovedImageIds.Split(',').Select(int.Parse).ToList();
                    await RemoveImagesAsync(removedIds, productToUpdate.Id);
                }*/

                // Xử lý thêm ảnh mới
                string defaultImage = productToUpdate.Image;
                if (ImageUpLoads != null && ImageUpLoads.Count > 0)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/product");
                    for (int i = 0; i < ImageUpLoads.Count; i++)
                    {
                        var file = ImageUpLoads[i];
                        string imageName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fs);
                        }
                        var productImage = new ProductImages
                        {
                            ImageUrl = imageName,
                            IsDefault = (i == DefaultImageIndex)
                        };
                        productToUpdate.ProductImages.Add(productImage);
                        if (productImage.IsDefault)
                        {
                            defaultImage = imageName;
                        }
                    }
                }

                // Cập nhật ảnh mặc định nếu có thay đổi
                foreach (var image in productToUpdate.ProductImages)
                {
                    image.IsDefault = (productToUpdate.ProductImages.ToList().IndexOf(image) == DefaultImageIndex);
                    if (image.IsDefault)
                    {
                        defaultImage = image.ImageUrl;
                    }
                }

                productToUpdate.Image = defaultImage;

                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Lỗi nhập dữ liệu, Vui lòng kiểm tra lại";
            return View("Edit", product);
        }

        /*private async Task RemoveImagesAsync(List<int> removedIds, int productId)
        {
            var imagesToRemove = await _dataContext.ProductImages
                .Where(pi => removedIds.Contains(pi.Id) && pi.ProductId == productId)
                .ToListAsync();

            foreach (var image in imagesToRemove)
            {
                _dataContext.ProductImages.Remove(image);

                // Xóa file ảnh từ server
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/product", image.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _dataContext.SaveChangesAsync(); // Đảm bảo lưu thay đổi vào cơ sở dữ liệu
        }*/
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            // Tìm hình ảnh dựa trên ID
            var image = await _dataContext.ProductImages
                .FirstOrDefaultAsync(pi => pi.Id == id);

            if (image == null)
            {
                _logger.LogWarning("Image with ID {Id} not found", id);
                return NotFound();
            }

            // Xóa hình ảnh từ cơ sở dữ liệu
            _dataContext.ProductImages.Remove(image);

            // Xóa tệp hình ảnh từ hệ thống tập tin
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/product", image.ImageUrl);
            if (System.IO.File.Exists(imagePath))
            {
                try
                {
                    System.IO.File.Delete(imagePath);
                    _logger.LogInformation("Deleted image file: {ImagePath}", imagePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete image file: {ImagePath}", imagePath);
                    return StatusCode(500, "Failed to delete image file");
                }
            }

            await _dataContext.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted image with ID {Id}", id);

            return Json(new { success = true });
        }


        [Route("Delete/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sellerId = _userManager.GetUserId(User); // Lấy ID của người bán hiện tại
            var product = await _dataContext.Products
                                            .Include(p => p.ProductImages) // Include ProductImages để xóa hình ảnh liên quan
                                            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);

            if (product == null)
            {
                return NotFound();
            }

            // Xóa hình ảnh liên quan
            if (product.ProductImages.Any())
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/product");

                foreach (var image in product.ProductImages)
                {
                    if (!string.Equals(image.ImageUrl, "noimage.jpg"))
                    {
                        string filePath = Path.Combine(uploadsDir, image.ImageUrl);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    _dataContext.ProductImages.Remove(image); // Xóa bản ghi hình ảnh khỏi cơ sở dữ liệu
                }
            }

            // Xóa sản phẩm
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Sản phẩm và tất cả hình ảnh liên quan đã bị xóa";
            return RedirectToAction("Index");
        }



    }
}
