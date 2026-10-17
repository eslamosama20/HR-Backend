using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNphiesFieldsToInsuranceClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<Guid>(
                name: "CashierPaymentId",
                table: "PharmacyInsuranceClaim",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimResponseJson",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoveragePercentage",
                table: "PharmacyInsuranceClaim",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "InsuranceCompanyId",
                table: "PharmacyInsuranceClaim",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceCoveredAmount",
                table: "PharmacyInsuranceClaim",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceMemberNo",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsurancePolicyNo",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NphiesAdjudicatedAt",
                table: "PharmacyInsuranceClaim",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NphiesClaimId",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NphiesStatus",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NphiesSubmittedAt",
                table: "PharmacyInsuranceClaim",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PatientShareAmount",
                table: "PharmacyInsuranceClaim",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PrescriptionId",
                table: "PharmacyInsuranceClaim",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleInvoiceId",
                table: "PharmacyInsuranceClaim",
                type: "int",
                nullable: true);


            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceClaim_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim",
                column: "InsuranceCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceClaim_NphiesClaimId",
                table: "PharmacyInsuranceClaim",
                column: "NphiesClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceClaim_PrescriptionId",
                table: "PharmacyInsuranceClaim",
                column: "PrescriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyInsuranceCompany_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim",
                column: "InsuranceCompanyId",
                principalTable: "PharmacyInsuranceCompany",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyPrescription_PrescriptionId",
                table: "PharmacyInsuranceClaim",
                column: "PrescriptionId",
                principalTable: "PharmacyPrescription",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyInsuranceCompany_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyPrescription_PrescriptionId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInsuranceClaim_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInsuranceClaim_NphiesClaimId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInsuranceClaim_PrescriptionId",
                table: "PharmacyInsuranceClaim");

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

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "PharmacyPrescriptionItem",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

        }
    }
}
