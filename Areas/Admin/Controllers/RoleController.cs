using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Role")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(DataContext dataContext, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _dataContext = dataContext;
        }
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            // Số lượng bản ghi trên mỗi trang
            const int pageSize = 10;

            // Tạo truy vấn sắp xếp theo ID giảm dần
            var query = _dataContext.Roles.OrderByDescending(p => p.Id);

            // Kiểm tra và gán giá trị trang hiện tại nếu cần
            if (pg < 1)
            {
                pg = 1;
            }

            // Lấy tổng số bản ghi
            int recsCount = await query.CountAsync();

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Tính số bản ghi cần bỏ qua
            int recSkip = (pg - 1) * pageSize;

            // Lấy dữ liệu cho trang hiện tại
            var roles = await query.Skip(recSkip).Take(pager.PageSize).ToListAsync();

            // Đưa thông tin phân trang vào ViewBag
            ViewBag.Pager = pager;

            return View(roles);
        }


        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            if (!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }
            return Redirect("Index");
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var role = await _roleManager.FindByIdAsync(id);
            return View(role);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, IdentityRole model)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }
                role.Name = model.Name;
                try
                {
                    await _roleManager.UpdateAsync(role);
                    TempData["success"] = "Cập nhật role thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(" ", "Có lỗi khi update Role");
                }
                
            }
            return View(model ?? new IdentityRole { Id = id });

        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Role đã bị xóa";

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(" ", "Có lỗi khi xóa Role");
            }
            return Redirect("Index");
        }
    }
}
