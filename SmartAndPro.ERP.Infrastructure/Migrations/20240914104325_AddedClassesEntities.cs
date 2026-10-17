using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedClassesEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RootClass",
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
                    table.PrimaryKey("PK_RootClass", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HeaderClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RootClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HeaderClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DetailClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MainClass");

            migrationBuilder.DropTable(
                name: "DetailClass");

            migrationBuilder.DropTable(
                name: "HeaderClass");

            migrationBuilder.DropTable(
                name: "RootClass");
        }
    }
}
