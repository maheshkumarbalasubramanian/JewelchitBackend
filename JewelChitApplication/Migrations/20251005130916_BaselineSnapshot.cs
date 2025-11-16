using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelChitApplication.Migrations
{
    /// <inheritdoc />
    public partial class BaselineSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create gold_loans table
            migrationBuilder.CreateTable(
                name: "gold_loans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    series = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "GOLD SERIES"),
                    loan_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ref_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    loan_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    maturity_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    interest_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    interest_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    advance_months = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    advance_interest_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    processing_fee_percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    processing_fee_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    net_payable = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_qty = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_gross_weight = table.Column<decimal>(type: "numeric(10,3)", nullable: false, defaultValue: 0m),
                    total_net_weight = table.Column<decimal>(type: "numeric(10,3)", nullable: false, defaultValue: 0m),
                    total_calculated_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    total_maximum_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    due_months = table.Column<int>(type: "integer", nullable: false),
                    remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gold_loans", x => x.id);
                    table.ForeignKey(
                        name: "fk_gold_loans_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_gold_loans_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_gold_loans_schemes_scheme_id",
                        column: x => x.scheme_id,
                        principalTable: "schemes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_gold_loans_item_groups_item_group_id",
                        column: x => x.item_group_id,
                        principalTable: "item_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create pledged_items table
            migrationBuilder.CreateTable(
                name: "pledged_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gold_loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gold_rate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: false),
                    gross_weight = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    net_weight = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    calculated_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    maximum_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    jewel_fault = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    huid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    hallmark_purity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    hallmark_gross_weight = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    hallmark_net_weight = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    images = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pledged_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_pledged_items_gold_loans_gold_loan_id",
                        column: x => x.gold_loan_id,
                        principalTable: "gold_loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pledged_items_item_types_item_type_id",
                        column: x => x.item_type_id,
                        principalTable: "item_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pledged_items_purities_purity_id",
                        column: x => x.purity_id,
                        principalTable: "purities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes for gold_loans
            migrationBuilder.CreateIndex(
                name: "ix_gold_loans_loan_number",
                table: "gold_loans",
                column: "loan_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_gold_loans_customer_id",
                table: "gold_loans",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_gold_loans_area_id",
                table: "gold_loans",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "ix_gold_loans_scheme_id",
                table: "gold_loans",
                column: "scheme_id");

            migrationBuilder.CreateIndex(
                name: "ix_gold_loans_item_group_id",
                table: "gold_loans",
                column: "item_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_gold_loans_loan_date",
                table: "gold_loans",
                column: "loan_date");

            migrationBuilder.CreateIndex(
                name: "ix_gold_loans_status",
                table: "gold_loans",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_gold_loans_area_id_status",
                table: "gold_loans",
                columns: new[] { "area_id", "status" });

            // Create indexes for pledged_items
            migrationBuilder.CreateIndex(
                name: "ix_pledged_items_gold_loan_id",
                table: "pledged_items",
                column: "gold_loan_id");

            migrationBuilder.CreateIndex(
                name: "ix_pledged_items_item_type_id",
                table: "pledged_items",
                column: "item_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_pledged_items_purity_id",
                table: "pledged_items",
                column: "purity_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pledged_items");

            migrationBuilder.DropTable(
                name: "gold_loans");
        }
    }
}
