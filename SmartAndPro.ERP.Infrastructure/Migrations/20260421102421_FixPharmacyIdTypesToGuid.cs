using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPharmacyIdTypesToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "PharmacyPrescriptionItem");

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "PharmacyPrescriptionItem",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "MainPurchaseClassId",
                table: "PharmacyAllocationLine");

            migrationBuilder.AddColumn<Guid>(
                name: "MainPurchaseClassId",
                table: "PharmacyAllocationLine",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "PharmacyPrescriptionItem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MainPurchaseClassId",
                table: "PharmacyAllocationLine",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
