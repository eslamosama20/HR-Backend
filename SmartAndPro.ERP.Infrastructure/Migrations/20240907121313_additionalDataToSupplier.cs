using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class additionalDataToSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "SupplierDetails");
        }
    }
}
