using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SupplierDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 414, DateTimeKind.Utc).AddTicks(5185),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 664, DateTimeKind.Utc).AddTicks(1004));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StoreDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 394, DateTimeKind.Utc).AddTicks(983),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 658, DateTimeKind.Utc).AddTicks(9644));

            migrationBuilder.AddColumn<string>(
                name: "StoreType",
                table: "StoreDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Store");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 380, DateTimeKind.Utc).AddTicks(6319),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 651, DateTimeKind.Utc).AddTicks(8542));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 366, DateTimeKind.Utc).AddTicks(1272),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 646, DateTimeKind.Utc).AddTicks(4928));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductBatchToUnitAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 358, DateTimeKind.Utc).AddTicks(1633),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 630, DateTimeKind.Utc).AddTicks(6628));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MeasurementUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 13, 43, 7, 355, DateTimeKind.Local).AddTicks(63),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 25, 22, 55, 33, 626, DateTimeKind.Local).AddTicks(1648));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CompanyDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 344, DateTimeKind.Utc).AddTicks(9830),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 600, DateTimeKind.Utc).AddTicks(1412));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 340, DateTimeKind.Utc).AddTicks(3667),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 596, DateTimeKind.Utc).AddTicks(5199));

            migrationBuilder.InsertData(
                table: "OTPStatuses",
                columns: new[] { "StatusID", "ArabicDescription", "EnglishDescription" },
                values: new object[,]
                {
                    { (short)1, "قيد الانتظار", "pending" },
                    { (short)2, "نجاح", "success" },
                    { (short)3, "محظور", "blocked" }
                });

            migrationBuilder.InsertData(
                table: "OTPType",
                columns: new[] { "ID", "IsActive", "Name" },
                values: new object[,]
                {
                    { (short)1, true, "sign up" },
                    { (short)2, true, "sign in" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OTPStatuses",
                keyColumn: "StatusID",
                keyValue: (short)1);

            migrationBuilder.DeleteData(
                table: "OTPStatuses",
                keyColumn: "StatusID",
                keyValue: (short)2);

            migrationBuilder.DeleteData(
                table: "OTPStatuses",
                keyColumn: "StatusID",
                keyValue: (short)3);

            migrationBuilder.DeleteData(
                table: "OTPType",
                keyColumn: "ID",
                keyValue: (short)1);

            migrationBuilder.DeleteData(
                table: "OTPType",
                keyColumn: "ID",
                keyValue: (short)2);

            migrationBuilder.DropColumn(
                name: "StoreType",
                table: "StoreDetails");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SupplierDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 664, DateTimeKind.Utc).AddTicks(1004),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 414, DateTimeKind.Utc).AddTicks(5185));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StoreDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 658, DateTimeKind.Utc).AddTicks(9644),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 394, DateTimeKind.Utc).AddTicks(983));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 651, DateTimeKind.Utc).AddTicks(8542),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 380, DateTimeKind.Utc).AddTicks(6319));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 646, DateTimeKind.Utc).AddTicks(4928),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 366, DateTimeKind.Utc).AddTicks(1272));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductBatchToUnitAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 630, DateTimeKind.Utc).AddTicks(6628),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 358, DateTimeKind.Utc).AddTicks(1633));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MeasurementUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 25, 22, 55, 33, 626, DateTimeKind.Local).AddTicks(1648),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 13, 43, 7, 355, DateTimeKind.Local).AddTicks(63));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CompanyDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 600, DateTimeKind.Utc).AddTicks(1412),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 344, DateTimeKind.Utc).AddTicks(9830));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 596, DateTimeKind.Utc).AddTicks(5199),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 6, 29, 10, 43, 7, 340, DateTimeKind.Utc).AddTicks(3667));
        }
    }
}
