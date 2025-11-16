using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelChitApplication.Models
{
    [Table("item_groups")]
    public class ItemGroup
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [StringLength(10)]
        [Column("group_code")]
        public string GroupCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("group_name")]
        public string GroupName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("group_name_tamil")]
        public string? GroupNameTamil { get; set; }

        [Required]
        [Column("area_id")]
        public Guid AreaId { get; set; }

        [StringLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("item_count")]
        public int ItemCount { get; set; } = 0;

        private DateTime _createdDate = DateTime.UtcNow;
        [Column("created_date")]
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => _createdDate = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        private DateTime _updatedDate = DateTime.UtcNow;
        [Column("updated_date")]
        public DateTime UpdatedDate
        {
            get => _updatedDate;
            set => _updatedDate = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        [StringLength(100)]
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        // Navigation property
        [ForeignKey("AreaId")]
        public Area? Area { get; set; }
    }

    // DTOs
    public class AddItemGroupRequest
    {
        [Required(ErrorMessage = "Area is required")]
        public Guid AreaId { get; set; }

        [Required(ErrorMessage = "Group code is required")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Group code must be between 2 and 10 characters")]
        public string GroupCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Group name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Group name must be between 2 and 100 characters")]
        public string GroupName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Tamil name cannot exceed 100 characters")]
        public string? GroupNameTamil { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class UpdateItemGroupRequest
    {
        [Required(ErrorMessage = "Group name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string GroupName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? GroupNameTamil { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ItemGroupResponse
    {
        public Guid Id { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string? GroupNameTamil { get; set; }
        public Guid AreaId { get; set; }
        public string? AreaName { get; set; }
        public string? AreaCode { get; set; }
        public string? CompanyName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class ItemGroupStatsResponse
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public int Recent { get; set; }
    }

    public class ItemGroupFilter
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public Guid? AreaId { get; set; }
    }

    public class AreaDropdownResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
    }

    public class ItemGroupDropdownResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}