using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dong_Xuan_Market_Online.Models
{
    public class ProductImages
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public bool IsDefault { get; set; } = false;
        public ProductModel Product { get; set; }
    }
}
