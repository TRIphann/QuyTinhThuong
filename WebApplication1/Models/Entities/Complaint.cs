using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Complaints")]
    public class Complaint
    {
        [Key]
        public int ComplaintId { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? ResponseBy { get; set; }
        public string? ResponseContent { get; set; }
        public DateTime? ResponseAt { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Chờ xử lý";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("TaskId")]
        public virtual SupportTask Task { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ResponseBy")]
        public virtual User? Responder { get; set; }
    }
}
