using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace JewelChitApplication.DTOs
{
    public class CustomerRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string CustomerCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string RelationshipName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RelationName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(15)]
        public string Mobile { get; set; } = string.Empty;

        public string? ProfileImage { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        public AddressInfoDto AddressInfo { get; set; } = new AddressInfoDto();

        [Required]
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();

        [Required]
        public OtherInfoDto OtherInfo { get; set; } = new OtherInfoDto();

        public DocumentsDto Documents { get; set; } = new DocumentsDto();

        public VerificationDto Verification { get; set; } = new VerificationDto();
    }

    // Response DTO for Customer
    public class CustomerResponseDto
    {
        public Guid Id { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string RelationshipName { get; set; } = string.Empty;
        public string RelationName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public AddressInfoDto AddressInfo { get; set; } = new AddressInfoDto();
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();
        public OtherInfoDto OtherInfo { get; set; } = new OtherInfoDto();
        public DocumentsDto Documents { get; set; } = new DocumentsDto();
        public VerificationDto Verification { get; set; } = new VerificationDto();
    }

    // Address Information DTO
    public class AddressInfoDto
    {
        [Required]
        [MaxLength(500)]
        public string Address1 { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address2 { get; set; }

        [MaxLength(500)]
        public string? Address3 { get; set; }

        [MaxLength(500)]
        public string? Address4 { get; set; }

        [Required]
        [MaxLength(100)]
        public string Area { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string Pincode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
    }

    // Contact Information DTO
    public class ContactInfoDto
    {
        [Required]
        [Phone]
        [MaxLength(15)]
        public string PrimaryPhone { get; set; } = string.Empty;

        [Phone]
        [MaxLength(15)]
        public string? SecondaryPhone { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }

        [Phone]
        [MaxLength(15)]
        public string? Whatsapp { get; set; }
    }

    // Other Information DTO
    public class OtherInfoDto
    {
        [Required]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhar number must be 12 digits")]
        public string AadharNumber { get; set; } = string.Empty;

        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format")]
        public string? PanNumber { get; set; }

        [MaxLength(100)]
        public string? Occupation { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly income must be positive")]
        public decimal? MonthlyIncome { get; set; }

        [MaxLength(255)]
        public string? ReferenceBy { get; set; }
    }

    // Documents DTO
    public class DocumentsDto
    {
        public string? AadharDocument { get; set; }
        public string? PanDocument { get; set; }
        public string? IncomeProof { get; set; }
        public string? AddressProof { get; set; }
        public string? OtherDocument { get; set; }
    }

    // Verification DTO
    public class VerificationDto
    {
        public bool AadharVerified { get; set; } = false;
        public bool FingerprintVerified { get; set; } = false;
    }

    // Customer Search/Filter DTO
    public class CustomerSearchFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Area { get; set; }
        public string? Status { get; set; }
        public string? CustomerCode { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string SortBy { get; set; } = "customerName";
        public string SortDirection { get; set; } = "asc";
    }

    // Paginated Response DTO
    public class PaginatedResponseDto<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    // Customer Statistics DTO
    public class CustomerStatsDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int VerifiedCustomers { get; set; }
        public int RecentCustomers { get; set; }
    }

    // Update Customer Status DTO
    public class UpdateCustomerStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
