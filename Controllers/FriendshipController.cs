using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class FriendshipController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly DataContext _dataContext;

        public FriendshipController(UserManager<AppUserModel> userManager, DataContext dataContext)
        {
            _userManager = userManager;
            _dataContext = dataContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(string friendUserName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var friend = await _userManager.FindByNameAsync(friendUserName);

            if (friend == null || user == null)
            {
                // Xử lý lỗi nếu người dùng hoặc bạn bè không tồn tại
                return RedirectToAction("Index", "Home");
            }

            var friendship = new FriendshipModel
            {
                UserId = user.Id,
                FriendId = friend.Id
            };

            _dataContext.Friendships.Add(friendship);
            await _dataContext.SaveChangesAsync();

            // Redirect hoặc cập nhật danh sách bạn bè trong _Layout.cshtml
            return RedirectToAction("Index", "Home");
        }
    }
}
