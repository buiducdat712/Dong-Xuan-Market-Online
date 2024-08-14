using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

public class ProductModel
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
    public string Description { get; set; }

    public decimal Price { get; set; }
    [NotMapped]
    public decimal DiscountedPrice
    {
        get
        {
            if (DiscountPercentage.HasValue && DiscountPercentage.Value > 0)
            {
                return Price - (Price * (decimal)DiscountPercentage.Value / 100);
            }
            return Price;
        }
    }

    [Range(0, 100, ErrorMessage = "Tỷ lệ giảm giá phải nằm trong khoảng từ 0 đến 100")]
    public double? DiscountPercentage { get; set; } // Nullable
    public string Slug { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public string Size { get; set; }
    public string Color { get; set; }
    public int StockQuantity { get; set; }
    public string Dimensions { get; set; } // Ví dụ: "30x30x30 cm"
    public double Weight { get; set; } // Ví dụ: 2.5 kg
    public string Material { get; set; } // Ví dụ: "Plastic", "Metal", etc.
    public string Specifications { get; set; }
    public string Image { get; set; } = "noimage.jpg";
    public bool Express { get; set; }
    public bool Save { get; set; }
    public bool Fast { get; set; }
    public bool IsApproved { get; set; } = false;
    public string Cate { get; set; }
    public string SellerId { get; set; }
    public AppUserModel Seller { get; set; }
    public CategoryModel Category { get; set; }
    public BrandModel Brand { get; set; }

    [NotMapped]
    [FileExtension]
    public IFormFile ImageUpLoad { get; set; }

}
