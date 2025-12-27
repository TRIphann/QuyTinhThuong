using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Support_Requests")]
    public class SupportRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int BeneficiaryId { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RequestedAmount { get; set; }

        public string? Reason { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Chờ xét duyệt";

        // Navigation properties
        [ForeignKey("BeneficiaryId")]
        public virtual Beneficiary Beneficiary { get; set; } = null!;

        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
