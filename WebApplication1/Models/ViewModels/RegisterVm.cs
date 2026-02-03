using System.ComponentModel.DataAnnotations;

namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class RegisterVm
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [MaxLength(150, ErrorMessage = "Họ tên không quá 150 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(150, ErrorMessage = "Email không quá 150 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [MaxLength(100, ErrorMessage = "Tên đăng nhập không quá 100 ký tự")]
        [MinLength(3, ErrorMessage = "Tên đăng nhập ít nhất 3 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ chứa chữ, số và dấu gạch dưới")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        [MaxLength(128, ErrorMessage = "Mật khẩu không quá 128 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại phải có 10-11 chữ số")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }
    }
}
