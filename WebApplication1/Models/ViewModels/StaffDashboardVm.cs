namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class StaffDashboardVm
    {
        public string FullName { get; set; } = string.Empty;
        public int TotalDonors { get; set; }
        public int TotalBeneficiaries { get; set; }
        public int DonationsToday { get; set; }
        public int RequestsCreatedToday { get; set; }
        public decimal TodayDonationAmount { get; set; }
        public List<DonorVm> RecentDonors { get; set; } = new();
        public List<BeneficiaryVm> RecentBeneficiaries { get; set; } = new();
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
