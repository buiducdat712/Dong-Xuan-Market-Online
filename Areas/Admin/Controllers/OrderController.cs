using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Order")]
    [Authorize(Roles = "Admin,Developer,Seller")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        public OrderController(DataContext context)
        {
            _dataContext = context;
        }
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            // Lấy tổng số đơn hàng từ cơ sở dữ liệu
            var query = _dataContext.Orders.OrderByDescending(p => p.Id);

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
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
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
                return BadRequest(new { success = false, message = "Order code is required." });
            }

            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound(new { success = false, message = "Order not found." });
            }

            try
            {
                _dataContext.Orders.Remove(order);
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error: " + ex.Message });
            }
        }


    }
}
