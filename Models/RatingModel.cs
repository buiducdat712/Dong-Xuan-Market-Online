using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dong_Xuan_Market_Online.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập bình luận")]
        public string Comment { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đánh giá")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        public int Rating { get; set; }

        public virtual AppUserModel User { get; set; }
        public virtual ProductModel Product { get; set; }
    }
}
