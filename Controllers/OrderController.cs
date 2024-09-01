using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Dong_Xuan_Market_Online.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IOrderService _orderService;

        public OrderController(DataContext dataContext, UserManager<AppUserModel> userManager, IOrderService orderService)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _orderService = orderService;
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
                    .ThenInclude(p => p.Seller)  // Bao gồm người bán thông qua ProductModel
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);  // Truyền danh sách OrderDetails vào view
        }


        // Action để hoàn tất đơn hàng
        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var order = await _dataContext.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            try
            {
                await _orderService.ProcessOrder(order);

                return RedirectToAction("OrderSuccess");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("DeleteOrder")]
        public async Task<IActionResult> DeleteOrder(string ordercode)
        {
            if (string.IsNullOrEmpty(ordercode))
            {
                return BadRequest(new { success = false, message = "Mã đơn hàng là bắt buộc." });
            }

            var order = await _dataContext.Orders
                                          .Include(o => o.OrderDetails) // Include OrderDetails để kiểm tra liên kết
                                          .Where(o => o.OrderCode == ordercode)
                                          .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new { success = false, message = "Đơn hàng không tìm thấy." });
            }

            try
            {
                _dataContext.OrderDetails.RemoveRange(order.OrderDetails); // Remove related order details first
                _dataContext.Orders.Remove(order);
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Đơn hàng đã được xóa thành công." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật cơ sở dữ liệu: " + dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
