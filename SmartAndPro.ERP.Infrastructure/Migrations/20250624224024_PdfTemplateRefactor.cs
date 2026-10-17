using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PdfTemplateRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Footer_Left");

            migrationBuilder.DropTable(
                name: "Footer_Middle");

            migrationBuilder.DropTable(
                name: "Footer_Right");

            migrationBuilder.DropTable(
                name: "Header_Left");

            migrationBuilder.DropTable(
                name: "Header_Middle");

            migrationBuilder.DropTable(
                name: "Header_Right");

            migrationBuilder.CreateTable(
                name: "Footer_Columns",
                columns: table => new
                {
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footer_Columns", x => new { x.PdfTemplateConfigurationId, x.Id });
                    table.ForeignKey(
                        name: "FK_Footer_Columns_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                        column: x => x.PdfTemplateConfigurationId,
                        principalTable: "PdfTemplateConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Header_Columns",
                columns: table => new
                {
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Header_Columns", x => new { x.PdfTemplateConfigurationId, x.Id });
                    table.ForeignKey(
                        name: "FK_Header_Columns_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                        column: x => x.PdfTemplateConfigurationId,
                        principalTable: "PdfTemplateConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Footer_Rows",
                columns: table => new
                {
                    PdfColumnGroupPdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PdfColumnGroupId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footer_Rows", x => new { x.PdfColumnGroupPdfTemplateConfigurationId, x.PdfColumnGroupId, x.Id });
                    table.ForeignKey(
                        name: "FK_Footer_Rows_Footer_Columns_PdfColumnGroupPdfTemplateConfigurationId_PdfColumnGroupId",
                        columns: x => new { x.PdfColumnGroupPdfTemplateConfigurationId, x.PdfColumnGroupId },
                        principalTable: "Footer_Columns",
                        principalColumns: new[] { "PdfTemplateConfigurationId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Header_Rows",
                columns: table => new
                {
                    PdfColumnGroupPdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PdfColumnGroupId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Header_Rows", x => new { x.PdfColumnGroupPdfTemplateConfigurationId, x.PdfColumnGroupId, x.Id });
                    table.ForeignKey(
                        name: "FK_Header_Rows_Header_Columns_PdfColumnGroupPdfTemplateConfigurationId_PdfColumnGroupId",
                        columns: x => new { x.PdfColumnGroupPdfTemplateConfigurationId, x.PdfColumnGroupId },
                        principalTable: "Header_Columns",
                        principalColumns: new[] { "PdfTemplateConfigurationId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Footer_Items",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PdfRowGroupPdfColumnGroupId = table.Column<int>(type: "int", nullable: false),
                    PdfRowGroupId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "field"),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "inherit"),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "left"),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footer_Items", x => new { x.PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId, x.PdfRowGroupPdfColumnGroupId, x.PdfRowGroupId, x.Id });
                    table.ForeignKey(
                        name: "FK_Footer_Items_Footer_Rows_PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupId_PdfRowGroupId",
                        columns: x => new { x.PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId, x.PdfRowGroupPdfColumnGroupId, x.PdfRowGroupId },
                        principalTable: "Footer_Rows",
                        principalColumns: new[] { "PdfColumnGroupPdfTemplateConfigurationId", "PdfColumnGroupId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Header_Items",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PdfRowGroupPdfColumnGroupId = table.Column<int>(type: "int", nullable: false),
                    PdfRowGroupId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "field"),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "inherit"),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "left"),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Header_Items", x => new { x.PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId, x.PdfRowGroupPdfColumnGroupId, x.PdfRowGroupId, x.Id });
                    table.ForeignKey(
                        name: "FK_Header_Items_Header_Rows_PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupId_PdfRowGroupId",
                        columns: x => new { x.PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId, x.PdfRowGroupPdfColumnGroupId, x.PdfRowGroupId },
                        principalTable: "Header_Rows",
                        principalColumns: new[] { "PdfColumnGroupPdfTemplateConfigurationId", "PdfColumnGroupId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Footer_Items");

            migrationBuilder.DropTable(
                name: "Header_Items");

            migrationBuilder.DropTable(
                name: "Footer_Rows");

            migrationBuilder.DropTable(
                name: "Header_Rows");

            migrationBuilder.DropTable(
                name: "Footer_Columns");

            migrationBuilder.DropTable(
                name: "Header_Columns");

            migrationBuilder.CreateTable(
                name: "Footer_Left",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footer_Left", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Footer_Left_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                        column: x => x.PdfTemplateConfigurationId,
                        principalTable: "PdfTemplateConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Footer_Middle",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footer_Middle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Footer_Middle_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                        column: x => x.PdfTemplateConfigurationId,
                        principalTable: "PdfTemplateConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Footer_Right",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footer_Right", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Footer_Right_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                        column: x => x.PdfTemplateConfigurationId,
                        principalTable: "PdfTemplateConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Header_Left",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Header_Left", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Header_Left_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                        column: x => x.PdfTemplateConfigurationId,
                        principalTable: "PdfTemplateConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Header_Middle",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Header_Middle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Header_Middle_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                        column: x => x.PdfTemplateConfigurationId,
                        principalTable: "PdfTemplateConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Header_Right",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Header_Right", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Header_Right_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                        column: x => x.PdfTemplateConfigurationId,
                        principalTable: "PdfTemplateConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Footer_Left_PdfTemplateConfigurationId",
                table: "Footer_Left",
                column: "PdfTemplateConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Footer_Middle_PdfTemplateConfigurationId",
                table: "Footer_Middle",
                column: "PdfTemplateConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Footer_Right_PdfTemplateConfigurationId",
                table: "Footer_Right",
                column: "PdfTemplateConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Header_Left_PdfTemplateConfigurationId",
                table: "Header_Left",
                column: "PdfTemplateConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Header_Middle_PdfTemplateConfigurationId",
                table: "Header_Middle",
                column: "PdfTemplateConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Header_Right_PdfTemplateConfigurationId",
                table: "Header_Right",
                column: "PdfTemplateConfigurationId");
        }
    }
}
