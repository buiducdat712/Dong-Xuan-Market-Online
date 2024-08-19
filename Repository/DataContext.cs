﻿using Dong_Xuan_Market_Online.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Repository
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<CategorySubModel> CategorySubModels { get; set; } 
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<RatingModel> Ratings { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }  // Add this line

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thiết lập quan hệ giữa Product và Seller
            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Seller)  // Product có một người bán
                .WithMany(u => u.Products)  // Người bán có nhiều sản phẩm
                .HasForeignKey(p => p.SellerId)  // SellerId là khóa ngoại
                .OnDelete(DeleteBehavior.Restrict);  // Quy tắc xóa
            // CategoryModel
            modelBuilder.Entity<CategoryModel>()
                .HasMany(c => c.SubCategories)
                .WithOne(sc => sc.Category)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập quan hệ giữa Order và Seller
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.Seller)
                .WithMany()  // Seller không cần truy cập các đơn hàng
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình mối quan hệ giữa OrderModel và OrderDetails
            modelBuilder.Entity<OrderModel>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderCode)
                .HasPrincipalKey(o => o.OrderCode);

            // Cấu hình khóa chính cho OrderDetails
            modelBuilder.Entity<OrderDetails>()
                .HasKey(od => od.Id);

            // Cấu hình khóa chính cho OrderModel
            modelBuilder.Entity<OrderModel>()
                .HasKey(o => o.Id);

            // Cấu hình quan hệ giữa ProductModel và ProductImages
            modelBuilder.Entity<ProductModel>()
                .HasMany(p => p.ProductImages)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ giữa ProductImages và Product
            modelBuilder.Entity<ProductImages>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ giữa RatingModel và User
            modelBuilder.Entity<RatingModel>()
                .HasOne(r => r.User)
                .WithMany()  // User không cần truy cập các đánh giá
                .HasForeignKey(r => r.UserId);

        }
    }
}
