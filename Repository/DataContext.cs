using Dong_Xuan_Market_Online.Models;
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
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thiết lập quan hệ giữa Product và Seller
            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Seller)  // Product có một người bán
                .WithMany(u => u.Products)  // Người bán có nhiều sản phẩm
                .HasForeignKey(p => p.SellerId)  // SellerId là khóa ngoại
                .OnDelete(DeleteBehavior.Restrict);  // Quy tắc xóa
            // Thiết lập quan hệ giữa Order và Seller
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.Seller)
                .WithMany()
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
            // Cấu hình mối quan hệ giữa OrderModel và OrderDetails
            modelBuilder.Entity<OrderModel>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderCode)
                .HasPrincipalKey(o => o.OrderCode);

            modelBuilder.Entity<OrderDetails>()
                .HasKey(od => od.Id);

            modelBuilder.Entity<OrderModel>()
                .HasKey(o => o.Id);

        }

    }
}
