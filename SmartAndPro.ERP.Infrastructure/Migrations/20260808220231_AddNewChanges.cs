using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Commented out since columns already exist in the database
            /*
            migrationBuilder.AddColumn<int>(
                name: "MaxBanksCount",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxBranchesCount",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCompaniesCount",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCustomersCount",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxStoresCount",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxSuppliersCount",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxTreasuriesCount",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsersCount",
                table: "Roles",
                type: "int",
                nullable: true);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxBanksCount",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "MaxBranchesCount",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "MaxCompaniesCount",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "MaxCustomersCount",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "MaxStoresCount",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "MaxSuppliersCount",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "MaxTreasuriesCount",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "MaxUsersCount",
                table: "Roles");
        }
    }
}
