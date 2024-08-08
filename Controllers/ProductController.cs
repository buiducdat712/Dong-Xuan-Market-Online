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

            var totalProducts = await _dataContext.Products.CountAsync(p => p.Category.Slug == "thoi-trang");
            var products = await _dataContext.Products
                .Where(p => p.Category.Slug == "thoi-trang")
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
                SidebarProducts = await _dataContext.Products.Where(p => p.Category.Slug == "thoi-trang").ToListAsync()
            };

            return View(viewModel);
        }   
        public async Task<IActionResult> IndexHouse()
        {
            var products = await _dataContext.Products.Where(p => p.Category.Slug == "do-gia-dung").ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> IndexDevice()
        {
            var products = await _dataContext.Products.Where(p => p.Category.Slug == "do-dien-tu").ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> IndexWithCate(string cate, int pg = 1)
        {
            int pageSize = 12;

            var totalProducts = await _dataContext.Products.CountAsync(p => p.Cate == cate);
            var products = await _dataContext.Products
                .Where(p => p.Cate == cate)
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
                SidebarProducts = await _dataContext.Products.Where(p => p.Cate == cate).ToListAsync(),
                SelectedCate = cate // Cập nhật thuộc tính này
            };

            return View(viewModel);
        }


        public async Task<IActionResult> DetailsFashion(int id)
        {
            var fashion = await _dataContext.Products
                .Include(f => f.Category)
                .Include(f => f.Brand)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fashion == null)
            {
                return NotFound();
            }

            return View(fashion);
        }
        public async Task<IActionResult> DetailsHouse(int id)
        {
            var house = await _dataContext.Products
                .Include(f => f.Category)
                .Include(f => f.Brand)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (house == null)
            {
                return NotFound();
            }

            return View(house);
        }
        public async Task<IActionResult> DetailsDevice(int id)
        {
            var device = await _dataContext.Products
                .Include(f => f.Category)
                .Include(f => f.Brand)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }
    }
}
