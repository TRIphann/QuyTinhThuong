namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class AdminUserVm
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public string? Email { get; set; }
        public string Role { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
