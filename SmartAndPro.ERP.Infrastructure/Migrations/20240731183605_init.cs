using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SupplierDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 274, DateTimeKind.Utc).AddTicks(2893),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 414, DateTimeKind.Utc).AddTicks(5185));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StoreDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 273, DateTimeKind.Utc).AddTicks(1062),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 394, DateTimeKind.Utc).AddTicks(983));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 271, DateTimeKind.Utc).AddTicks(9013),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 380, DateTimeKind.Utc).AddTicks(6319));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 271, DateTimeKind.Utc).AddTicks(1114),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 366, DateTimeKind.Utc).AddTicks(1272));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductBatchToUnitAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 268, DateTimeKind.Utc).AddTicks(9509),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 358, DateTimeKind.Utc).AddTicks(1633));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MeasurementUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 21, 36, 3, 267, DateTimeKind.Local).AddTicks(9512),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 13, 43, 7, 355, DateTimeKind.Local).AddTicks(63));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CompanyDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 264, DateTimeKind.Utc).AddTicks(7524),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 344, DateTimeKind.Utc).AddTicks(9830));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 263, DateTimeKind.Utc).AddTicks(4247),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 340, DateTimeKind.Utc).AddTicks(3667));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SupplierDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 414, DateTimeKind.Utc).AddTicks(5185),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 274, DateTimeKind.Utc).AddTicks(2893));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StoreDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 394, DateTimeKind.Utc).AddTicks(983),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 273, DateTimeKind.Utc).AddTicks(1062));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 380, DateTimeKind.Utc).AddTicks(6319),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 271, DateTimeKind.Utc).AddTicks(9013));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 366, DateTimeKind.Utc).AddTicks(1272),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 271, DateTimeKind.Utc).AddTicks(1114));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductBatchToUnitAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 358, DateTimeKind.Utc).AddTicks(1633),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 268, DateTimeKind.Utc).AddTicks(9509));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MeasurementUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 13, 43, 7, 355, DateTimeKind.Local).AddTicks(63),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 21, 36, 3, 267, DateTimeKind.Local).AddTicks(9512));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CompanyDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 344, DateTimeKind.Utc).AddTicks(9830),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 264, DateTimeKind.Utc).AddTicks(7524));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 340, DateTimeKind.Utc).AddTicks(3667),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 263, DateTimeKind.Utc).AddTicks(4247));
        }
    }
}
