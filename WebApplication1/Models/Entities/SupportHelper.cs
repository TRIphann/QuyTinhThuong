using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    /// <summary>
    /// Bảng lưu danh sách nhân viên được mời hỗ trợ cho một công việc
    /// </summary>
    [Table("Support_Helpers")]
    public class SupportHelper
    {
        [Key]
        public int HelperId { get; set; }

        // Task cần hỗ trợ
        public int TaskId { get; set; }

        // Nhân viên được mời
        public int StaffId { get; set; }

        // Manager đã mời
        public int? InvitedBy { get; set; }

        // Thời gian mời
        public DateTime InvitedAt { get; set; } = DateTime.Now;

        // Trạng thái: "Đang chờ", "Chấp nhận", "Từ chối"
        [MaxLength(50)]
        public string Status { get; set; } = "Đang chờ";

        // Thời gian phản hồi
        public DateTime? RespondedAt { get; set; }

        // Ghi chú từ nhân viên (nếu từ chối)
        public string? StaffNote { get; set; }

        // Navigation properties
        [ForeignKey("TaskId")]
        public virtual SupportTask Task { get; set; } = null!;

        [ForeignKey("StaffId")]
        public virtual User Staff { get; set; } = null!;

        [ForeignKey("InvitedBy")]
        public virtual User? Inviter { get; set; }
    }
}
