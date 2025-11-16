using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelChitApplication.Models
{
    public class Receipt
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ReceiptDate { get; set; }

        [Required]
        public DateTime TillDate { get; set; }

        [Required]
        public Guid GoldLoanId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LoanNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CustomerCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PaymentType { get; set; } = "interest"; // interest, partial, full

        [Required]
        public decimal PrincipalAmount { get; set; }

        [Required]
        public decimal InterestAmount { get; set; }

        public decimal OtherCredits { get; set; }

        public decimal OtherDebits { get; set; }

        public decimal DefaultAmount { get; set; }

        public decimal AddLess { get; set; }

        [Required]
        public decimal NetPayable { get; set; }

        [Required]
        public decimal CalculatedInterest { get; set; }

        [Required]
        public decimal OutstandingPrincipal { get; set; }

        public decimal OutstandingInterest { get; set; }

        public string? Remarks { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Completed"; // Completed, Cancelled

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("GoldLoanId")]
        public virtual GoldLoan? GoldLoan { get; set; }

        public virtual ICollection<ReceiptInterestStatement> InterestStatements { get; set; } = new List<ReceiptInterestStatement>();

        public virtual ICollection<ReceiptPaymentMode> PaymentModes { get; set; } = new List<ReceiptPaymentMode>();
    }

    public class ReceiptInterestStatement
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ReceiptId { get; set; }

        [Required]
        public Guid GoldLoanId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        public int DurationDays { get; set; }

        [Required]
        public decimal InterestAccrued { get; set; }

        [Required]
        public decimal TotalAccrued { get; set; }

        [Required]
        public decimal InterestPaid { get; set; }

        [Required]
        public decimal PrincipalPaid { get; set; }

        public decimal AddedPrincipal { get; set; }

        public decimal AdjustedPrincipal { get; set; }

        [Required]
        public decimal NewPrincipal { get; set; }

        [Required]
        public decimal OpeningPrincipal { get; set; }

        [Required]
        public decimal ClosingPrincipal { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ReceiptId")]
        public virtual Receipt? Receipt { get; set; }

        [ForeignKey("GoldLoanId")]
        public virtual GoldLoan? GoldLoan { get; set; }
    }

    public class ReceiptPaymentMode
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ReceiptId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMode { get; set; } = "Cash"; // Cash, Card, UPI, Cheque, Bank Transfer

        [Required]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ReceiptId")]
        public virtual Receipt? Receipt { get; set; }
    }
}