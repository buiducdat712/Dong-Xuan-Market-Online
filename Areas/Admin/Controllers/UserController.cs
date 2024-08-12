using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Dong_Xuan_Market_Online.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        public UserController(UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager, DataContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;
        }
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            // Số lượng bản ghi trên mỗi trang
            const int pageSize = 10;

            // Tạo truy vấn để lấy thông tin người dùng và vai trò
            var query = from u in _dataContext.Users
                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                        join r in _dataContext.Roles on ur.RoleId equals r.Id
                        select new { User = u, RoleName = r.Name };

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
            var usersWithRoles = await query.Skip(recSkip).Take(pager.PageSize).ToListAsync();

            // Đưa thông tin phân trang vào ViewBag
            ViewBag.Pager = pager;

            return View(usersWithRoles);
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin người dùng
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                // Lấy vai trò hiện tại của người dùng
                var currentRoles = await _userManager.GetRolesAsync(existingUser);

                // Xóa vai trò cũ của người dùng (nếu có)
                if (currentRoles.Any())
                {
                    var removeRoleResult = await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                    if (!removeRoleResult.Succeeded)
                    {
                        AddIdentityErrors(removeRoleResult);
                        TempData["error"] = "Có lỗi xảy ra khi xóa vai trò cũ của người dùng.";
                        return View(user);
                    }
                }

                // Thêm vai trò mới cho người dùng
                var role = await _roleManager.FindByIdAsync(user.RoleId);
                if (role != null)
                {
                    var addRoleResult = await _userManager.AddToRoleAsync(existingUser, role.Name);
                    if (!addRoleResult.Succeeded)
                    {
                        AddIdentityErrors(addRoleResult);
                        TempData["error"] = "Có lỗi xảy ra khi thêm vai trò mới cho người dùng.";
                        return View(user);
                    }
                }

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    TempData["error"] = "Có lỗi xảy ra khi cập nhật người dùng.";
                    return View(user);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            TempData["error"] = "Lỗi nhập dữ liệu, vui lòng kiểm tra lại.";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorsMessage = string.Join("\n", errors);
            TempData["modelErrors"] = errorsMessage;

            return View(user);
        }


        private void AddIdentityErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }


        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email);
                    var userId = createUser.Id;
                    var role = _roleManager.FindByIdAsync(user.RoleId);
                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in createUserResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }
            }
            else
            {
                TempData["error"] = "Lỗi nhập dữ liệu, Vui lòng kiểm tra lại";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return View("Error");
            }
            TempData["error"] = "Tài khoản đã bị xóa";
            return RedirectToAction("Index");
        }
    }
}
