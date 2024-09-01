using Dong_Xuan_Market_Online.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Dong_Xuan_Market_Online.Repository;
using Dong_Xuan_Market_Online.Models.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Dong_Xuan_Market_Online.Hubs;

namespace Dong_Xuan_Market_Online.Controllers
{
    public class ChatController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly DataContext _dataContext;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(UserManager<AppUserModel> userManager, DataContext dataContext, IHubContext<ChatHub> hubContext, ILogger<ChatController> logger)
        {
            _userManager = userManager;
            _dataContext = dataContext;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var friends = await GetFriendsList(userId);

            ViewBag.FriendsList = friends;

            return View();
        }

        private async Task<List<AppUserModel>> GetFriendsList(string userId)
        {
            return await _dataContext.Friendships
                .Where(f => f.UserId == userId)
                .Select(f => f.Friend)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> GetFriendsList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var friends = await GetFriendsList(userId);
            var friendsList = friends.Select(friend => new {
                Id = friend.Id,
                UserName = friend.UserName
            }).ToList();

            return Json(friendsList);
        }

        [HttpGet]
        public async Task<IActionResult> ChatWith(string friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var messages = await _dataContext.Messages
                    .Where(m => (m.SenderId == userId && m.ReceiverId == friendId) ||
                                (m.SenderId == friendId && m.ReceiverId == userId))
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                var friend = await _userManager.FindByIdAsync(friendId);
                if (friend == null)
                {
                    _logger.LogWarning($"Friend with ID {friendId} not found");
                    return NotFound();
                }

                var viewModel = new ChatViewModel
                {
                    Friend = friend,
                    Messages = messages
                };

                return PartialView("_ChatPartial", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching chat history");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

         
    }
}