using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PdfTemplateConfigurationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PdfTemplateConfiguration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Header_AddressSubfields = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Footer_AddressSubfields = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdfTemplateConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Footer_CustomTexts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footer_CustomTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Footer_CustomTexts_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                        column: x => x.PdfTemplateConfigurationId,
                        principalTable: "PdfTemplateConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Footer_Left",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                name: "Header_CustomTexts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Header_CustomTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Header_CustomTexts_PdfTemplateConfiguration_PdfTemplateConfigurationId",
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
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Align = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfTemplateConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                name: "IX_Footer_CustomTexts_PdfTemplateConfigurationId",
                table: "Footer_CustomTexts",
                column: "PdfTemplateConfigurationId");

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
                name: "IX_Header_CustomTexts_PdfTemplateConfigurationId",
                table: "Header_CustomTexts",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Footer_CustomTexts");

            migrationBuilder.DropTable(
                name: "Footer_Left");

            migrationBuilder.DropTable(
                name: "Footer_Middle");

            migrationBuilder.DropTable(
                name: "Footer_Right");

            migrationBuilder.DropTable(
                name: "Header_CustomTexts");

            migrationBuilder.DropTable(
                name: "Header_Left");

            migrationBuilder.DropTable(
                name: "Header_Middle");

            migrationBuilder.DropTable(
                name: "Header_Right");

            migrationBuilder.DropTable(
                name: "PdfTemplateConfiguration");
        }
    }
}
