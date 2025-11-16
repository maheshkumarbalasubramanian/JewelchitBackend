using System;
using System.ComponentModel.DataAnnotations;

namespace JewelChitApplication.Models
{
    public class Purity
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string PurityCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PurityName { get; set; } = string.Empty;

        [Required]
        public decimal PurityPercentage { get; set; }

        public int? Karat { get; set; }

        [Required]
        public Guid ItemGroupId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

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