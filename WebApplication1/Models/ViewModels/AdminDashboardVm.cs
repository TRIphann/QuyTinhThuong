namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class AdminDashboardVm
    {
        public string FullName { get; set; } = "Admin";

        public int TotalUsers { get; set; }
        public int TotalSupportRequests { get; set; }
        public decimal FundBalance { get; set; }
        public int ApprovalsToday { get; set; }

        public List<RecentLogVm> RecentLogs { get; set; } = new();
    }

    public class RecentLogVm
    {
        public string Action { get; set; } = "";
        public string? TableName { get; set; }
        public DateTime ActionTime { get; set; }
        public string UserName { get; set; } = "";
        public string RoleName { get; set; } = "";
        
        public string TimeAgo
        {
            get
            {
                var diff = DateTime.Now - ActionTime;
                if (diff.TotalMinutes < 1) return "vừa xong";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
                return $"{(int)diff.TotalDays} ngày trước";
            }
        }
    }
}
