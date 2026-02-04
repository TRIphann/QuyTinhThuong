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

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RequestedAmount { get; set; }

        public string? SupportIssue { get; set; }  // Vấn đề cần hỗ trợ

        public string? Reason { get; set; }  // Lý do hỗ trợ

        public int? CreatedBy { get; set; }  // Ai tạo yêu cầu

        [MaxLength(50)]
        public string Status { get; set; } = "Chờ xét duyệt";

        // Navigation properties
        [ForeignKey("BeneficiaryId")]
        public virtual Beneficiary Beneficiary { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }

        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
