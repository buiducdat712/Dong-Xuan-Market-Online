using Diacritics.Extensions;
using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly UserManager<AppUserModel> _userManager;

        public ProductController(DataContext dataContext, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager)
        {
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
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
            return View();
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
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

                // Liên kết sản phẩm với người bán
                var sellerId = _userManager.GetUserId(User);
                product.SellerId = sellerId;

                if (product.ImageUpLoad != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/product");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpLoad.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpLoad.CopyToAsync(fs);
                    }
                    product.Image = imageName;
                }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Lỗi nhập dữ liệu, Vui lòng kiểm tra lại";
            return View(product);
        }
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var sellerId = _userManager.GetUserId(User); // Lấy ID của người bán hiện tại
            var product = await _dataContext.Products
                                            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            return View(product);
        }

        [Route("Edit/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductModel product)
        {
            var sellerId = _userManager.GetUserId(User); // Lấy ID của người bán hiện tại
            var oldProduct = await _dataContext.Products
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);
            if (oldProduct == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                product.Slug = GenerateSlug(product.Name);

                var existingProduct = await _dataContext.Products
                                                        .AsNoTracking()
                                                        .FirstOrDefaultAsync(p => p.Slug == product.Slug && p.Id != id);
                if (existingProduct != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã có trong DataBase");
                    return View(product);
                }

                if (product.ImageUpLoad != null)
                {
                    if (!string.Equals(oldProduct.Image, "noimage.jpg"))
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/product");
                        string oldfileImage = Path.Combine(uploadsDir, oldProduct.Image);
                        if (System.IO.File.Exists(oldfileImage))
                        {
                            System.IO.File.Delete(oldfileImage);
                        }
                    }

                    string newUploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/product");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpLoad.FileName;
                    string filePath = Path.Combine(newUploadsDir, imageName);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpLoad.CopyToAsync(fs);
                    }
                    product.Image = imageName;
                }
                else
                {
                    product.Image = oldProduct.Image;
                }

                product.SellerId = sellerId; // Đảm bảo ID người bán không thay đổi
                _dataContext.Update(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Lỗi nhập dữ liệu, Vui lòng kiểm tra lại";
            return View(product);
        }

        [Route("Delete/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sellerId = _userManager.GetUserId(User); // Lấy ID của người bán hiện tại
            var product = await _dataContext.Products
                                            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == sellerId);

            if (product == null)
            {
                return NotFound();
            }

            if (!string.Equals(product.Image, "noimage.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "assets/images/product");
                string oldfileImage = Path.Combine(uploadsDir, product.Image);
                if (System.IO.File.Exists(oldfileImage))
                {
                    System.IO.File.Delete(oldfileImage);
                }
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản phẩm đã bị xóa";
            return RedirectToAction("Index");
        }


    }
}
