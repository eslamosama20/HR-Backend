using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addInvoiceTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "InvoiceTypeId",
                table: "Invoice",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "InvoiceTypes",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_InvoiceTypeId",
                table: "Invoice",
                column: "InvoiceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_InvoiceTypes_InvoiceTypeId",
                table: "Invoice",
                column: "InvoiceTypeId",
                principalTable: "InvoiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_InvoiceTypes_InvoiceTypeId",
                table: "Invoice");

            migrationBuilder.DropTable(
                name: "InvoiceTypes");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_InvoiceTypeId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceTypeId",
                table: "Invoice");
        }
    }
}
