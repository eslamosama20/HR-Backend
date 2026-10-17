using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTameenAndZidIntegrationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyAllocationLine_MainPurchaseClass_MainPurchaseClassId",
                table: "PharmacyAllocationLine");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyInsuranceCompany_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyPrescription_PrescriptionId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPrescriptionItem_MainPurchaseClass_ItemId",
                table: "PharmacyPrescriptionItem");

            migrationBuilder.DropTable(
                name: "ZidAuthTokens");

            migrationBuilder.DropTable(
                name: "ZidOrderItems");

            migrationBuilder.DropTable(
                name: "ZidProductMappings");

            migrationBuilder.DropTable(
                name: "ZidSyncLogs");

            migrationBuilder.DropTable(
                name: "ZidOrders");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyPrescriptionItem_ItemId",
                table: "PharmacyPrescriptionItem");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInsuranceClaim_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInsuranceClaim_NphiesClaimId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInsuranceClaim_PrescriptionId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyAllocationLine_MainPurchaseClassId",
                table: "PharmacyAllocationLine");

            migrationBuilder.DropColumn(
                name: "CashierPaymentId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "ClaimResponseJson",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "CoveragePercentage",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "InsuranceCompanyId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "InsuranceCoveredAmount",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "InsuranceMemberNo",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "InsurancePolicyNo",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "NphiesAdjudicatedAt",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "NphiesClaimId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "NphiesStatus",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "NphiesSubmittedAt",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "PatientShareAmount",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "PrescriptionId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "SaleInvoiceId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "PharmacyPrescriptionItem");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "PharmacyPrescriptionItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.DropColumn(
                name: "MainPurchaseClassId",
                table: "PharmacyAllocationLine");

            migrationBuilder.AddColumn<int>(
                name: "MainPurchaseClassId",
                table: "PharmacyAllocationLine",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
