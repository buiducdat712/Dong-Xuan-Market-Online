using System.ComponentModel.DataAnnotations;

namespace Dong_Xuan_Market_Online.Models.ViewModels
{
    public class VoucherViewModel
    {
        [Required(ErrorMessage = "Yêu cầu nhập mã voucher")]
        public string Code { get; set; }

        [Range(0, 100, ErrorMessage = "Tỷ lệ giảm giá phải nằm trong khoảng từ 0 đến 100")]
        public double? DiscountPercentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá phải lớn hơn hoặc bằng 0")]
        public decimal? DiscountAmount { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập ngày hết hạn")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
