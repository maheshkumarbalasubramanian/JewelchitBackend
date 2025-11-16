using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelChitApplication.Models
{
    [Table("rate_sets")]
    public class RateSet
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("area_id")]
        public Guid AreaId { get; set; }

        [Required]
        [Column("effective_date")]
        public DateTime EffectiveDate { get; set; }

        [Required]
        [Column("gold_rate_per_gram")]
        [Range(0.01, double.MaxValue)]
        public decimal GoldRatePerGram { get; set; }

        [Required]
        [Column("gold_purity_rate_per_gram")]
        [Range(0.01, double.MaxValue)]
        public decimal GoldPurityRatePerGram { get; set; }

        [Required]
        [Column("silver_rate_per_gram")]
        [Range(0.01, double.MaxValue)]
        public decimal SilverRatePerGram { get; set; }

        [Required]
        [Column("silver_purity_rate_per_gram")]
        [Range(0.01, double.MaxValue)]
        public decimal SilverPurityRatePerGram { get; set; }

        [Column("notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("updated_date")]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [Column("updated_by")]
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        // Navigation property
        [ForeignKey("AreaId")]
        public virtual Area? Area { get; set; }
    }
}