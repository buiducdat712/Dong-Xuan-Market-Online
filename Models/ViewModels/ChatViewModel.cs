using System.Collections.Generic;

namespace Dong_Xuan_Market_Online.Models.ViewModels
{
    public class ChatViewModel
    {
        public AppUserModel Friend { get; set; }
        public List<MessageModel> Messages { get; set; }
    }
}
