using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Dong_Xuan_Market_Online.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Bạn chưa nhập vào tài khoản")]
        public string UserName { get; set; }
        [DataType(DataType.Password),Required(ErrorMessage = "Bạn chưa nhập vào mật khẩu")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Bạn chưa nhập vào Email"), EmailAddress]
        public string Email { get; set; }

    }
}
