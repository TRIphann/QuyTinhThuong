using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Support_Tasks")]
    public class SupportTask
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public int RequestId { get; set; }

        public int? DonorUserId { get; set; }
        public int? AssignedStaffId { get; set; }
        public int? AssignedBy { get; set; }
        public DateTime? AssignedAt { get; set; }

        // Số tiền do quản lý nhập
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;

        // Số tiền hỗ trợ thêm được duyệt
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalAmount { get; set; } = 0;

        [MaxLength(50)]
        public string Status { get; set; } = "Chờ thực hiện";

        // Thời gian nhân viên bắt đầu thực hiện (lúc này mới trừ tiền)
        public DateTime? StartedAt { get; set; }

        public string? StaffNote { get; set; }
        public DateTime? StaffCompletedAt { get; set; }

        public string? ManagerNote { get; set; }
        public DateTime? ManagerVerifiedAt { get; set; }

        // Yêu cầu hỗ trợ từ nhân viên
        [MaxLength(50)]
        public string? SupportRequestType { get; set; } // "Tiền" hoặc "Nhân lực"

        public string? SupportRequestReason { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SupportRequestAmount { get; set; }

        public DateTime? SupportRequestAt { get; set; }

        // Phản hồi từ quản lý
        public string? SupportResponseNote { get; set; }
        public DateTime? SupportResponseAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RequestId")]
        public virtual SupportRequest SupportRequest { get; set; } = null!;

        [ForeignKey("DonorUserId")]
        public virtual User? DonorUser { get; set; }

        [ForeignKey("AssignedStaffId")]
        public virtual User? AssignedStaff { get; set; }

        [ForeignKey("AssignedBy")]
        public virtual User? Assigner { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

        // Tổng tiền = Amount + AdditionalAmount
        [NotMapped]
        public decimal TotalAmount => Amount + AdditionalAmount;
    }
}
