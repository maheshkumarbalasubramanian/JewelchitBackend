using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelChitApplication.Models
{
    public class GoldLoan
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Series { get; set; } = "GOLD SERIES";

        public string? CustomerImage { get; set; }
        public decimal TotalStoneWeight { get; set; }


        [Required]
        [MaxLength(50)]
        public string LoanNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? RefNumber { get; set; }

        [Required]
        public DateTime LoanDate { get; set; }

        [Required]
        public DateTime MaturityDate { get; set; }

        // Foreign Keys
        [Required]
        public Guid AreaId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }


        [Required]
        public Guid SchemeId { get; set; }

        [Required]
        public Guid ItemGroupId { get; set; }

        // Navigation Properties
        [ForeignKey("AreaId")]
        public Area? Area { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [ForeignKey("SchemeId")]
        public Scheme? Scheme { get; set; }

        [ForeignKey("ItemGroupId")]
        public ItemGroup? ItemGroup { get; set; }

        // Loan Details
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LoanAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestAmount { get; set; }

        [Required]
        public int AdvanceMonths { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvanceInterestAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal ProcessingFeePercent { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProcessingFeeAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPayable { get; set; }

        // Item Totals
        [Required]
        public int TotalQty { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal TotalGrossWeight { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal TotalNetWeight { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCalculatedValue { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalMaximumValue { get; set; }

        [Required]
        public int DueMonths { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        // Audit Fields
        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        // Collection
        public ICollection<PledgedItem> PledgedItems { get; set; } = new List<PledgedItem>();
    }

    public class PledgedItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid GoldLoanId { get; set; }

        [ForeignKey("GoldLoanId")]
        public GoldLoan? GoldLoan { get; set; }

        public decimal StoneWeight { get; set; }

        // Foreign Keys
        [Required]
        public Guid ItemTypeId { get; set; }

        [Required]
        public Guid PurityId { get; set; }

        // Navigation Properties
        [ForeignKey("ItemTypeId")]
        public ItemType? ItemType { get; set; }

        [ForeignKey("PurityId")]
        public Purity? Purity { get; set; }

        // Item Details
        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GoldRate { get; set; }

        [Required]
        public int Qty { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal GrossWeight { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal NetWeight { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CalculatedValue { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaximumValue { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [MaxLength(100)]
        public string? JewelFault { get; set; }

        // Hallmark Details (Optional)
        [MaxLength(50)]
        public string? Huid { get; set; }

        [MaxLength(20)]
        public string? HallmarkPurity { get; set; }

        [Column(TypeName = "decimal(10,3)")]
        public decimal? HallmarkGrossWeight { get; set; }

        [Column(TypeName = "decimal(10,3)")]
        public decimal? HallmarkNetWeight { get; set; }

        // Audit
        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        // Images stored as JSON array of base64 strings or URLs
        public string? Images { get; set; }
    }
}