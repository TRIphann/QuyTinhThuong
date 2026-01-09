using System.ComponentModel.DataAnnotations;

namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class LoginVm
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true;
    }
}
