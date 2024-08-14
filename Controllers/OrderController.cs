using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;

        public OrderController(DataContext dataContext, UserManager<AppUserModel> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
        }

        // Hiển thị danh sách đơn hàng của người dùng
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _dataContext.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();


            if (orders == null || !orders.Any())
            {
                // Thêm log hoặc thông báo để kiểm tra xem có đơn hàng nào không
                Console.WriteLine("Không có đơn hàng nào.");
            }

            return View(orders);
        }

        public async Task<IActionResult> ViewOrder(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _dataContext.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)  // Bao gồm sản phẩm
                .ThenInclude(p => p.Seller)  // Bao gồm người bán
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order.OrderDetails);  // Truyền danh sách OrderDetails vào view
        }


    }
}
