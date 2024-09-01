    using Diacritics.Extensions;
    using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    namespace Dong_Xuan_Market_Online.Areas.Admin.Controllers
    {
        [Area("Admin")]
        [Route("Admin/Product")]
        [Authorize(Roles = "Admin,Developer")]
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
                                                 .Include(p => p.CategorySub)
                                                 .Include(p => p.Brand)
                                                 .Skip(recSkip) // Bỏ qua các bản ghi trước đó
                                                 .Take(pager.PageSize) // Lấy số bản ghi cần thiết
                                                 .ToListAsync();

                ViewBag.Pager = pager;

                return View(products);
            }


        [Route("Create")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Categorysubs = new SelectList(_dataContext.CategorySubModels, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        /*[Route("Create")]
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
        }*/

        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Categorysubs = new SelectList(_dataContext.CategorySubModels, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            return View(product);
        }
        [HttpGet]
        [Route("Details/{id:int}")]
        public IActionResult Details(int id)
        {
            var product = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.CategorySub)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductEditViewModel
            {
                Product = product,
                ProductImages = product.ProductImages != null ? product.ProductImages.ToList() : new List<ProductImages>()
            };

            return View(viewModel);
        }

        [Route("Delete/{id:int}")]
            public async Task<IActionResult> Delete(int id)
            {
                ProductModel product = await _dataContext.Products.FindAsync(id);

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
        [Route("ApproveProducts")]
        public async Task<IActionResult> ApproveProducts()
        {
            var products = await _dataContext.Products.Where(p => !p.IsApproved).ToListAsync();
            return View(products);
        }

        [HttpPost]
        [Route("ApproveProduct")]
        public async Task<IActionResult> ApproveProduct(int productId)
        {
            var product = await _dataContext.Products.FindAsync(productId);
            if (product != null)
            {
                product.IsApproved = true;
                await _dataContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Route("UnapproveProduct")]
        public async Task<IActionResult> UnapproveProduct(int productId)
        {
            var product = await _dataContext.Products.FindAsync(productId);
            if (product != null)
            {
                product.IsApproved = false;
                await _dataContext.SaveChangesAsync();
            }
            return RedirectToAction("Index"); // Hoặc hành động khác tùy theo nhu cầu
        }
    }
}
