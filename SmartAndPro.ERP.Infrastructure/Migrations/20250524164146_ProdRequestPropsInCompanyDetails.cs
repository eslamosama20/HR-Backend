using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProdRequestPropsInCompanyDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProdBinarySecurityToken",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProdDispositionMessage",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProdRequestId",
                table: "CompanyDetails",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProdSecret",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProdBinarySecurityToken",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "ProdDispositionMessage",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "ProdRequestId",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "ProdSecret",
                table: "CompanyDetails");
        }
    }
}
