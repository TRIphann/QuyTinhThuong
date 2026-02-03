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
        public DateTime? LastLoginTime { get; set; }
        
        public string LastActiveText
        {
            get
            {
                if (LastLoginTime == null) return "Chưa đăng nhập";
                var diff = DateTime.Now - LastLoginTime.Value;
                if (diff.TotalMinutes < 1) return "Đang hoạt động";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
                return $"{(int)diff.TotalDays} ngày trước";
            }
        }
    }
}
