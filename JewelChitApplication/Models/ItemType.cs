using System;
using System.ComponentModel.DataAnnotations;

namespace JewelChitApplication.Models
{
    public class ItemType
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ItemNameTamil { get; set; }

        [Required]
        public Guid ItemGroupId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public int ItemCount { get; set; } = 0;

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