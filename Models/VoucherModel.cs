using System.ComponentModel.DataAnnotations;

namespace Dong_Xuan_Market_Online.Models
{
    public class VoucherModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mã voucher")]
        public string Code { get; set; }
        public string Description { get; set; }

        [Range(0, 100, ErrorMessage = "Tỷ lệ giảm giá phải nằm trong khoảng từ 0 đến 100")]
        public double? DiscountPercentage { get; set; }

        public decimal? DiscountAmount { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập ngày hết hạn")]
        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
        public ProductModel Product { get; set; }
    }
}
