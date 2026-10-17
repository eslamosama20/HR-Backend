using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingCostToInventoryPermit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: The ShippingCost column is added in the subsequent migration 'AddQrCode'.
            // This file is recreated as an empty migration to restore the migration chain
            // and fix the compilation error in the accompanied .Designer.cs file.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
