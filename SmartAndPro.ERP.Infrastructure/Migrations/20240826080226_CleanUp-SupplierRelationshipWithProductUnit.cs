using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanUpSupplierRelationshipWithProductUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginActionLogs");

            migrationBuilder.DropTable(
                name: "OTPs");

            migrationBuilder.DropTable(
                name: "passwordsLogs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "OTPStatuses");

            migrationBuilder.DropTable(
                name: "OTPType");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SupplierDetails",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 274, DateTimeKind.Utc).AddTicks(2893));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StoreDetails",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 273, DateTimeKind.Utc).AddTicks(1062));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductUnit",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 271, DateTimeKind.Utc).AddTicks(9013));

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierDetailsId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductDetails",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 271, DateTimeKind.Utc).AddTicks(1114));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductBatchToUnitAssignments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 268, DateTimeKind.Utc).AddTicks(9509));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MeasurementUnit",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 21, 36, 3, 267, DateTimeKind.Local).AddTicks(9512));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CompanyDetails",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 264, DateTimeKind.Utc).AddTicks(7524));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchDetails",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 263, DateTimeKind.Utc).AddTicks(4247));

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_SupplierDetailsId",
                table: "ProductUnit",
                column: "SupplierDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_SupplierDetails_SupplierDetailsId",
                table: "ProductUnit",
                column: "SupplierDetailsId",
                principalTable: "SupplierDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_SupplierDetails_SupplierDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_SupplierDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "SupplierDetailsId",
                table: "ProductUnit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SupplierDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 274, DateTimeKind.Utc).AddTicks(2893),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "StoreDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 273, DateTimeKind.Utc).AddTicks(1062),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 271, DateTimeKind.Utc).AddTicks(9013),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 271, DateTimeKind.Utc).AddTicks(1114),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductBatchToUnitAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 268, DateTimeKind.Utc).AddTicks(9509),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MeasurementUnit",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 21, 36, 3, 267, DateTimeKind.Local).AddTicks(9512),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CompanyDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 264, DateTimeKind.Utc).AddTicks(7524),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "BranchDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 31, 18, 36, 3, 263, DateTimeKind.Utc).AddTicks(4247),
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "LoginActionLogs",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsFailedTrail = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginActionLogs", x => new { x.UserId, x.LoginTime });
                });

            migrationBuilder.CreateTable(
                name: "OTPStatuses",
                columns: table => new
                {
                    StatusID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPStatuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "OTPType",
                columns: table => new
                {
                    ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPType", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "passwordsLogs",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwordsLogs", x => new { x.UserId, x.CreatedAt });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArabicFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArabicLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArabicMiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnglishFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishMiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_CompanyDetails_CompanyDetailsId",
                        column: x => x.CompanyDetailsId,
                        principalTable: "CompanyDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OTPs",
                columns: table => new
                {
                    OTPReference = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OtpTypeID = table.Column<short>(type: "smallint", nullable: true),
                    StatusID = table.Column<short>(type: "smallint", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    NumberOfFailedTrails = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    OTPPayload = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OTPReferenceKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPs", x => x.OTPReference);
                    table.ForeignKey(
                        name: "FK_OTPs_OTPStatuses_StatusID",
                        column: x => x.StatusID,
                        principalTable: "OTPStatuses",
                        principalColumn: "StatusID");
                    table.ForeignKey(
                        name: "FK_OTPs_OTPType_OtpTypeID",
                        column: x => x.OtpTypeID,
                        principalTable: "OTPType",
                        principalColumn: "ID");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_OTPs_OtpTypeID",
                table: "OTPs",
                column: "OtpTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_OTPs_StatusID",
                table: "OTPs",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyDetailsId",
                table: "Users",
                column: "CompanyDetailsId");
        }
    }
}
