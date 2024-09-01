using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
using Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Product")]
    [Authorize(Roles = "Admin")]
    public class VoucherController : Controller
    {
        private readonly DataContext _context;

        public VoucherController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var vouchers = await _context.Vouchers.ToListAsync();
            return View(vouchers);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(VoucherViewModel model)
        {
            if (ModelState.IsValid)
            {
                var voucher = new VoucherModel
                {
                    Code = model.Code,
                    DiscountPercentage = model.DiscountPercentage,
                    DiscountAmount = model.DiscountAmount,
                    ExpiryDate = model.ExpiryDate,
                    IsActive = model.IsActive
                };

                _context.Vouchers.Add(voucher);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }

}
