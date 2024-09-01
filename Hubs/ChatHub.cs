using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;
using Microsoft.AspNetCore.SignalR;

namespace Dong_Xuan_Market_Online.Hubs
{
    public class ChatHub : Hub
    {
        private readonly DataContext _context;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(DataContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SendMessage(string receiverId, string content)
        {
            var senderId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(senderId))
            {
                _logger.LogWarning("SendMessage attempted with null or empty senderId.");
                return;
            }

            var message = new MessageModel
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Message sent from {senderId} to {receiverId}");

            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, content, message.SentAt.ToString("HH:mm:ss"));
        }
    }
}