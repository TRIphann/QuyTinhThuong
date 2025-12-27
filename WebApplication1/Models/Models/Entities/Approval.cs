using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Approvals")]
    public class Approval
    {
        [Key]
        public int ApprovalId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public int ApprovedBy { get; set; }

        public DateTime ApprovalDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string Result { get; set; } = string.Empty;

        public string? Note { get; set; }

        // Navigation properties
        [ForeignKey("RequestId")]
        public virtual SupportRequest SupportRequest { get; set; } = null!;

        [ForeignKey("ApprovedBy")]
        public virtual User ApprovedByUser { get; set; } = null!;
    }
}
