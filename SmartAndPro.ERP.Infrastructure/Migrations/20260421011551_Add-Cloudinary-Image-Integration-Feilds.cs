using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudinaryImageIntegrationFeilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "RoomType",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoPublicId",
                table: "RoomType",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "RoomType",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoPublicId",
                table: "ReservationService",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "ReservationService",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoPublicId",
                table: "Hotel",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Hotel",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "CompanyDetails",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StampUrl",
                table: "CompanyDetails",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Base64Url",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "RoomType");

            migrationBuilder.DropColumn(
                name: "LogoPublicId",
                table: "RoomType");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "RoomType");

            migrationBuilder.DropColumn(
                name: "LogoPublicId",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "LogoPublicId",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "StampUrl",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "Base64Url",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Assets");
        }
    }
}
