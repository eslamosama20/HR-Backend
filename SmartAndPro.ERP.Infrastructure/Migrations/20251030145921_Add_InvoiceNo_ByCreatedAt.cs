using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_InvoiceNo_ByCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SaleInvoice_InvoiceNo' AND object_id = OBJECT_ID('SaleInvoice'))
                    DROP INDEX IX_SaleInvoice_InvoiceNo ON SaleInvoice;

                ;WITH Ordered AS (
                    SELECT 
                        Id,
                        CASE WHEN IsFrontal = 1 THEN 1 ELSE 0 END AS IsFrontal,
                        ROW_NUMBER() OVER (
                            PARTITION BY CASE WHEN IsFrontal = 1 THEN 1 ELSE 0 END 
                            ORDER BY CreatedAt
                        ) AS RowNum
                    FROM SaleInvoice
                )
                UPDATE s
                SET s.InvoiceNo = 
                    CASE 
                        WHEN o.IsFrontal = 1 THEN 'FINV-' + CAST(o.RowNum AS NVARCHAR(10))
                        ELSE 'INV-' + CAST(o.RowNum AS NVARCHAR(10))
                    END
                FROM SaleInvoice s
                INNER JOIN Ordered o ON s.Id = o.Id;
            ");


            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "SaleInvoice",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

           

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_InvoiceNo",
                table: "SaleInvoice",
                column: "InvoiceNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_InvoiceNo",
                table: "SaleInvoice");

          
            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "SaleInvoice",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
