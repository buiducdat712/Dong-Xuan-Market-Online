using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Models;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        public ProductController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IActionResult> IndexFashion(int pg = 1)
        {
            int pageSize = 12;

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
            var products = await _dataContext.Products
                .Where(p => p.Category.Slug == "do-dien-tu" && p.IsApproved)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> IndexWithCate(string cate, int pg = 1)
        {
            int pageSize = 12;

            var totalProducts = await _dataContext.Products.CountAsync(p => p.Cate == cate && p.IsApproved);
            var products = await _dataContext.Products
                .Where(p => p.Cate == cate && p.IsApproved)
                .Include(p => p.Category)
                .Skip((pg - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginateModel = new Paginate(totalProducts, pg, pageSize)
            {
                ActionName = "IndexWithCate"
            };

            var viewModel = new ProductPaginateViewModel
            {
                Products = products,
                Paginate = paginateModel,
                SidebarProducts = await _dataContext.Products.Where(p => p.Cate == cate && p.IsApproved).ToListAsync(),
                SelectedCate = cate
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
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _dataContext.Products
                .Include(d => d.Category)
                .Include(d => d.Brand)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsApproved);

            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsApproved)
                .Take(4)
                .ToListAsync();
            ViewBag.RelatedProducts = relatedProducts;

            string actionName = product.Category?.Name switch
            {
                "Đồ Gia Dụng" => "IndexHouse",
                "Thời Trang" => "IndexFashion",
                "Đồ Điện Tử" => "IndexDevice",
                _ => null
            };

            ViewBag.ActionName = actionName;

            return View(product);
        }



    }
}
