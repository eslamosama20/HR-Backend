using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchStatus",
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
                    table.PrimaryKey("PK_BatchStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BatchUnitStatuses",
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
                    table.PrimaryKey("PK_BatchUnitStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BranchStatus",
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
                    table.PrimaryKey("PK_BranchStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyStatus",
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
                    table.PrimaryKey("PK_CompanyStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoginActionLogs",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsFailedTrail = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginActionLogs", x => new { x.UserId, x.LoginTime });
                });

            migrationBuilder.CreateTable(
                name: "MeasurementUnit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ArabicSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EnglishSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2024, 6, 25, 22, 55, 33, 626, DateTimeKind.Local).AddTicks(1648))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementUnit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OTPStatuses",
                columns: table => new
                {
                    StatusID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPStatuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "OTPType",
                columns: table => new
                {
                    ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPType", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "passwordsLogs",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwordsLogs", x => new { x.UserId, x.CreatedAt });
                });

            migrationBuilder.CreateTable(
                name: "ProductUnitStatus",
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
                    table.PrimaryKey("PK_ProductUnitStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreStatus",
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
                    table.PrimaryKey("PK_StoreStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierStatus",
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
                    table.PrimaryKey("PK_SupplierStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 600, DateTimeKind.Utc).AddTicks(1412))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyDetails_CompanyStatus_CompanyStatusId",
                        column: x => x.CompanyStatusId,
                        principalTable: "CompanyStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OTPs",
                columns: table => new
                {
                    OTPReference = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OTPReferenceKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OTPPayload = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumberOfFailedTrails = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ExpiryDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    StatusID = table.Column<short>(type: "smallint", nullable: true),
                    OtpTypeID = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPs", x => x.OTPReference);
                    table.ForeignKey(
                        name: "FK_OTPs_OTPStatuses_StatusID",
                        column: x => x.StatusID,
                        principalTable: "OTPStatuses",
                        principalColumn: "StatusID");
                    table.ForeignKey(
                        name: "FK_OTPs_OTPType_OtpTypeID",
                        column: x => x.OtpTypeID,
                        principalTable: "OTPType",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "StoreDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 658, DateTimeKind.Utc).AddTicks(9644))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreDetails_StoreStatus_StoreStatusId",
                        column: x => x.StoreStatusId,
                        principalTable: "StoreStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 664, DateTimeKind.Utc).AddTicks(1004))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDetails_SupplierStatus_SupplierStatusId",
                        column: x => x.SupplierStatusId,
                        principalTable: "SupplierStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 596, DateTimeKind.Utc).AddTicks(5199))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchDetails_BranchStatus_BranchStatusId",
                        column: x => x.BranchStatusId,
                        principalTable: "BranchStatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BranchDetails_CompanyDetails_CompanyDetailsId",
                        column: x => x.CompanyDetailsId,
                        principalTable: "CompanyDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArabicFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArabicMiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArabicLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishMiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_CompanyDetails_CompanyDetailsId",
                        column: x => x.CompanyDetailsId,
                        principalTable: "CompanyDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductBatchDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatchStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBatchDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBatchDetails_BatchStatus_BatchStatusId",
                        column: x => x.BatchStatusId,
                        principalTable: "BatchStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBatchDetails_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasurementUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 646, DateTimeKind.Utc).AddTicks(4928))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDetails_MeasurementUnit_MeasurementUnitId",
                        column: x => x.MeasurementUnitId,
                        principalTable: "MeasurementUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductDetails_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductUnit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductUnitStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 651, DateTimeKind.Utc).AddTicks(8542))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductUnit_ProductDetails_ProductDetailsId",
                        column: x => x.ProductDetailsId,
                        principalTable: "ProductDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductUnit_ProductUnitStatus_ProductUnitStatusId",
                        column: x => x.ProductUnitStatusId,
                        principalTable: "ProductUnitStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductBatchToUnitAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductBatchDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchUnitStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2024, 6, 25, 19, 55, 33, 630, DateTimeKind.Utc).AddTicks(6628))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBatchToUnitAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBatchToUnitAssignments_BatchUnitStatuses_BatchUnitStatusId",
                        column: x => x.BatchUnitStatusId,
                        principalTable: "BatchUnitStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBatchToUnitAssignments_ProductBatchDetails_ProductBatchDetailsId",
                        column: x => x.ProductBatchDetailsId,
                        principalTable: "ProductBatchDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBatchToUnitAssignments_ProductUnit_ProductUnitId",
                        column: x => x.ProductUnitId,
                        principalTable: "ProductUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchDetails_BranchStatusId",
                table: "BranchDetails",
                column: "BranchStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchDetails_CompanyDetailsId",
                table: "BranchDetails",
                column: "CompanyDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_CompanyStatusId",
                table: "CompanyDetails",
                column: "CompanyStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OTPs_OtpTypeID",
                table: "OTPs",
                column: "OtpTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_OTPs_StatusID",
                table: "OTPs",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatchDetails_BatchStatusId",
                table: "ProductBatchDetails",
                column: "BatchStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatchDetails_StoreDetailsId",
                table: "ProductBatchDetails",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatchToUnitAssignments_BatchUnitStatusId",
                table: "ProductBatchToUnitAssignments",
                column: "BatchUnitStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatchToUnitAssignments_ProductBatchDetailsId",
                table: "ProductBatchToUnitAssignments",
                column: "ProductBatchDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatchToUnitAssignments_ProductUnitId",
                table: "ProductBatchToUnitAssignments",
                column: "ProductUnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductDetails_MeasurementUnitId",
                table: "ProductDetails",
                column: "MeasurementUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDetails_StoreDetailsId",
                table: "ProductDetails",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_ProductDetailsId",
                table: "ProductUnit",
                column: "ProductDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_ProductUnitStatusId",
                table: "ProductUnit",
                column: "ProductUnitStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreDetails_StoreStatusId",
                table: "StoreDetails",
                column: "StoreStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDetails_SupplierStatusId",
                table: "SupplierDetails",
                column: "SupplierStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyDetailsId",
                table: "Users",
                column: "CompanyDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchDetails");

            migrationBuilder.DropTable(
                name: "LoginActionLogs");

            migrationBuilder.DropTable(
                name: "OTPs");

            migrationBuilder.DropTable(
                name: "passwordsLogs");

            migrationBuilder.DropTable(
                name: "ProductBatchToUnitAssignments");

            migrationBuilder.DropTable(
                name: "SupplierDetails");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "BranchStatus");

            migrationBuilder.DropTable(
                name: "OTPStatuses");

            migrationBuilder.DropTable(
                name: "OTPType");

            migrationBuilder.DropTable(
                name: "BatchUnitStatuses");

            migrationBuilder.DropTable(
                name: "ProductBatchDetails");

            migrationBuilder.DropTable(
                name: "ProductUnit");

            migrationBuilder.DropTable(
                name: "SupplierStatus");

            migrationBuilder.DropTable(
                name: "CompanyDetails");

            migrationBuilder.DropTable(
                name: "BatchStatus");

            migrationBuilder.DropTable(
                name: "ProductDetails");

            migrationBuilder.DropTable(
                name: "ProductUnitStatus");

            migrationBuilder.DropTable(
                name: "CompanyStatus");

            migrationBuilder.DropTable(
                name: "MeasurementUnit");

            migrationBuilder.DropTable(
                name: "StoreDetails");

            migrationBuilder.DropTable(
                name: "StoreStatus");
        }
    }
}
