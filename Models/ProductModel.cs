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
    public string Cate { get; set; }
    public string SellerId { get; set; }
    public AppUserModel Seller { get; set; }
    public CategoryModel Category { get; set; }
    public BrandModel Brand { get; set; }

    [NotMapped]
    [FileExtension]
    public IFormFile ImageUpLoad { get; set; }
}
