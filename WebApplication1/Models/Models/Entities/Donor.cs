using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Donors")]
    public class Donor
    {
        [Key]
        public int DonorId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DonorName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DonorType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        // Navigation properties
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
