using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelChitApplication.Models
{
    [Table("areas")]
    public class Area
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [StringLength(10)]
        [Column("area_code")]
        public string AreaCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("area_name")]
        public string AreaName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("area_type")]
        public string AreaType { get; set; } = "PRIMARY";

        [Required]
        [Column("company_id")]
        public Guid CompanyId { get; set; }

        [StringLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [StringLength(10)]
        [Column("pincode")]
        public string? Pincode { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("customer_count")]
        public int CustomerCount { get; set; } = 0;

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
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }
    }

    // DTOs
    public class AddAreaRequest
    {
        [Required(ErrorMessage = "Company is required")]
        public Guid CompanyId { get; set; }

        [Required(ErrorMessage = "Area code is required")]
        [RegularExpression(@"^[A-Z]{2}\d{3}$", ErrorMessage = "Area code must be in format AR001")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Area code must be exactly 5 characters")]
        public string AreaCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Area name must be between 2 and 100 characters")]
        public string AreaName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area type is required")]
        [StringLength(20)]
        public string AreaType { get; set; } = "PRIMARY";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be exactly 6 digits")]
        [StringLength(10)]
        public string? Pincode { get; set; }
    }

    public class UpdateAreaRequest
    {
        [Required(ErrorMessage = "Area name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string AreaName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area type is required")]
        [StringLength(20)]
        public string AreaType { get; set; } = "PRIMARY";

        [StringLength(500)]
        public string? Description { get; set; }

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be exactly 6 digits")]
        [StringLength(10)]
        public string? Pincode { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class AreaResponse
    {
        public Guid Id { get; set; }
        public string AreaCode { get; set; } = string.Empty;
        public string AreaName { get; set; } = string.Empty;
        public string AreaType { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyCode { get; set; }
        public string? Description { get; set; }
        public string? Pincode { get; set; }
        public bool IsActive { get; set; }
        public int CustomerCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class AreaStatsResponse
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public int Recent { get; set; }
    }

    public class AreaFilter
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public Guid? CompanyId { get; set; }
        public int? MinCustomers { get; set; }
        public int? MaxCustomers { get; set; }
    }

    public class CompanyDropdownResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}