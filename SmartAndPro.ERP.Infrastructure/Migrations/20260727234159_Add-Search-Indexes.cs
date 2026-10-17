using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "SupplierDetails",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaxNumber",
                table: "CustomerProfile",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "CustomerProfile",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDetails_ArabicName",
                table: "SupplierDetails",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDetails_ContactNumber",
                table: "SupplierDetails",
                column: "ContactNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDetails_EnglishName",
                table: "SupplierDetails",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDetails_TaxNumber",
                table: "SupplierDetails",
                column: "TaxNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRepresentative_ArabicName",
                table: "SalesRepresentative",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRepresentative_EnglishName",
                table: "SalesRepresentative",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RoomType_ArabicName",
                table: "RoomType",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RoomType_EnglishName",
                table: "RoomType",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationView_ArabicName",
                table: "ReservationView",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationView_EnglishName",
                table: "ReservationView",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSupplier_ArabicName",
                table: "ReservationSupplier",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSupplier_ContactNumber",
                table: "ReservationSupplier",
                column: "ContactNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSupplier_EnglishName",
                table: "ReservationSupplier",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSupplier_TaxNumber",
                table: "ReservationSupplier",
                column: "TaxNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationService_ArabicName",
                table: "ReservationService",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationService_EnglishName",
                table: "ReservationService",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationService_TaxNumber",
                table: "ReservationService",
                column: "TaxNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeason_ArabicName",
                table: "ReservationSeason",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeason_EnglishName",
                table: "ReservationSeason",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationMeal_ArabicName",
                table: "ReservationMeal",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationMeal_EnglishName",
                table: "ReservationMeal",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCompany_ArabicName",
                table: "ReservationCompany",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCompany_ContactNumber",
                table: "ReservationCompany",
                column: "ContactNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCompany_Email",
                table: "ReservationCompany",
                column: "Email",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCompany_EnglishName",
                table: "ReservationCompany",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCompany_TaxNumber",
                table: "ReservationCompany",
                column: "TaxNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Hotel_ArabicName",
                table: "Hotel",
                column: "ArabicName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Hotel_ContactNumber",
                table: "Hotel",
                column: "ContactNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Hotel_Email",
                table: "Hotel",
                column: "Email",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Hotel_EnglishName",
                table: "Hotel",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Hotel_TaxNumber",
                table: "Hotel",
                column: "TaxNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfile_ContactNumber",
                table: "CustomerProfile",
                column: "ContactNumber",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfile_EnglishName",
                table: "CustomerProfile",
                column: "EnglishName",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfile_TaxNumber",
                table: "CustomerProfile",
                column: "TaxNumber",
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierDetails_ArabicName",
                table: "SupplierDetails");

            migrationBuilder.DropIndex(
                name: "IX_SupplierDetails_ContactNumber",
                table: "SupplierDetails");

            migrationBuilder.DropIndex(
                name: "IX_SupplierDetails_EnglishName",
                table: "SupplierDetails");

            migrationBuilder.DropIndex(
                name: "IX_SupplierDetails_TaxNumber",
                table: "SupplierDetails");

            migrationBuilder.DropIndex(
                name: "IX_SalesRepresentative_ArabicName",
                table: "SalesRepresentative");

            migrationBuilder.DropIndex(
                name: "IX_SalesRepresentative_EnglishName",
                table: "SalesRepresentative");

            migrationBuilder.DropIndex(
                name: "IX_RoomType_ArabicName",
                table: "RoomType");

            migrationBuilder.DropIndex(
                name: "IX_RoomType_EnglishName",
                table: "RoomType");

            migrationBuilder.DropIndex(
                name: "IX_ReservationView_ArabicName",
                table: "ReservationView");

            migrationBuilder.DropIndex(
                name: "IX_ReservationView_EnglishName",
                table: "ReservationView");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSupplier_ArabicName",
                table: "ReservationSupplier");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSupplier_ContactNumber",
                table: "ReservationSupplier");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSupplier_EnglishName",
                table: "ReservationSupplier");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSupplier_TaxNumber",
                table: "ReservationSupplier");

            migrationBuilder.DropIndex(
                name: "IX_ReservationService_ArabicName",
                table: "ReservationService");

            migrationBuilder.DropIndex(
                name: "IX_ReservationService_EnglishName",
                table: "ReservationService");

            migrationBuilder.DropIndex(
                name: "IX_ReservationService_TaxNumber",
                table: "ReservationService");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeason_ArabicName",
                table: "ReservationSeason");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeason_EnglishName",
                table: "ReservationSeason");

            migrationBuilder.DropIndex(
                name: "IX_ReservationMeal_ArabicName",
                table: "ReservationMeal");

            migrationBuilder.DropIndex(
                name: "IX_ReservationMeal_EnglishName",
                table: "ReservationMeal");

            migrationBuilder.DropIndex(
                name: "IX_ReservationCompany_ArabicName",
                table: "ReservationCompany");

            migrationBuilder.DropIndex(
                name: "IX_ReservationCompany_ContactNumber",
                table: "ReservationCompany");

            migrationBuilder.DropIndex(
                name: "IX_ReservationCompany_Email",
                table: "ReservationCompany");

            migrationBuilder.DropIndex(
                name: "IX_ReservationCompany_EnglishName",
                table: "ReservationCompany");

            migrationBuilder.DropIndex(
                name: "IX_ReservationCompany_TaxNumber",
                table: "ReservationCompany");

            migrationBuilder.DropIndex(
                name: "IX_Hotel_ArabicName",
                table: "Hotel");

            migrationBuilder.DropIndex(
                name: "IX_Hotel_ContactNumber",
                table: "Hotel");

            migrationBuilder.DropIndex(
                name: "IX_Hotel_Email",
                table: "Hotel");

            migrationBuilder.DropIndex(
                name: "IX_Hotel_EnglishName",
                table: "Hotel");

            migrationBuilder.DropIndex(
                name: "IX_Hotel_TaxNumber",
                table: "Hotel");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfile_ContactNumber",
                table: "CustomerProfile");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfile_EnglishName",
                table: "CustomerProfile");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfile_TaxNumber",
                table: "CustomerProfile");

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "SupplierDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaxNumber",
                table: "CustomerProfile",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "CustomerProfile",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
