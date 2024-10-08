﻿using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

public class ProductModel
{
    public ProductModel()
    {
        this.ProductImages = new HashSet<ProductImages>();
        
    }
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
    [NotMapped]
    public decimal FinalPrice
    {
        get
        {
            decimal finalPrice = DiscountedPrice;

            if (Voucher != null && Voucher.IsActive && Voucher.ExpiryDate >= DateTime.Now)
            {
                if (Voucher.DiscountPercentage.HasValue)
                {
                    finalPrice -= finalPrice * (decimal)Voucher.DiscountPercentage.Value / 100;
                }
                else if (Voucher.DiscountAmount.HasValue)
                {
                    finalPrice -= Voucher.DiscountAmount.Value;
                }
            }

            return finalPrice < 0 ? 0 : finalPrice; // Giá cuối cùng không được nhỏ hơn 0
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
    public double? Weight { get; set; } // Ví dụ: 2.5 kg
    public string Material { get; set; } // Ví dụ: "Plastic", "Metal", etc.
    public string Specifications { get; set; }
    public string Image { get; set; } = "noimage.jpg";
    public bool Express { get; set; }
    public bool Save { get; set; }
    public bool Fast { get; set; }
    public bool IsApproved { get; set; } = false;
    //public string Cate { get; set; }
    public int? CategorySubId { get; set; }
    public CategorySubModel CategorySub { get; set; }
    public string SellerId { get; set; }
    public AppUserModel Seller { get; set; }
    public CategoryModel Category { get; set; }
    public BrandModel Brand { get; set; }
    public int? VoucherId { get; set; }
    public VoucherModel Voucher { get; set; }

    public ICollection<ProductImages> ProductImages { get; set; } 

    [NotMapped]
    [FileExtension]
    public List<IFormFile> ImageUpLoads { get; set; }
    public ICollection<RatingModel> Ratings { get; set; }
    public int? SoldQuantity { get; set; } // Số lượng đã bán
}
