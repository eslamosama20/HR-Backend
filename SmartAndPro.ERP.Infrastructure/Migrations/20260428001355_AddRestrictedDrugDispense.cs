using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRestrictedDrugDispense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestrictedDrugDispense",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DoctorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PharmacistName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AuthorizationNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DispenseReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RestrictionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DispensedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestrictedDrugDispense", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestrictedDrugDispense_ItemCode",
                table: "RestrictedDrugDispense",
                column: "ItemCode");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictedDrugDispense_Status",
                table: "RestrictedDrugDispense",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictedDrugDispense_StoreDetailsId",
                table: "RestrictedDrugDispense",
                column: "StoreDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestrictedDrugDispense");
        }
    }
}
