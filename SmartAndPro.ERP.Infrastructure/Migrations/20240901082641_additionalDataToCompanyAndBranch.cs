using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class additionalDataToCompanyAndBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "BranchDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "BranchDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "BranchDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "BranchDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "BranchDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "BranchDetails");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "BranchDetails");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "BranchDetails");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "BranchDetails");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "BranchDetails");
        }
    }
}
