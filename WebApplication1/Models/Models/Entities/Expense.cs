using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Expenses")]
    public class Expense
    {
        [Key]
        public int ExpenseId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        public int? PaidBy { get; set; }

        // Navigation properties
        [ForeignKey("RequestId")]
        public virtual SupportRequest SupportRequest { get; set; } = null!;

        [ForeignKey("PaidBy")]
        public virtual User? PaidByUser { get; set; }
    }
}
