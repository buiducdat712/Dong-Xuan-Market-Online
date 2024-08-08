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
            return View(new LoginViewModel { ReturnUrl = returnUrl});
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
        


        public async Task<IActionResult> Logout(string returnUrl ="/")
        {
            await _singInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
    }
}
