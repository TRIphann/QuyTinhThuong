namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    public class AccountantDashboardVm
    {
        public string FullName { get; set; } = string.Empty;
        public int UnreadNotifications { get; set; }
        
        // Thống kê của bản thân
        public decimal MyTotalDonations { get; set; }
        public int MyDonationCount { get; set; }
        
        // Thống kê chung
        public int CompletedSupports { get; set; }
    }

    // Chi tiêu của bản thân (từ SupportTasks) - không dùng nữa
    public class MyExpenseVm
    {
        public int TaskId { get; set; }
        public string BeneficiaryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ManagerNote { get; set; }
        public bool CanComplain => Status != "Hoàn thành" && (DateTime.Now - Date).TotalDays > 3;
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

    // ViewModel cho người cần hỗ trợ (hiển thị trong trang Donations của Khách hàng)
    public class BeneficiarySupportVm
    {
        public int RequestId { get; set; }
        public int BeneficiaryId { get; set; }
        public string BeneficiaryName { get; set; } = string.Empty;
        public string BeneficiaryType { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Reason { get; set; }
        public decimal RequestedAmount { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
