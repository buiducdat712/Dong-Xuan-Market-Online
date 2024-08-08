using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
            };
            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View();
        }

        public async Task<IActionResult> Add(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

            if (cartItem == null)
            {
                cart.Add(new CartItemModel(product));
            }
            else
            {
                cartItem.Quantity += 1;
            }

            HttpContext.Session.SetJson("Cart", cart);

            TempData["success"] = "Thêm vào giỏ hàng thành công";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult Remove(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
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
    }
}
