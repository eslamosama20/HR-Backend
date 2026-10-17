using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyReturnReferenceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "PharmacyReturn",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNo",
                table: "PharmacyReturn",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "PharmacyReturn");

            migrationBuilder.DropColumn(
                name: "ReferenceNo",
                table: "PharmacyReturn");
        }
    }
}
