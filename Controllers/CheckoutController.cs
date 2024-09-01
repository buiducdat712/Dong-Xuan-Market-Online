using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
using Dong_Xuan_Market_Online.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IOrderService _orderService;

        public CheckoutController(DataContext context, UserManager<AppUserModel> userManager, IOrderService orderService)
        {
            _dataContext = context;
            _userManager = userManager;
            _orderService = orderService;
        }

        public async Task<IActionResult> Checkout()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = _userManager.GetUserId(User);
            var cartVM = HttpContext.Session.GetJson<CartItemViewModel>("CartViewModel");

            if (cartVM == null || !cartVM.CartItems.Any())
            {
                TempData["error"] = "Giỏ hàng của bạn rỗng.";
                return RedirectToAction("Cart", "Cart");
            }

            var groupedItems = cartVM.CartItems.GroupBy(item => item.SellerId);
            foreach (var group in groupedItems)
            {
                var orderCode = Guid.NewGuid().ToString();
                var sellerSubtotal = group.Sum(item => item.Price * item.Quantity);
                var sellerDiscountAmount = (sellerSubtotal / cartVM.CartItems.Sum(item => item.Price * item.Quantity)) * cartVM.DiscountAmount;

                var sellerOrder = new OrderModel
                {
                    OrderCode = orderCode,
                    UserId = userId,
                    UserName = userEmail,
                    CreatedDate = DateTime.Now,
                    Status = 1,
                    SellerId = group.Key,
                    Subtotal = sellerSubtotal,
                    DiscountAmount = sellerDiscountAmount ?? 0m,
                    VoucherCode = cartVM.AppliedVoucherCode,
                    GrandTotal = sellerSubtotal - sellerDiscountAmount ?? 0m,
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

                try
                {
                    await _orderService.ProcessOrder(sellerOrder);
                }
                catch (Exception ex)
                {
                    TempData["error"] = ex.Message;
                    return RedirectToAction("Cart", "Cart");
                }
            }

            await _dataContext.SaveChangesAsync();

            // Clear the cart and applied voucher
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("CartViewModel");
            HttpContext.Session.Remove("AppliedVoucher");

            TempData["success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng";
            return RedirectToAction("Index", "Order");
        }
    }
}