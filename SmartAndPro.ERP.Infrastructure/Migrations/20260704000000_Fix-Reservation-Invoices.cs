using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReservationInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_CustomerProfile_ArabicName",
            //    table: "CustomerProfile");

            migrationBuilder.AddColumn<Guid>(
                name: "SalesClassId",
                table: "RoomType",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesClassId",
                table: "ReservationView",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesClassId",
                table: "ReservationService",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesClassId",
                table: "ReservationMeal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalContactNumber",
                table: "ReservationCompany",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "ReservationCompany",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ReservationCompany",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "ReservationCompany",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerProfileId",
                table: "ReservationCompany",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "ReservationCompany",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GaztNumber",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "ReservationCompany",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortAddress",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubNumber",
                table: "ReservationCompany",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "ReservationCompany",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReservationItem",
                table: "MainSalesClass",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReservationCompany",
                table: "CustomerProfile",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubNumber",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCompany_CustomerProfileId",
                table: "ReservationCompany",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfile_ArabicName_IsReservationCompany",
                table: "CustomerProfile",
                columns: new[] { "ArabicName", "IsReservationCompany" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationCompany_CustomerProfile_CustomerProfileId",
                table: "ReservationCompany",
                column: "CustomerProfileId",
                principalTable: "CustomerProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationCompany_CustomerProfile_CustomerProfileId",
                table: "ReservationCompany");

            migrationBuilder.DropIndex(
                name: "IX_ReservationCompany_CustomerProfileId",
                table: "ReservationCompany");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfile_ArabicName_IsReservationCompany",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "SalesClassId",
                table: "RoomType");

            migrationBuilder.DropColumn(
                name: "SalesClassId",
                table: "ReservationView");

            migrationBuilder.DropColumn(
                name: "SalesClassId",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "SalesClassId",
                table: "ReservationMeal");

            migrationBuilder.DropColumn(
                name: "AdditionalContactNumber",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "CustomerProfileId",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "District",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "GaztNumber",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "ShortAddress",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "SubNumber",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "IsReservationItem",
                table: "MainSalesClass");

            migrationBuilder.DropColumn(
                name: "IsReservationCompany",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "SubNumber",
                table: "CustomerProfile");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfile_ArabicName",
                table: "CustomerProfile",
                column: "ArabicName",
                unique: true);
        }
    }
}
