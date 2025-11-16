using System;
using System.ComponentModel.DataAnnotations;

namespace JewelChitApplication.Models
{
    public class JewelFault
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string FaultCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FaultName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string FaultType { get; set; } = string.Empty; // PHYSICAL, QUALITY, STRUCTURAL, AESTHETIC

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL

        [Required]
        public bool AffectsValuation { get; set; } = false;

        public decimal? ValuationImpactPercentage { get; set; }

        [Required]
        public Guid ItemGroupId { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        // Navigation property
        public virtual ItemGroup? ItemGroup { get; set; }
    }
}