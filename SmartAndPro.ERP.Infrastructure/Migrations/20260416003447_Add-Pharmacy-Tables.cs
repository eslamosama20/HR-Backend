using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
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
