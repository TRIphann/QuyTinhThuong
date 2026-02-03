using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDuLichRBAC_Upgrade.Models.Entities
{
    [Table("Donations")]
    public class Donation
    {
        [Key]
        public int DonationId { get; set; }

        [Required]
        public int DonorId { get; set; }

        public int? DonorUserId { get; set; } // Liên kết với User nếu có tài khoản

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DonationDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string Method { get; set; } = string.Empty;

        public int? ReceivedBy { get; set; }

        public bool? IsConfirmed { get; set; } = true; // Đã xác nhận chuyển khoản

        // Navigation properties
        [ForeignKey("DonorId")]
        public virtual Donor Donor { get; set; } = null!;

        [ForeignKey("DonorUserId")]
        public virtual User? DonorUser { get; set; }

        [ForeignKey("ReceivedBy")]
        public virtual User? ReceivedByUser { get; set; }
    }
}
