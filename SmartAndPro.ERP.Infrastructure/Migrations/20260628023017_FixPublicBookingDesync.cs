using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPublicBookingDesync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_IsQuotation_SaleInvoiceType",
                table: "SaleInvoice");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_SaleInvoiceType",
                table: "SaleInvoice",
                column: "SaleInvoiceType");

            migrationBuilder.CreateTable(
                name: "PublicPaidServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ServiceTypeCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicPaidServices", x => x.Id);
                });

            migrationBuilder.AddColumn<string>(
                name: "CancellationPolicy",
                table: "PublicRoomTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationPolicyAr",
                table: "PublicRoomTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MunicipalityTaxPercentage",
                table: "PublicRoomTypes",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "PayAtHotel",
                table: "PublicRoomTypes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "PayNow",
                table: "PublicRoomTypes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ViewPrice",
                table: "PublicRoomTypes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_SaleInvoiceType",
                table: "SaleInvoice");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_IsQuotation_SaleInvoiceType",
                table: "SaleInvoice",
                columns: new[] { "IsQuotation", "SaleInvoiceType" });

            migrationBuilder.DropTable(
                name: "PublicPaidServices");

            migrationBuilder.DropColumn(
                name: "CancellationPolicy",
                table: "PublicRoomTypes");

            migrationBuilder.DropColumn(
                name: "CancellationPolicyAr",
                table: "PublicRoomTypes");

            migrationBuilder.DropColumn(
                name: "MunicipalityTaxPercentage",
                table: "PublicRoomTypes");

            migrationBuilder.DropColumn(
                name: "PayAtHotel",
                table: "PublicRoomTypes");

            migrationBuilder.DropColumn(
                name: "PayNow",
                table: "PublicRoomTypes");

            migrationBuilder.DropColumn(
                name: "ViewPrice",
                table: "PublicRoomTypes");
        }
    }
}
