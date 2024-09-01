using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;

        public CartController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Cart()
        {
            CartItemViewModel cartVM = HttpContext.Session.GetJson<CartItemViewModel>("CartViewModel");

            if (cartVM == null)
            {
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                cartVM = new CartItemViewModel
                {
                    CartItems = cartItems,
                    GrandTotal = cartItems.Sum(x => x.Quantity * x.Price)
                };
            }

            return View(cartVM);
        }
        public IActionResult GetCartItemCount()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            int itemCount = cartItems.Sum(x => x.Quantity);

            return Json(itemCount);
        }
        public IActionResult Checkout()
        {
            return View();
        }

        public async Task<IActionResult> Add(int Id, int quantity = 1) // Đặt giá trị mặc định là 1
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            // Kiểm tra xem sản phẩm có tồn tại không
            if (product == null)
            {
                TempData["error"] = "Sản phẩm không tồn tại.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // Lấy giỏ hàng từ session hoặc khởi tạo mới nếu chưa có
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Tìm sản phẩm trong giỏ hàng
            CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

            // Nếu sản phẩm chưa có trong giỏ hàng, thêm mới; nếu đã có thì cập nhật số lượng
            if (cartItem == null)
            {
                cart.Add(new CartItemModel(product) { Quantity = quantity });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            // Lưu giỏ hàng vào session
            HttpContext.Session.SetJson("Cart", cart);

            TempData["success"] = "Thêm vào giỏ hàng thành công";
            return Redirect(Request.Headers["Referer"].ToString());
        }
        public IActionResult Remove(int Id)
        {
            CartItemViewModel cartVM = HttpContext.Session.GetJson<CartItemViewModel>("CartViewModel");
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Xóa sản phẩm khỏi cả CartViewModel và danh sách CartItemModel
            if (cartVM != null)
            {
                cartVM.CartItems.RemoveAll(c => c.ProductId == Id);
                cart.RemoveAll(c => c.ProductId == Id);

                cartVM.GrandTotal = cartVM.CartItems.Sum(x => x.Quantity * x.Price) - (cartVM.DiscountAmount ?? 0m);

                if (cartVM.CartItems.Count == 0)
                {
                    HttpContext.Session.Remove("CartViewModel");
                    HttpContext.Session.Remove("AppliedVoucher");
                    HttpContext.Session.Remove("Cart");
                }
                else
                {
                    HttpContext.Session.SetJson("CartViewModel", cartVM);
                    HttpContext.Session.SetJson("Cart", cart);
                }
            }
            else
            {
                cart.RemoveAll(c => c.ProductId == Id);

                if (cart.Count == 0)
                {
                    HttpContext.Session.Remove("Cart");
                }
                else
                {
                    HttpContext.Session.SetJson("Cart", cart);
                }
            }

            TempData["success"] = "Đã xóa sản phẩm thành công";
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult Update(Dictionary<int, int> Quantities)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            foreach (var item in Quantities)
            {
                CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == item.Key);
                if (cartItem != null)
                {
                    cartItem.Quantity = item.Value;
                }
            }

            HttpContext.Session.SetJson("Cart", cart);
            TempData["success"] = "Cập nhật giỏ hàng thành công";
            return RedirectToAction("Cart");
        }
        [HttpPost]
        public async Task<IActionResult> ApplyVoucher(string voucherCode)
        {
            var voucher = await _dataContext.Vouchers.FirstOrDefaultAsync(v => v.Code == voucherCode && v.IsActive);

            if (voucher == null || voucher.ExpiryDate < DateTime.Now)
            {
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var voucherUsed = await _dataContext.Orders
                .AnyAsync(o => o.VoucherCode == voucherCode && o.UserId == userId);

            if (voucherUsed)
            {
                return Json(new { success = false, message = "Bạn đã sử dụng mã giảm giá này." });
            }

            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            decimal subtotal = cartItems.Sum(x => x.Quantity * x.Price);

            decimal discountAmount = 0;
            if (voucher.DiscountPercentage.HasValue)
            {
                discountAmount = subtotal * (decimal)(voucher.DiscountPercentage.Value / 100);
            }
            else if (voucher.DiscountAmount.HasValue)
            {
                discountAmount = voucher.DiscountAmount.Value;
            }

            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = subtotal - discountAmount,
                AppliedVoucherCode = voucherCode,
                DiscountAmount = discountAmount
            };

            HttpContext.Session.SetJson("AppliedVoucher", voucher);
            HttpContext.Session.SetJson("CartViewModel", cartVM);
            HttpContext.Session.SetJson("Cart", cartItems);

            return Json(new { success = true });
        }



        public IActionResult RemoveVoucher()
        {
            CartItemViewModel cartVM = HttpContext.Session.GetJson<CartItemViewModel>("CartViewModel");
            if (cartVM != null)
            {
                cartVM.AppliedVoucherCode = null;
                cartVM.DiscountAmount = 0m;
                cartVM.GrandTotal = cartVM.CartItems.Sum(x => x.Quantity * x.Price);

                HttpContext.Session.SetJson("CartViewModel", cartVM);
            }

            HttpContext.Session.Remove("AppliedVoucher");

            TempData["success"] = "Đã gỡ mã giảm giá thành công";
            return RedirectToAction("Cart");
        }
        public async Task<IActionResult> GetAvailableVouchers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách các voucher chưa hết hạn và chưa được người dùng hiện tại sử dụng
            var vouchers = await _dataContext.Vouchers
                .Where(v => v.IsActive && v.ExpiryDate >= DateTime.Now)
                .Select(v => new
                {
                    v.Code,
                    v.Description
                })
                .ToListAsync();

            return Json(vouchers);
        }

    }
}
