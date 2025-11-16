using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelChitApplication.Models
{
    [Table("companies")]
    public class Company
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [StringLength(10)]
        [Column("company_code")]
        public string CompanyCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("company_name")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("company_type")]
        public string? CompanyType { get; set; } = "PRIMARY";

        [StringLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [StringLength(255)]
        [Column("logo_path")]
        public string? LogoPath { get; set; }

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
    }

    // DTOs
    public class AddCompanyRequest
    {
        [Required(ErrorMessage = "Company code is required")]
        [RegularExpression(@"^[A-Z]{2}\d{3}$", ErrorMessage = "Company code must be in format CO001")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Company code must be exactly 5 characters")]
        public string CompanyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 100 characters")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CompanyType { get; set; } = "PRIMARY";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(10)]
        public string? Pincode { get; set; }

        public IFormFile? Logo { get; set; }
    }

    public class UpdateCompanyRequest
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CompanyType { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(10)]
        public string? Pincode { get; set; }

        public bool IsActive { get; set; } = true;

        public IFormFile? Logo { get; set; }

        public bool RemoveLogo { get; set; } = false;
    }

    public class CompanyResponse
    {
        public Guid Id { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyType { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Pincode { get; set; }
        public bool IsActive { get; set; }
        public int CustomerCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class CompanyStatsResponse
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public int Recent { get; set; }
    }

    public class CompanyFilter
    {
        public string? Search { get; set; }
        public string? Status { get; set; } // "active", "inactive", or empty
        public string? Type { get; set; } // "PRIMARY", "SECONDARY", "RURAL", "URBAN", or empty
        public CustomerRange? CustomerRange { get; set; }
    }

    public class CustomerRange
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }
}