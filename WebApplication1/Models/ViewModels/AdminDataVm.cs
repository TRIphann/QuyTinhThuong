using QLDuLichRBAC_Upgrade.Models.Entities;

namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class AdminDataVm
    {
        // Counts
        public int Beneficiaries { get; set; }
        public int SupportRequests { get; set; }
        public int Approvals { get; set; }
        public int Expenses { get; set; }
        public int Donors { get; set; }
        public int Donations { get; set; }
        public int Funds { get; set; }
        public int Logs { get; set; }

        // Detailed data for tables
        public List<AdminBeneficiaryVm> RecentBeneficiaries { get; set; } = new();
        public List<SupportRequestVm> RecentRequests { get; set; } = new();
        public List<AdminDonationVm> RecentDonations { get; set; } = new();
        public List<AdminExpenseVm> RecentExpenses { get; set; } = new();
        public List<AdminDonorVm> TopDonors { get; set; } = new();
        public List<AdminLogVm> RecentLogs { get; set; } = new();

        // Stats
        public decimal TotalDonations { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal FundBalance { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }

        // Chart data
        public string DonationChartLabels { get; set; } = "";
        public string DonationChartData { get; set; } = "";
        public string ExpenseChartLabels { get; set; } = "";
        public string ExpenseChartData { get; set; } = "";
    }

    public class AdminBeneficiaryVm
    {
        public int BeneficiaryId { get; set; }
        public string FullName { get; set; } = "";
        public string BeneficiaryType { get; set; } = "";
        public string? Address { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AdminDonationVm
    {
        public int DonationId { get; set; }
        public string DonorName { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime DonationDate { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class AdminExpenseVm
    {
        public int ExpenseId { get; set; }
        public string BeneficiaryName { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class AdminDonorVm
    {
        public int DonorId { get; set; }
        public string FullName { get; set; } = "";
        public string DonorType { get; set; } = "";
        public decimal TotalDonated { get; set; }
        public int DonationCount { get; set; }
    }

    public class AdminLogVm
    {
        public int LogId { get; set; }
        public string Action { get; set; } = "";
        public string? TableName { get; set; }
        public string? UserName { get; set; }
        public DateTime ActionTime { get; set; }
    }
}
