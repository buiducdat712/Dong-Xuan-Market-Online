namespace Dong_Xuan_Market_Online.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string SellerId { get; set; }

        // Khóa ngoại liên kết với AppUserModel
        public AppUserModel Seller { get; set; }

        // Các thuộc tính khác
        public ICollection<OrderDetails> OrderDetails { get; set; }
    }
}
