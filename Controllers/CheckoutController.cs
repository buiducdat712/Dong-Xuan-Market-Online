using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;

        public CheckoutController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Checkout()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách sản phẩm từ giỏ hàng
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            if (!cartItems.Any())
            {
                TempData["error"] = "Giỏ hàng của bạn rỗng.";
                return RedirectToAction("Cart", "Cart");
            }

            // Nhóm các mặt hàng theo SellerId
            var groupedItems = cartItems.GroupBy(item => item.SellerId);

            foreach (var group in groupedItems)
            {
                var orderCode = Guid.NewGuid().ToString();
                var sellerOrder = new OrderModel
                {
                    OrderCode = orderCode,
                    UserName = userEmail,
                    CreatedDate = DateTime.Now,
                    Status = 1, // Trạng thái mặc định
                    SellerId = group.Key,
                    OrderDetails = group.Select(item => new OrderDetails
                    {
                        ProductId = (int)item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        OrderCode = orderCode,
                        UserName = userEmail
                    }).ToList()
                };

                _dataContext.Orders.Add(sellerOrder);
            }

            await _dataContext.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");
            TempData["success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng";
            return RedirectToAction("Cart", "Cart");
        }
    }
}
