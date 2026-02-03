using System.ComponentModel.DataAnnotations;

namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class LoginVm
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [MaxLength(100, ErrorMessage = "Tên đăng nhập không quá 100 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MaxLength(128, ErrorMessage = "Mật khẩu không quá 128 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true;
    }
}
