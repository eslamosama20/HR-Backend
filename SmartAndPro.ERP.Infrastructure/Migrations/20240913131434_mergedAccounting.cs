using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mergedAccounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RootAccount",
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
                    table.PrimaryKey("PK_RootAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HeaderAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RootAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeaderAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeaderAccount_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HeaderAccount_RootAccount_RootAccountId",
                        column: x => x.RootAccountId,
                        principalTable: "RootAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VendorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transaction_CustomerProfile_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Transaction_TransactionType_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transaction_VendorProfile_VendorProfileId",
                        column: x => x.VendorProfileId,
                        principalTable: "VendorProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DetailAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HeaderAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailAccount_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetailAccount_HeaderAccount_HeaderAccountId",
                        column: x => x.HeaderAccountId,
                        principalTable: "HeaderAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MainAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DetailAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainAccount_ClientProfile_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MainAccount_DetailAccount_DetailAccountId",
                        column: x => x.DetailAccountId,
                        principalTable: "DetailAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransactionEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "money", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "money", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionEntry_MainAccount_MainAccountId",
                        column: x => x.MainAccountId,
                        principalTable: "MainAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransactionEntry_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProfile_ArabicName",
                table: "ClientProfile",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientProfile_EnglishName",
                table: "ClientProfile",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfile_ArabicName",
                table: "CustomerProfile",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfile_EnglishName",
                table: "CustomerProfile",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailAccount_ArabicName",
                table: "DetailAccount",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailAccount_ClientProfileId",
                table: "DetailAccount",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailAccount_EnglishName",
                table: "DetailAccount",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailAccount_HeaderAccountId",
                table: "DetailAccount",
                column: "HeaderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailAccount_ReferenceCode",
                table: "DetailAccount",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderAccount_ArabicName",
                table: "HeaderAccount",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderAccount_ClientProfileId",
                table: "HeaderAccount",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderAccount_EnglishName",
                table: "HeaderAccount",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderAccount_ReferenceCode",
                table: "HeaderAccount",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderAccount_RootAccountId",
                table: "HeaderAccount",
                column: "RootAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MainAccount_ArabicName",
                table: "MainAccount",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainAccount_ClientProfileId",
                table: "MainAccount",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MainAccount_DetailAccountId",
                table: "MainAccount",
                column: "DetailAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MainAccount_EnglishName",
                table: "MainAccount",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MainAccount_ReferenceCode",
                table: "MainAccount",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootAccount_ArabicName",
                table: "RootAccount",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootAccount_EnglishName",
                table: "RootAccount",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootAccount_ReferenceCode",
                table: "RootAccount",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ClientProfileId",
                table: "Transaction",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CustomerProfileId",
                table: "Transaction",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ReferenceNumber",
                table: "Transaction",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionTypeId",
                table: "Transaction",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_VendorProfileId",
                table: "Transaction",
                column: "VendorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionEntry_MainAccountId",
                table: "TransactionEntry",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionEntry_TransactionId",
                table: "TransactionEntry",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionType_ArabicName",
                table: "TransactionType",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionType_EnglishName",
                table: "TransactionType",
                column: "EnglishName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfile_ArabicName",
                table: "VendorProfile",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfile_EnglishName",
                table: "VendorProfile",
                column: "EnglishName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionEntry");

            migrationBuilder.DropTable(
                name: "MainAccount");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "DetailAccount");

            migrationBuilder.DropTable(
                name: "CustomerProfile");

            migrationBuilder.DropTable(
                name: "TransactionType");

            migrationBuilder.DropTable(
                name: "VendorProfile");

            migrationBuilder.DropTable(
                name: "HeaderAccount");

            migrationBuilder.DropTable(
                name: "ClientProfile");

            migrationBuilder.DropTable(
                name: "RootAccount");
        }
    }
}
