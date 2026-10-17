using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankAccountEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneralBankAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BankBranchName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SwiftCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsFrontal = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralBankAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralBankAccount_MainAccount_MainAccountId",
                        column: x => x.MainAccountId,
                        principalTable: "MainAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralBankAccount_BankAccountNumber",
                table: "GeneralBankAccount",
                column: "BankAccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneralBankAccount_MainAccountId",
                table: "GeneralBankAccount",
                column: "MainAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralBankAccount");
        }
    }
}
