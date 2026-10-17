using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TreasuryFrontal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaycheckReceipts_Treasuries_TreasuryId",
                table: "PaycheckReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Treasuries_TreasuryId",
                table: "PaymentReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Treasuries_MainAccount_MainAccountId",
                table: "Treasuries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Treasuries",
                table: "Treasuries");

            migrationBuilder.RenameTable(
                name: "Treasuries",
                newName: "Treasury");

            migrationBuilder.RenameIndex(
                name: "IX_Treasuries_MainAccountId",
                table: "Treasury",
                newName: "IX_Treasury_MainAccountId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Treasury",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Treasury",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "Treasury",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Treasury",
                table: "Treasury",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Treasury_Name",
                table: "Treasury",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaycheckReceipts_Treasury_TreasuryId",
                table: "PaycheckReceipts",
                column: "TreasuryId",
                principalTable: "Treasury",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReceipts_Treasury_TreasuryId",
                table: "PaymentReceipts",
                column: "TreasuryId",
                principalTable: "Treasury",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Treasury_MainAccount_MainAccountId",
                table: "Treasury",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaycheckReceipts_Treasury_TreasuryId",
                table: "PaycheckReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Treasury_TreasuryId",
                table: "PaymentReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Treasury_MainAccount_MainAccountId",
                table: "Treasury");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Treasury",
                table: "Treasury");

            migrationBuilder.DropIndex(
                name: "IX_Treasury_Name",
                table: "Treasury");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "Treasury");

            migrationBuilder.RenameTable(
                name: "Treasury",
                newName: "Treasuries");

            migrationBuilder.RenameIndex(
                name: "IX_Treasury_MainAccountId",
                table: "Treasuries",
                newName: "IX_Treasuries_MainAccountId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Treasuries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Treasuries",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Treasuries",
                table: "Treasuries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaycheckReceipts_Treasuries_TreasuryId",
                table: "PaycheckReceipts",
                column: "TreasuryId",
                principalTable: "Treasuries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReceipts_Treasuries_TreasuryId",
                table: "PaymentReceipts",
                column: "TreasuryId",
                principalTable: "Treasuries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Treasuries_MainAccount_MainAccountId",
                table: "Treasuries",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");
        }
    }
}
