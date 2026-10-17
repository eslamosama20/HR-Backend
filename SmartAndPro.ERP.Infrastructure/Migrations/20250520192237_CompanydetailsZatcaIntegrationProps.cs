using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanydetailsZatcaIntegrationProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BinarySecurityToken",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Csr",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DispositionMessage",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnBoarding",
                table: "CompanyDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrivateKey",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RequestId",
                table: "CompanyDetails",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Secret",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BinarySecurityToken",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "Csr",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "DispositionMessage",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "IsOnBoarding",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "PrivateKey",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "Secret",
                table: "CompanyDetails");
        }
    }
}
