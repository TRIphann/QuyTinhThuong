using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    /// <summary>
    /// Bảng lưu danh sách quyên góp cho từng hoạt động hỗ trợ cụ thể
    /// (khác với Donations là quyên góp vào quỹ chung)
    /// </summary>
    [Table("Task_Donations")]
    public class TaskDonation
    {
        [Key]
        public int TaskDonationId { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DonatedAt { get; set; } = DateTime.Now;

        public string? Note { get; set; }

        public bool IsConfirmed { get; set; } = true;

        // Navigation properties
        [ForeignKey("TaskId")]
        public virtual SupportTask Task { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
