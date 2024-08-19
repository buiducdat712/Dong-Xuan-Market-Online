using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dong_Xuan_Market_Online.Models
{
    public class CategorySubModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên danh mục con")]
        public string Name { get; set; }

        public string Slug { get; set; }

        public int Status { get; set; }

        [ForeignKey("CategoryModel")]
        public int CategoryId { get; set; }

        public CategoryModel Category { get; set; }

        public ICollection<ProductModel> Products { get; set; } = new HashSet<ProductModel>();
    }
}