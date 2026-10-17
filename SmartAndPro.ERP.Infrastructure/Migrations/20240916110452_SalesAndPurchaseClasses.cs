using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesAndPurchaseClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MainClass");

            migrationBuilder.DropTable(
                name: "DetailClass");

            migrationBuilder.DropTable(
                name: "HeaderClass");

            migrationBuilder.DropTable(
                name: "RootClass");

            migrationBuilder.CreateTable(
                name: "RootPurchaseClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootPurchaseClass", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RootSalesClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootSalesClass", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HeaderPurchaseClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RootPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeaderPurchaseClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeaderPurchaseClass_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HeaderPurchaseClass_RootPurchaseClass_RootPurchaseClassId",
                        column: x => x.RootPurchaseClassId,
                        principalTable: "RootPurchaseClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HeaderSalesClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RootSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeaderSalesClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeaderSalesClass_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HeaderSalesClass_RootSalesClass_RootSalesClassId",
                        column: x => x.RootSalesClassId,
                        principalTable: "RootSalesClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetailPurchaseClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HeaderPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailPurchaseClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailPurchaseClass_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetailPurchaseClass_HeaderPurchaseClass_HeaderPurchaseClassId",
                        column: x => x.HeaderPurchaseClassId,
                        principalTable: "HeaderPurchaseClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetailSalesClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HeaderSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailSalesClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailSalesClass_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetailSalesClass_HeaderSalesClass_HeaderSalesClassId",
                        column: x => x.HeaderSalesClassId,
                        principalTable: "HeaderSalesClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MainPurchaseClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DetailPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainPurchaseClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainPurchaseClass_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MainPurchaseClass_DetailPurchaseClass_DetailPurchaseClassId",
                        column: x => x.DetailPurchaseClassId,
                        principalTable: "DetailPurchaseClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MainSalesClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DetailSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainSalesClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainSalesClass_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MainSalesClass_DetailSalesClass_DetailSalesClassId",
                        column: x => x.DetailSalesClassId,
                        principalTable: "DetailSalesClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetailPurchaseClass_ArabicName",
                table: "DetailPurchaseClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailPurchaseClass_ClientProfileId",
                table: "DetailPurchaseClass",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPurchaseClass_EnglishName",
                table: "DetailPurchaseClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailPurchaseClass_HeaderPurchaseClassId",
                table: "DetailPurchaseClass",
                column: "HeaderPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailPurchaseClass_ReferenceCode",
                table: "DetailPurchaseClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailSalesClass_ArabicName",
                table: "DetailSalesClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailSalesClass_ClientProfileId",
                table: "DetailSalesClass",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailSalesClass_EnglishName",
                table: "DetailSalesClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailSalesClass_HeaderSalesClassId",
                table: "DetailSalesClass",
                column: "HeaderSalesClassId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailSalesClass_ReferenceCode",
                table: "DetailSalesClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderPurchaseClass_ArabicName",
                table: "HeaderPurchaseClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderPurchaseClass_ClientProfileId",
                table: "HeaderPurchaseClass",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderPurchaseClass_EnglishName",
                table: "HeaderPurchaseClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderPurchaseClass_ReferenceCode",
                table: "HeaderPurchaseClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderPurchaseClass_RootPurchaseClassId",
                table: "HeaderPurchaseClass",
                column: "RootPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderSalesClass_ArabicName",
                table: "HeaderSalesClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderSalesClass_ClientProfileId",
                table: "HeaderSalesClass",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderSalesClass_EnglishName",
                table: "HeaderSalesClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderSalesClass_ReferenceCode",
                table: "HeaderSalesClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderSalesClass_RootSalesClassId",
                table: "HeaderSalesClass",
                column: "RootSalesClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MainPurchaseClass_ArabicName",
                table: "MainPurchaseClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainPurchaseClass_ClientProfileId",
                table: "MainPurchaseClass",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MainPurchaseClass_DetailPurchaseClassId",
                table: "MainPurchaseClass",
                column: "DetailPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MainPurchaseClass_EnglishName",
                table: "MainPurchaseClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainPurchaseClass_ReferenceCode",
                table: "MainPurchaseClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_ArabicName",
                table: "MainSalesClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_ClientProfileId",
                table: "MainSalesClass",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_DetailSalesClassId",
                table: "MainSalesClass",
                column: "DetailSalesClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_EnglishName",
                table: "MainSalesClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_ReferenceCode",
                table: "MainSalesClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootPurchaseClass_ArabicName",
                table: "RootPurchaseClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootPurchaseClass_EnglishName",
                table: "RootPurchaseClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootPurchaseClass_ReferenceCode",
                table: "RootPurchaseClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootSalesClass_ArabicName",
                table: "RootSalesClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootSalesClass_EnglishName",
                table: "RootSalesClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootSalesClass_ReferenceCode",
                table: "RootSalesClass",
                column: "ReferenceCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MainPurchaseClass");

            migrationBuilder.DropTable(
                name: "MainSalesClass");

            migrationBuilder.DropTable(
                name: "DetailPurchaseClass");

            migrationBuilder.DropTable(
                name: "DetailSalesClass");

            migrationBuilder.DropTable(
                name: "HeaderPurchaseClass");

            migrationBuilder.DropTable(
                name: "HeaderSalesClass");

            migrationBuilder.DropTable(
                name: "RootPurchaseClass");

            migrationBuilder.DropTable(
                name: "RootSalesClass");

            migrationBuilder.CreateTable(
                name: "RootClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootClass", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HeaderClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RootClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeaderClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeaderClass_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HeaderClass_RootClass_RootClassId",
                        column: x => x.RootClassId,
                        principalTable: "RootClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetailClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HeaderClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailClass_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetailClass_HeaderClass_HeaderClassId",
                        column: x => x.HeaderClassId,
                        principalTable: "HeaderClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MainClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DetailClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReferenceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainClass_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MainClass_DetailClass_DetailClassId",
                        column: x => x.DetailClassId,
                        principalTable: "DetailClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetailClass_ArabicName",
                table: "DetailClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailClass_ClientProfileId",
                table: "DetailClass",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailClass_EnglishName",
                table: "DetailClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailClass_HeaderClassId",
                table: "DetailClass",
                column: "HeaderClassId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailClass_ReferenceCode",
                table: "DetailClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderClass_ArabicName",
                table: "HeaderClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderClass_ClientProfileId",
                table: "HeaderClass",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderClass_EnglishName",
                table: "HeaderClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderClass_ReferenceCode",
                table: "HeaderClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderClass_RootClassId",
                table: "HeaderClass",
                column: "RootClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MainClass_ArabicName",
                table: "MainClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainClass_ClientProfileId",
                table: "MainClass",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MainClass_DetailClassId",
                table: "MainClass",
                column: "DetailClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MainClass_EnglishName",
                table: "MainClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainClass_ReferenceCode",
                table: "MainClass",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootClass_ArabicName",
                table: "RootClass",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootClass_EnglishName",
                table: "RootClass",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootClass_ReferenceCode",
                table: "RootClass",
                column: "ReferenceCode",
                unique: true);
        }
    }
}
