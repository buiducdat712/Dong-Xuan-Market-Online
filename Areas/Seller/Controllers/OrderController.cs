using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dong_Xuan_Market_Online.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Route("Seller/Order")]
    [Authorize(Roles = "Seller")]
    public class OrderController : Controller
    {
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
