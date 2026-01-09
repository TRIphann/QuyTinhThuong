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
    }
}
