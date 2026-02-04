using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    /// <summary>
    /// Bảng yêu cầu phê duyệt ngân sách - Admin phê duyệt các yêu cầu chi tiền từ Manager
    /// </summary>
    [Table("Budget_Approvals")]
    public class BudgetApproval
    {
        [Key]
        public int ApprovalId { get; set; }

        // Loại yêu cầu: "CreateTask", "AdditionalSupport"
        [Required]
        [MaxLength(50)]
        public string RequestType { get; set; } = string.Empty;

        // Manager yêu cầu
        public int RequestedBy { get; set; }

        // Thời gian yêu cầu
        public DateTime RequestedAt { get; set; } = DateTime.Now;

        // Số tiền yêu cầu
        public decimal Amount { get; set; }

        // Mô tả yêu cầu
        public string? Description { get; set; }

        // Liên kết với task (nếu có)
        public int? RelatedTaskId { get; set; }

        // Liên kết với support request (nếu có)
        public int? RelatedRequestId { get; set; }

        // Trạng thái: "Chờ duyệt", "Đã duyệt", "Từ chối"
        [MaxLength(50)]
        public string Status { get; set; } = "Chờ duyệt";

        // Admin phê duyệt
        public int? ApprovedBy { get; set; }

        // Thời gian phê duyệt
        public DateTime? ApprovedAt { get; set; }

        // Lý do từ chối
        public string? RejectionReason { get; set; }

        // Dữ liệu bổ sung cho CreateTask
        public string? StaffIds { get; set; } // JSON array

        public DateTime? ScheduledDate { get; set; }

        public string? ManagerNote { get; set; }

        // Navigation properties
        [ForeignKey("RequestedBy")]
        public virtual User Requester { get; set; } = null!;

        [ForeignKey("ApprovedBy")]
        public virtual User? Approver { get; set; }

        [ForeignKey("RelatedTaskId")]
        public virtual SupportTask? RelatedTask { get; set; }

        [ForeignKey("RelatedRequestId")]
        public virtual SupportRequest? RelatedRequest { get; set; }
    }
}
