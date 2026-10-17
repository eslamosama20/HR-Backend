using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMorePrposForCompanydetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GaztNumber",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortAddress",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubNumber",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "District",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "GaztNumber",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "ShortAddress",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "SubNumber",
                table: "CompanyDetails");
        }
    }
}
