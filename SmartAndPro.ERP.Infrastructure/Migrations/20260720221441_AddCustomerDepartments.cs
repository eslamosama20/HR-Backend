using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_ReferenceNumber",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PaycheckReceipts_ReferenceNumber",
                table: "PaycheckReceipts");

            migrationBuilder.AddColumn<bool>(
                name: "IsForProjects",
                table: "CustomerProfile",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CustomerDepartments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDepartments_CustomerProfile_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_ReferenceNumber",
                table: "PaymentReceipts",
                column: "ReferenceNumber",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckReceipts_ReferenceNumber",
                table: "PaycheckReceipts",
                column: "ReferenceNumber",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDepartments_CustomerProfileId",
                table: "CustomerDepartments",
                column: "CustomerProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDepartments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_ReferenceNumber",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PaycheckReceipts_ReferenceNumber",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "IsForProjects",
                table: "CustomerProfile");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_ReferenceNumber",
                table: "PaymentReceipts",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckReceipts_ReferenceNumber",
                table: "PaycheckReceipts",
                column: "ReferenceNumber",
                unique: true);
        }
    }
}
