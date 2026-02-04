using QLDuLichRBAC_Upgrade.Models.Entities;

namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class StaffDashboardVm
    {
        public string FullName { get; set; } = string.Empty;
        public int UnreadNotifications { get; set; }
        public int AssignedTasks { get; set; } // Công việc được giao
        public int PendingTasks { get; set; } // Chờ thực hiện
        public int InProgressTasks { get; set; } // Đang thực hiện
        public int CompletedTasks { get; set; } // Đã hoàn thành
        public int TotalDonors { get; set; }
        public int TotalBeneficiaries { get; set; }
        public int DonationsToday { get; set; }
        public int RequestsCreatedToday { get; set; }
        public decimal TodayDonationAmount { get; set; }
        public List<DonorVm> RecentDonors { get; set; } = new();
        public List<BeneficiaryVm> RecentBeneficiaries { get; set; } = new();
        
        // Công việc của nhân viên
        public List<SupportTask> MyTasks { get; set; } = new();
    }

    public class DonorVm
    {
        public int DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string DonorType { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public decimal TotalDonated { get; set; }
    }

    public class BeneficiaryVm
    {
        public int BeneficiaryId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BeneficiaryType { get; set; } = string.Empty;
        public string? Address { get; set; }
        public int TotalRequests { get; set; }
        public string Status { get; set; } = "Đã duyệt";
    }

    public class DonorCreateVm
    {
        public string DonorName { get; set; } = string.Empty;
        public string DonorType { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class BeneficiaryCreateVm
    {
        public string FullName { get; set; } = string.Empty;
        public string BeneficiaryType { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Description { get; set; }
    }

    public class DonationCreateVm
    {
        public int DonorId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
    }

    public class SupportRequestCreateVm
    {
        public int BeneficiaryId { get; set; }
        public decimal RequestedAmount { get; set; }
        public string? Reason { get; set; }
    }
}
