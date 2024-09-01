namespace Dong_Xuan_Market_Online.Models
{
    public class FriendshipModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FriendId { get; set; }

        public AppUserModel User { get; set; }
        public AppUserModel Friend { get; set; }
    }
}
