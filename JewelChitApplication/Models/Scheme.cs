using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelChitApplication.Models
{
    [Table("schemes")]
    public class Scheme
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("scheme_code")]
        [MaxLength(10)]
        public string SchemeCode { get; set; } = string.Empty;

        [Required]
        [Column("scheme_name")]
        [MaxLength(50)]
        public string SchemeName { get; set; } = string.Empty;

        [Required]
        [Column("area_id")]
        public Guid AreaId { get; set; }

        [Required]
        [Column("item_group_id")]
        public Guid ItemGroupId { get; set; }

        [Required]
        [Column("roi")]
        [Range(0.1, 100)]
        public decimal Roi { get; set; }

        [Required]
        [Column("calculation_method")]
        [MaxLength(20)]
        public string CalculationMethod { get; set; } = "Simple";

        [Required]
        [Column("is_std_roi")]
        public bool IsStdRoi { get; set; } = false;

        [Required]
        [Column("calculation_based")]
        [MaxLength(20)]
        public string CalculationBased { get; set; } = "Monthly";

        [Column("customized_style")]
        [MaxLength(50)]
        public string? CustomizedStyle { get; set; }

        [Required]
        [Column("processing_fee_slab")]
        public bool ProcessingFeeSlab { get; set; } = false;

        [Required]
        [Column("min_calc_days")]
        public int MinCalcDays { get; set; } = 15;

        [Required]
        [Column("grace_days")]
        public int GraceDays { get; set; } = 0;

        [Required]
        [Column("advance_month")]
        public int AdvanceMonth { get; set; } = 1;

        [Required]
        [Column("processing_fee_percent")]
        public decimal ProcessingFeePercent { get; set; } = 0.1m;

        [Required]
        [Column("min_market_value")]
        public decimal MinMarketValue { get; set; } = 0;

        [Required]
        [Column("max_market_value")]
        public decimal MaxMarketValue { get; set; } = 100000;

        [Required]
        [Column("min_loan_value")]
        public decimal MinLoanValue { get; set; } = 1000;

        [Required]
        [Column("max_loan_value")]
        public decimal MaxLoanValue { get; set; } = 50000;

        [Column("penalty_rate")]
        public decimal? PenaltyRate { get; set; }

        [Column("penalty_grace_days")]
        public int? PenaltyGraceDays { get; set; }

        [Column("compounding_frequency")]
        [MaxLength(20)]
        public string? CompoundingFrequency { get; set; }

        [Column("emi_tenure")]
        public int? EmiTenure { get; set; }

        [Required]
        [Column("reduction_percent")]
        public decimal ReductionPercent { get; set; } = 0;

        [Required]
        [Column("validity_in_months")]
        public int ValidityInMonths { get; set; } = 12;

        [Required]
        [Column("interest_percent_after_validity")]
        public decimal InterestPercentAfterValidity { get; set; } = 0;

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

        // Navigation properties
        [ForeignKey("AreaId")]
        public virtual Area? Area { get; set; }

        [ForeignKey("ItemGroupId")]
        public virtual ItemGroup? ItemGroup { get; set; }
    }
}