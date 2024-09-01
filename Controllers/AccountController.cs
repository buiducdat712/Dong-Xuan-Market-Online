using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _singInManager;
        private readonly DataContext _dataContext;
        public AccountController(SignInManager<AppUserModel> singInManager, UserManager<AppUserModel> userManager, DataContext dataContext)
        {
            _singInManager = singInManager;
            _userManager = userManager;
            _dataContext = dataContext;
        }
        public IActionResult Login(string returnUrl)
        {
            // Nếu ReturnUrl là null hoặc trống, thiết lập một giá trị mặc định
            returnUrl = returnUrl ?? "/";
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _singInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password,false,false);
                if (result.Succeeded)
                {
                    return Redirect(loginVM.ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng");
            }
            return View(loginVM);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel { UserName = user.UserName, Email = user.Email };
                IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    // Gán vai trò "Customer" cho người dùng mới
                    await _userManager.AddToRoleAsync(newUser, "Customer");

                    TempData["success"] = "Tạo tài khoản thành công";
                    return RedirectToAction("Index", "User", new { area = "Admin" }); // Điều hướng lại IndexWithRoles trong UserController thuộc area Admin
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            var roleId = user.RoleId;
            var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            var roleName = role?.Name ?? "Không có vai trò";

            var model = new UserProfileViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Occupation = roleName,
                AvatarUrl = user.AvatarUrl,
                BirthDate = user.BirthDate,
                Address = user.Address
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model, IFormFile Avatar)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Occupation = model.Occupation;
                user.Address = model.Address;
                user.BirthDate = model.BirthDate;

                // Handle avatar upload
                if (Avatar != null && Avatar.Length > 0)
                {
                    // Delete old avatar if it exists
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Upload new avatar
                    var fileName = Path.GetFileName(Avatar.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images/user", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Avatar.CopyToAsync(stream);
                    }

                    user.AvatarUrl = $"/images/user/{fileName}";
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout(string returnUrl ="/")
        {
            await _singInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
    }
}
