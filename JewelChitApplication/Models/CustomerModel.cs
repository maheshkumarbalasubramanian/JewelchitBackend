using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelChitApplication.Models
{
    [Table("customers")]
    public class Customer
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        [Column("customer_code")]
        public string CustomerCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("relationship_name")]
        public string RelationshipName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("relation_name")]
        public string RelationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        [Column("mobile")]
        public string Mobile { get; set; } = string.Empty;


        [Column("profile_image")]
        public string? ProfileImage { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "ACTIVE";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        // Navigation Properties
        public CustomerAddressInfo? AddressInfo { get; set; }
        public CustomerContactInfo? ContactInfo { get; set; }
        public CustomerOtherInfo? OtherInfo { get; set; }
        public CustomerVerification? Verification { get; set; }
        public ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
    }

    // Customer Address Information
    [Table("customer_address_info")]
    public class CustomerAddressInfo
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("address1")]
        public string Address1 { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("address2")]
        public string? Address2 { get; set; }

        [MaxLength(500)]
        [Column("address3")]
        public string? Address3 { get; set; }

        [MaxLength(500)]
        [Column("address4")]
        public string? Address4 { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("area")]
        public string Area { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("state")]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Column("pincode")]
        public string Pincode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("city")]
        public string City { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }

    // Customer Contact Information
    [Table("customer_contact_info")]
    public class CustomerContactInfo
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(15)]
        [Column("primary_phone")]
        public string PrimaryPhone { get; set; } = string.Empty;

        [MaxLength(15)]
        [Column("secondary_phone")]
        public string? SecondaryPhone { get; set; }

        [MaxLength(255)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(15)]
        [Column("whatsapp")]
        public string? Whatsapp { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }

    // Customer Other Information
    [Table("customer_other_info")]
    public class CustomerOtherInfo
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(12)]
        [Column("aadhar_number")]
        public string AadharNumber { get; set; } = string.Empty;

        [MaxLength(10)]
        [Column("pan_number")]
        public string? PanNumber { get; set; }

        [MaxLength(100)]
        [Column("occupation")]
        public string? Occupation { get; set; }

        [Column("monthly_income")]
        public decimal? MonthlyIncome { get; set; }

        [MaxLength(255)]
        [Column("reference_by")]
        public string? ReferenceBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }

    // Customer Documents
    [Table("customer_documents")]
    public class CustomerDocument
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("document_type")]
        public string DocumentType { get; set; } = string.Empty;

        [Column("document_data")]
        public string? DocumentData { get; set; }

        [MaxLength(255)]
        [Column("file_name")]
        public string? FileName { get; set; }

        [Column("file_size")]
        public long? FileSize { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }

    // Customer Verification
    [Table("customer_verification")]
    public class CustomerVerification
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Column("aadhar_verified")]
        public bool AadharVerified { get; set; } = false;

        [Column("fingerprint_verified")]
        public bool FingerprintVerified { get; set; } = false;

        [Column("aadhar_verified_at")]
        public DateTime? AadharVerifiedAt { get; set; }

        [Column("fingerprint_verified_at")]
        public DateTime? FingerprintVerifiedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
