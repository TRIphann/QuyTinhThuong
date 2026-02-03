namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class ManagerDashboardVm
    {
        public string FullName { get; set; } = string.Empty;
        public int PendingRequests { get; set; }
        public int ApprovedToday { get; set; }
        public int RejectedToday { get; set; }
        public int TotalBeneficiaries { get; set; }
        public decimal TotalApprovedAmount { get; set; }
        public List<SupportRequestVm> RecentRequests { get; set; } = new();
    }

    public class SupportRequestVm
    {
        public int RequestId { get; set; }
        public string BeneficiaryName { get; set; } = string.Empty;
        public string BeneficiaryType { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ApprovalCreateVm
    {
        public int RequestId { get; set; }
        public string Result { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
