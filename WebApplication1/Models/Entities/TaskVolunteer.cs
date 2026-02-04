using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    /// <summary>
    /// Bảng lưu danh sách tình nguyện viên đăng ký tham gia hoạt động hỗ trợ
    /// </summary>
    [Table("Task_Volunteers")]
    public class TaskVolunteer
    {
        [Key]
        public int VolunteerId { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Status { get; set; } = "Đăng ký"; // Đăng ký, Đã xác nhận, Đã tham gia, Hủy

        public string? Note { get; set; }

        // Navigation properties
        [ForeignKey("TaskId")]
        public virtual SupportTask Task { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
