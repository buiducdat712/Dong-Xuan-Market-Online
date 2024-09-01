using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dong_Xuan_Market_Online.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserId { get; set; }  // Thêm thuộc tính UserId
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string SellerId { get; set; }
        [Required]
        public decimal Subtotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public string VoucherCode { get; set; }

        [Required]
        public decimal GrandTotal { get; set; }


        // Khóa ngoại liên kết với AppUserModel
        public AppUserModel Seller { get; set; }
        public AppUserModel User { get; set; }

        // Các thuộc tính khác
        public ICollection<OrderDetails> OrderDetails { get; set; }
        [NotMapped]
        public decimal TotalAfterDiscount => Subtotal - DiscountAmount;
    }
}
