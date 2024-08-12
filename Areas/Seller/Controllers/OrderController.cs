using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Dong_Xuan_Market_Online.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Route("Seller/Order")]
    [Authorize(Roles = "Seller")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderController(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = context;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            // Lấy thông tin người bán hiện tại
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy tổng số đơn hàng từ cơ sở dữ liệu
            var query = _dataContext.Orders
                .Where(o => o.SellerId == userId)
                .OrderByDescending(p => p.Id);

            const int pageSize = 10; // Số lượng đơn hàng trên mỗi trang

            if (pg < 1) // Nếu trang nhỏ hơn 1, gán giá trị trang là 1
            {
                pg = 1;
            }

            int recsCount = await query.CountAsync(); // Tổng số đơn hàng

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; // Số lượng đơn hàng bỏ qua

            // Lấy danh sách đơn hàng cho trang hiện tại
            var data = await query.Skip(recSkip).Take(pager.PageSize).ToListAsync();

            ViewBag.Pager = pager;

            return View(data);
        }

        [Route("ViewOrder")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            if (string.IsNullOrEmpty(ordercode))
            {
                return BadRequest("Order code is required.");
            }

            // Lấy ID của người bán hiện tại
            var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            // Lấy đơn hàng dựa trên mã đơn hàng và ID của người bán
            var order = await _dataContext.Orders
                                          .Where(o => o.OrderCode == ordercode && o.SellerId == sellerId)
                                          .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound("Order not found or you are not authorized to view this order.");
            }

            // Lấy danh sách chi tiết đơn hàng cho mã đơn hàng cụ thể
            var detailsOrder = await _dataContext.OrderDetails
                                                 .Include(od => od.Product)
                                                 .Where(od => od.OrderCode == ordercode)
                                                 .ToListAsync();

            return View(detailsOrder);
        }


        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            if (string.IsNullOrEmpty(ordercode))
            {
                return BadRequest(new { success = false, message = "Order code is required." });
            }

            var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var order = await _dataContext.Orders
                                          .Where(o => o.OrderCode == ordercode && o.SellerId == sellerId)
                                          .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new { success = false, message = "Order not found or you are not authorized to update this order." });
            }
            order.Status = status;

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
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
