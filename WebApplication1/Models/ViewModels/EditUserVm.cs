using System.ComponentModel.DataAnnotations;

namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class EditUserVm
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        [MaxLength(150, ErrorMessage = "Họ tên không quá 150 ký tự")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        [MaxLength(100, ErrorMessage = "Tên đăng nhập không quá 100 ký tự")]
        public string Username { get; set; } = "";

        [Display(Name = "Mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        public string? NewPassword { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        [MaxLength(150, ErrorMessage = "Email không quá 150 ký tự")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "SĐT phải từ 10-11 số")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Active";

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public int RoleId { get; set; }

        public string? CurrentRole { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }

    public class CreateUserVm
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        [MaxLength(150, ErrorMessage = "Họ tên không quá 150 ký tự")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        [MaxLength(100, ErrorMessage = "Tên đăng nhập không quá 100 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ chứa chữ, số và dấu gạch dưới")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        public string Password { get; set; } = "";

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        [MaxLength(150, ErrorMessage = "Email không quá 150 ký tự")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "SĐT phải từ 10-11 số")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public int RoleId { get; set; }
    }
}
