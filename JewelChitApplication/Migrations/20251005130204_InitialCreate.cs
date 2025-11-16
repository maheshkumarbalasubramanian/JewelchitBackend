using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelChitApplication.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        -- Create gold_loans table only if it doesn't exist
        CREATE TABLE IF NOT EXISTS gold_loans (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            series VARCHAR(20) NOT NULL DEFAULT 'GOLD SERIES',
            loan_number VARCHAR(50) NOT NULL UNIQUE,
            ref_number VARCHAR(50),
            loan_date TIMESTAMP NOT NULL,
            maturity_date TIMESTAMP NOT NULL,
            area_id UUID NOT NULL,
            customer_id UUID NOT NULL,
            scheme_id UUID NOT NULL,
            item_group_id UUID NOT NULL,
            loan_amount DECIMAL(18,2) NOT NULL,
            interest_rate DECIMAL(5,2) NOT NULL,
            interest_amount DECIMAL(18,2) NOT NULL,
            advance_months INTEGER NOT NULL DEFAULT 0,
            advance_interest_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
            processing_fee_percent DECIMAL(5,2) NOT NULL,
            processing_fee_amount DECIMAL(18,2) NOT NULL,
            net_payable DECIMAL(18,2) NOT NULL,
            total_qty INTEGER NOT NULL DEFAULT 0,
            total_gross_weight DECIMAL(10,3) NOT NULL DEFAULT 0,
            total_net_weight DECIMAL(10,3) NOT NULL DEFAULT 0,
            total_calculated_value DECIMAL(18,2) NOT NULL DEFAULT 0,
            total_maximum_value DECIMAL(18,2) NOT NULL DEFAULT 0,
            due_months INTEGER NOT NULL,
            remarks VARCHAR(500),
            status VARCHAR(20) NOT NULL DEFAULT 'Open',
            created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_date TIMESTAMP,
            created_by VARCHAR(100),
            updated_by VARCHAR(100),
            CONSTRAINT fk_gold_loans_areas FOREIGN KEY (area_id) REFERENCES areas(id) ON DELETE RESTRICT,
            CONSTRAINT fk_gold_loans_customers FOREIGN KEY (customer_id) REFERENCES customers(id) ON DELETE RESTRICT,
            CONSTRAINT fk_gold_loans_schemes FOREIGN KEY (scheme_id) REFERENCES schemes(id) ON DELETE RESTRICT,
            CONSTRAINT fk_gold_loans_item_groups FOREIGN KEY (item_group_id) REFERENCES item_groups(id) ON DELETE RESTRICT
        );

        CREATE INDEX IF NOT EXISTS ix_gold_loans_loan_number ON gold_loans(loan_number);
        CREATE INDEX IF NOT EXISTS ix_gold_loans_customer_id ON gold_loans(customer_id);
        CREATE INDEX IF NOT EXISTS ix_gold_loans_area_id ON gold_loans(area_id);
        CREATE INDEX IF NOT EXISTS ix_gold_loans_status ON gold_loans(status);

        -- Create pledged_items table only if it doesn't exist
        CREATE TABLE IF NOT EXISTS pledged_items (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            gold_loan_id UUID NOT NULL,
            item_type_id UUID NOT NULL,
            purity_id UUID NOT NULL,
            item_name VARCHAR(100) NOT NULL,
            gold_rate DECIMAL(18,2) NOT NULL,
            qty INTEGER NOT NULL,
            gross_weight DECIMAL(10,3) NOT NULL,
            net_weight DECIMAL(10,3) NOT NULL,
            calculated_value DECIMAL(18,2) NOT NULL,
            maximum_value DECIMAL(18,2) NOT NULL,
            remarks VARCHAR(500),
            jewel_fault VARCHAR(100),
            huid VARCHAR(50),
            hallmark_purity VARCHAR(20),
            hallmark_gross_weight DECIMAL(10,3),
            hallmark_net_weight DECIMAL(10,3),
            images TEXT,
            created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_date TIMESTAMP,
            CONSTRAINT fk_pledged_items_gold_loans FOREIGN KEY (gold_loan_id) REFERENCES gold_loans(id) ON DELETE CASCADE,
            CONSTRAINT fk_pledged_items_item_types FOREIGN KEY (item_type_id) REFERENCES item_types(id) ON DELETE RESTRICT,
            CONSTRAINT fk_pledged_items_purities FOREIGN KEY (purity_id) REFERENCES purities(id) ON DELETE RESTRICT
        );

        CREATE INDEX IF NOT EXISTS ix_pledged_items_gold_loan_id ON pledged_items(gold_loan_id);
        CREATE INDEX IF NOT EXISTS ix_pledged_items_item_type_id ON pledged_items(item_type_id);
        CREATE INDEX IF NOT EXISTS ix_pledged_items_purity_id ON pledged_items(purity_id);
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        DROP TABLE IF EXISTS pledged_items;
        DROP TABLE IF EXISTS gold_loans;
    ");
        }
    }
}
