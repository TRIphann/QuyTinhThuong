namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class AccountantDashboardVm
    {
        public string FullName { get; set; } = string.Empty;
        public decimal FundBalance { get; set; }
        public decimal TotalDonations { get; set; }
        public decimal TotalExpenses { get; set; }
        public int DonationsThisMonth { get; set; }
        public int ExpensesThisMonth { get; set; }
        public List<DonationVm> RecentDonations { get; set; } = new();
        public List<ExpenseVm> RecentExpenses { get; set; } = new();
    }

    public class DonationVm
    {
        public int DonationId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string DonorType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DonationDate { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? ReceivedByName { get; set; }
    }

    public class ExpenseVm
    {
        public int ExpenseId { get; set; }
        public string BeneficiaryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaidByName { get; set; }
    }

    public class ExpenseCreateVm
    {
        public int RequestId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
