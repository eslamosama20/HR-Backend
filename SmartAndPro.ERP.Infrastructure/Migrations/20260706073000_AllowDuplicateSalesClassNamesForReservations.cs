using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowDuplicateSalesClassNamesForReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
                name: "IX_MainSalesClass_ArabicName",
                table: "MainSalesClass");

            migrationBuilder.DropIndex(
                name: "IX_MainSalesClass_EnglishName",
                table: "MainSalesClass");

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_ArabicName_IsReservationItem",
                table: "MainSalesClass",
                columns: new[] { "ArabicName", "IsReservationItem" },
                unique: true,
                filter: "[ArabicName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_EnglishName_IsReservationItem",
                table: "MainSalesClass",
                columns: new[] { "EnglishName", "IsReservationItem" },
                unique: true,
                filter: "[EnglishName] IS NOT NULL");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MainSalesClass_ArabicName_IsReservationItem",
                table: "MainSalesClass");

            migrationBuilder.DropIndex(
                name: "IX_MainSalesClass_EnglishName_IsReservationItem",
                table: "MainSalesClass");

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_ArabicName",
                table: "MainSalesClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_EnglishName",
                table: "MainSalesClass",
                column: "EnglishName",
                unique: true);
        }
    }
}
