using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class reservationTableTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReservationPrintTableTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationPrintTableTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrintTableColumn",
                columns: table => new
                {
                    ReservationPrintTableTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColumnName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Alignment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintTableColumn", x => new { x.ReservationPrintTableTemplateId, x.Id });
                    table.ForeignKey(
                        name: "FK_PrintTableColumn_ReservationPrintTableTemplates_ReservationPrintTableTemplateId",
                        column: x => x.ReservationPrintTableTemplateId,
                        principalTable: "ReservationPrintTableTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrintTableColumn");

            migrationBuilder.DropTable(
                name: "ReservationPrintTableTemplates");
        }
    }
}
