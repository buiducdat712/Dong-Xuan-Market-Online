using Microsoft.AspNetCore.Identity;

namespace Dong_Xuan_Market_Online.Models
{
    public class AppUserModel: IdentityUser
    {
        public string Occupation {  get; set; } 
        public string RoleId { get; set; }
        public string FullName { get; set; }

        // Điều hướng ngược từ Seller đến Products
        public ICollection<ProductModel> Products { get; set; }
    }
}
