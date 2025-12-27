using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Funds")]
    public class Fund
    {
        [Key]
        public int FundId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FundName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
