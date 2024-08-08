using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dong_Xuan_Market_Online.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Route("Seller/Trangchu")]
    [Authorize(Roles = "Seller")]
    public class TrangchuController : Controller
    {
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
