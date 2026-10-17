using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Recipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recipe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MainSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipe_MainSalesClass_MainSalesClassId",
                        column: x => x.MainSalesClassId,
                        principalTable: "MainSalesClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecipeEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeEntry_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecipeEntry_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_MainSalesClassId",
                table: "Recipe",
                column: "MainSalesClassId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeEntry_MainPurchaseClassId",
                table: "RecipeEntry",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeEntry_RecipeId",
                table: "RecipeEntry",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeEntry");

            migrationBuilder.DropTable(
                name: "Recipe");
        }
    }
}
