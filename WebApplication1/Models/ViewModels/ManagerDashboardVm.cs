using QLDuLichRBAC_Upgrade.Models.Entities;

namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class ManagerDashboardVm
    {
        public string FullName { get; set; } = string.Empty;
        public int UnreadNotifications { get; set; }

        // Tài chính
        public decimal TotalDonations { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal CurrentBalance { get; set; }

        // Công việc
        public int PendingTasks { get; set; } // Chờ thực hiện
        public int TasksInProgress { get; set; } // Đang thực hiện
        public int PendingSupportRequests { get; set; } // Yêu cầu hỗ trợ từ nhân viên
        public int CompletedToday { get; set; }
        public int TotalBeneficiaries { get; set; }

        // Danh sách yêu cầu hỗ trợ từ nhân viên
        public List<SupportTask> SupportRequests { get; set; } = new();
    }

    public class SupportRequestVm
    {
        public int RequestId { get; set; }
        public string BeneficiaryName { get; set; } = string.Empty;
        public string BeneficiaryType { get; set; } = string.Empty;
        public decimal? RequestedAmount { get; set; }
        public string? SupportIssue { get; set; }
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
