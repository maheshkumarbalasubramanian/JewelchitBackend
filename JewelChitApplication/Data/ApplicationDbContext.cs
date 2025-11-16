using Microsoft.EntityFrameworkCore;
using JewelChitApplication.Models;

namespace JewelChitApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerAddressInfo> CustomerAddressInfo { get; set; }
        public DbSet<CustomerContactInfo> CustomerContactInfo { get; set; }
        public DbSet<CustomerOtherInfo> CustomerOtherInfo { get; set; }
        public DbSet<CustomerDocument> CustomerDocuments { get; set; }
        public DbSet<CustomerVerification> CustomerVerification { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<ItemGroup> ItemGroups { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<JewelFault> JewelFaults { get; set; }
        public DbSet<Purity> Purities { get; set; }
        public DbSet<RateSet> RateSets { get; set; }
        public DbSet<Scheme> Schemes { get; set; }
        public DbSet<GoldLoan> GoldLoans { get; set; }
        public DbSet<PledgedItem> PledgedItems { get; set; }

        // Receipt related DbSets
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptInterestStatement> ReceiptInterestStatements { get; set; }
        public DbSet<ReceiptPaymentMode> ReceiptPaymentModes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Add this line - it converts everything to snake_case automatically
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure GoldLoan entity
            modelBuilder.Entity<GoldLoan>(entity =>
            {
                entity.ToTable("gold_loans");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.LoanNumber)
                    .IsUnique()
                    .HasDatabaseName("idx_gold_loans_loan_number");

                entity.HasIndex(e => e.CustomerId)
                    .HasDatabaseName("idx_gold_loans_customer");

                entity.HasIndex(e => e.AreaId)
                    .HasDatabaseName("idx_gold_loans_area");

                entity.HasIndex(e => e.LoanDate)
                    .HasDatabaseName("idx_gold_loans_loan_date");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("idx_gold_loans_status");

                entity.HasIndex(e => new { e.AreaId, e.Status })
                    .HasDatabaseName("idx_gold_loans_area_status");

                entity.HasOne(e => e.Area)
                    .WithMany()
                    .HasForeignKey(e => e.AreaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Scheme)
                    .WithMany()
                    .HasForeignKey(e => e.SchemeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ItemGroup)
                    .WithMany()
                    .HasForeignKey(e => e.ItemGroupId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PledgedItem entity
            modelBuilder.Entity<PledgedItem>(entity =>
            {
                entity.ToTable("pledged_items");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.GoldLoanId)
                    .HasDatabaseName("idx_pledged_items_loan");

                entity.HasIndex(e => e.ItemTypeId)
                    .HasDatabaseName("idx_pledged_items_item_type");

                entity.HasIndex(e => e.PurityId)
                    .HasDatabaseName("idx_pledged_items_purity");

                entity.HasOne(e => e.GoldLoan)
                    .WithMany(gl => gl.PledgedItems)
                    .HasForeignKey(e => e.GoldLoanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ItemType)
                    .WithMany()
                    .HasForeignKey(e => e.ItemTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Purity)
                    .WithMany()
                    .HasForeignKey(e => e.PurityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Scheme entity
            modelBuilder.Entity<Scheme>(entity =>
            {
                entity.ToTable("schemes");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.SchemeCode)
                    .IsUnique()
                    .HasDatabaseName("idx_schemes_code");

                entity.HasIndex(e => e.AreaId)
                    .HasDatabaseName("idx_schemes_area");

                entity.HasIndex(e => e.ItemGroupId)
                    .HasDatabaseName("idx_schemes_item_group");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("idx_schemes_active");

                entity.HasIndex(e => new { e.AreaId, e.IsActive })
                    .HasDatabaseName("idx_schemes_area_active");

                entity.HasIndex(e => new { e.ItemGroupId, e.IsActive })
                    .HasDatabaseName("idx_schemes_group_active");

                entity.HasOne(e => e.Area)
                    .WithMany()
                    .HasForeignKey(e => e.AreaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ItemGroup)
                    .WithMany()
                    .HasForeignKey(e => e.ItemGroupId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Receipt entity
            modelBuilder.Entity<Receipt>(entity =>
            {
                entity.ToTable("receipts");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ReceiptNumber)
                    .IsUnique()
                    .HasDatabaseName("idx_receipts_number");

                entity.HasIndex(e => e.GoldLoanId)
                    .HasDatabaseName("idx_receipts_loan");

                entity.HasIndex(e => e.CustomerId)
                    .HasDatabaseName("idx_receipts_customer");

                entity.HasIndex(e => e.ReceiptDate)
                    .HasDatabaseName("idx_receipts_date");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("idx_receipts_status");

                // Configure decimal precision
                entity.Property(e => e.PrincipalAmount).HasPrecision(18, 2);
                entity.Property(e => e.InterestAmount).HasPrecision(18, 2);
                entity.Property(e => e.OtherCredits).HasPrecision(18, 2);
                entity.Property(e => e.OtherDebits).HasPrecision(18, 2);
                entity.Property(e => e.DefaultAmount).HasPrecision(18, 2);
                entity.Property(e => e.AddLess).HasPrecision(18, 2);
                entity.Property(e => e.NetPayable).HasPrecision(18, 2);
                entity.Property(e => e.CalculatedInterest).HasPrecision(18, 2);
                entity.Property(e => e.OutstandingPrincipal).HasPrecision(18, 2);
                entity.Property(e => e.OutstandingInterest).HasPrecision(18, 2);

                // Configure relationships
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.GoldLoan)
                    .WithMany()
                    .HasForeignKey(e => e.GoldLoanId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.InterestStatements)
                    .WithOne(i => i.Receipt)
                    .HasForeignKey(i => i.ReceiptId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.PaymentModes)
                    .WithOne(p => p.Receipt)
                    .HasForeignKey(p => p.ReceiptId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ReceiptInterestStatement entity
            modelBuilder.Entity<ReceiptInterestStatement>(entity =>
            {
                entity.ToTable("receipt_interest_statements");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ReceiptId)
                    .HasDatabaseName("idx_interest_statements_receipt");

                entity.HasIndex(e => e.GoldLoanId)
                    .HasDatabaseName("idx_interest_statements_loan");

                // Configure decimal precision
                entity.Property(e => e.InterestAccrued).HasPrecision(18, 2);
                entity.Property(e => e.TotalAccrued).HasPrecision(18, 2);
                entity.Property(e => e.InterestPaid).HasPrecision(18, 2);
                entity.Property(e => e.PrincipalPaid).HasPrecision(18, 2);
                entity.Property(e => e.AddedPrincipal).HasPrecision(18, 2);
                entity.Property(e => e.AdjustedPrincipal).HasPrecision(18, 2);
                entity.Property(e => e.NewPrincipal).HasPrecision(18, 2);
                entity.Property(e => e.OpeningPrincipal).HasPrecision(18, 2);
                entity.Property(e => e.ClosingPrincipal).HasPrecision(18, 2);

                // Configure relationships
                entity.HasOne(e => e.GoldLoan)
                    .WithMany()
                    .HasForeignKey(e => e.GoldLoanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ReceiptPaymentMode entity
            modelBuilder.Entity<ReceiptPaymentMode>(entity =>
            {
                entity.ToTable("receipt_payment_modes");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ReceiptId)
                    .HasDatabaseName("idx_payment_modes_receipt");

                // Configure decimal precision
                entity.Property(e => e.Amount).HasPrecision(18, 2);
            });
        }
    }
}