using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Beneficiaries")]
    public class Beneficiary
    {
        [Key]
        public int BeneficiaryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string BeneficiaryType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<SupportRequest> SupportRequests { get; set; } = new List<SupportRequest>();
    }
}
