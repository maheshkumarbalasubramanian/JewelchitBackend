using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelChitApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "stone_weight",
                table: "pledged_items",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "customer_image",
                table: "gold_loans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_stone_weight",
                table: "gold_loans",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "receipts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receipt_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    till_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    gold_loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    principal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    other_credits = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    other_debits = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    default_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    add_less = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    net_payable = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    calculated_interest = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    outstanding_principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    outstanding_interest = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receipts", x => x.id);
                    table.ForeignKey(
                        name: "fk_receipts_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_receipts_gold_loans_gold_loan_id",
                        column: x => x.gold_loan_id,
                        principalTable: "gold_loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "receipt_interest_statements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gold_loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    to_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    duration_days = table.Column<int>(type: "integer", nullable: false),
                    interest_accrued = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_accrued = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    principal_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    added_principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    adjusted_principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    new_principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    opening_principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    closing_principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receipt_interest_statements", x => x.id);
                    table.ForeignKey(
                        name: "fk_receipt_interest_statements_gold_loans_gold_loan_id",
                        column: x => x.gold_loan_id,
                        principalTable: "gold_loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_receipt_interest_statements_receipts_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "receipt_payment_modes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reference_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receipt_payment_modes", x => x.id);
                    table.ForeignKey(
                        name: "fk_receipt_payment_modes_receipts_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_interest_statements_loan",
                table: "receipt_interest_statements",
                column: "gold_loan_id");

            migrationBuilder.CreateIndex(
                name: "idx_interest_statements_receipt",
                table: "receipt_interest_statements",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "idx_payment_modes_receipt",
                table: "receipt_payment_modes",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "idx_receipts_customer",
                table: "receipts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "idx_receipts_date",
                table: "receipts",
                column: "receipt_date");

            migrationBuilder.CreateIndex(
                name: "idx_receipts_loan",
                table: "receipts",
                column: "gold_loan_id");

            migrationBuilder.CreateIndex(
                name: "idx_receipts_number",
                table: "receipts",
                column: "receipt_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_receipts_status",
                table: "receipts",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "receipt_interest_statements");

            migrationBuilder.DropTable(
                name: "receipt_payment_modes");

            migrationBuilder.DropTable(
                name: "receipts");

            migrationBuilder.DropColumn(
                name: "stone_weight",
                table: "pledged_items");

            migrationBuilder.DropColumn(
                name: "customer_image",
                table: "gold_loans");

            migrationBuilder.DropColumn(
                name: "total_stone_weight",
                table: "gold_loans");
        }
    }
}
