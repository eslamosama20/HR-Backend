using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanyMultiPhonesAndBankAccountsAndPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "BankBranchName",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "PolicyParagraph",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "SwiftCode",
                table: "CompanyDetails");

            migrationBuilder.AddColumn<byte[]>(
                name: "Stamp",
                table: "CompanyDetails",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BankAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankBranchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SwiftCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccount_CompanyDetails_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyPhones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyPhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyPhones_CompanyDetails_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyPolicy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyParagraph = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyPolicy_CompanyDetails_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "CompanyDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_CompanyId",
                table: "BankAccount",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyPhones_CompanyId",
                table: "CompanyPhones",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyPolicy_CompanyId",
                table: "CompanyPolicy",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccount");

            migrationBuilder.DropTable(
                name: "CompanyPhones");

            migrationBuilder.DropTable(
                name: "CompanyPolicy");

            migrationBuilder.DropColumn(
                name: "Stamp",
                table: "CompanyDetails");

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBranchName",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolicyParagraph",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SwiftCode",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
