using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddpaginationAndEnhancments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "SaleInvoice",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "ReservationSupplier",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "ReservationService",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "ReservationService",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "ReservationService",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ReservationService",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "ReservationService",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "ReservationService",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaxNumber",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "ReservationCompany",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerReservationNumber",
                table: "MealReservation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MealReservation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "MealReservation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Hotel",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

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

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "CustomerReservationNumber",
                table: "MealReservation");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MealReservation");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "MealReservation");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Hotel");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "SaleInvoice",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "TaxNumber",
                table: "ReservationCompany",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
