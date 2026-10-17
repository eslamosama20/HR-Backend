using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePharmacyEntitiesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            
            
            migrationBuilder.CreateTable(
                name: "PharmacyDoctor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeqNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LicenseNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyDoctor", x => x.Id);
                });

            
            
            migrationBuilder.CreateTable(
                name: "PharmacyInsuranceCompany",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeqNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultCoveragePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyInsuranceCompany", x => x.Id);
                });

            
            
            
            migrationBuilder.CreateTable(
                name: "PharmacyAttachment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeqNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyAttachment_PharmacyPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "PharmacyPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyPrescriptionItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeqNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionRxNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DispensedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoreId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllowSubstitute = table.Column<bool>(type: "bit", nullable: false),
                    IsControlled = table.Column<bool>(type: "bit", nullable: false),
                    AvailabilityStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyPrescriptionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyPrescriptionItem_PharmacyPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "PharmacyPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAttachment_PrescriptionId",
                table: "PharmacyAttachment",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAttachment_SeqNo",
                table: "PharmacyAttachment",
                column: "SeqNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDoctor_LicenseNo",
                table: "PharmacyDoctor",
                column: "LicenseNo");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDoctor_Name",
                table: "PharmacyDoctor",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDoctor_SeqNo",
                table: "PharmacyDoctor",
                column: "SeqNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceCompany_Name",
                table: "PharmacyInsuranceCompany",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceCompany_SeqNo",
                table: "PharmacyInsuranceCompany",
                column: "SeqNo",
                unique: true);

            
            
            
            
            
            
            
            
            
            
            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPrescriptionItem_PrescriptionId",
                table: "PharmacyPrescriptionItem",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPrescriptionItem_SeqNo",
                table: "PharmacyPrescriptionItem",
                column: "SeqNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PharmacyAlert");

            migrationBuilder.DropTable(
                name: "PharmacyAttachment");

            migrationBuilder.DropTable(
                name: "PharmacyBatch");

            migrationBuilder.DropTable(
                name: "PharmacyControlledDrugRecord");

            migrationBuilder.DropTable(
                name: "PharmacyDrugInteraction");

            migrationBuilder.DropTable(
                name: "PharmacyInsuranceClaim");

            migrationBuilder.DropTable(
                name: "PharmacyPrescriptionItem");

            migrationBuilder.DropTable(
                name: "PharmacyReturn");

            migrationBuilder.DropTable(
                name: "PharmacyPrescription");

            migrationBuilder.DropTable(
                name: "PharmacyDoctor");

            migrationBuilder.DropTable(
                name: "PharmacyInsuranceCompany");

            migrationBuilder.DropTable(
                name: "PharmacyPatient");
        }
    }
}
