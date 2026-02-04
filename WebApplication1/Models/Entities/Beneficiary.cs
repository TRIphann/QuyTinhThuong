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

        // Trạng thái duyệt: "Chờ duyệt", "Đã duyệt", "Từ chối"
        [MaxLength(50)]
        public string Status { get; set; } = "Đã duyệt";

        // Ai đã thêm
        public int? CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }
        
        public virtual ICollection<SupportRequest> SupportRequests { get; set; } = new List<SupportRequest>();
    }
}
