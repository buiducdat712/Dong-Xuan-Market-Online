namespace Dong_Xuan_Market_Online.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }

        public AppUserModel Sender { get; set; }
        public AppUserModel Receiver { get; set; }
    }
}
