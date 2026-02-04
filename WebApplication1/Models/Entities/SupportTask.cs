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

        // Ngày dự kiến bắt đầu (do Manager chọn)
        public DateTime? ScheduledDate { get; set; }

        // Số tiền mục tiêu do quản lý đặt
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;

        // Số tiền tình nguyện viên đã quyên góp cho hoạt động này
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonatedAmount { get; set; } = 0;

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

        // Số người yêu cầu hỗ trợ (khi type = "Nhân lực")
        public int? SupportRequestPeopleCount { get; set; }

        // Số người đã được điều đến và chấp nhận
        public int? SupportAssignedPeopleCount { get; set; }

        public DateTime? SupportRequestAt { get; set; }

        // Phản hồi từ quản lý
        public string? SupportResponseStatus { get; set; } // "Đang xử lý", "Đã duyệt", "Từ chối"
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

        // Danh sách tình nguyện viên và quyên góp cho hoạt động này
        public virtual ICollection<TaskVolunteer> Volunteers { get; set; } = new List<TaskVolunteer>();
        public virtual ICollection<TaskDonation> TaskDonations { get; set; } = new List<TaskDonation>();

        // Tổng tiền = Amount + AdditionalAmount
        [NotMapped]
        public decimal TotalAmount => Amount + AdditionalAmount;
    }
}
