using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyAndPosEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashierRefund",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefundNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CashierName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReasonText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RefundMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemsCount = table.Column<int>(type: "int", nullable: false),
                    RefundSubtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundTaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundTotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefundDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierRefund", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KdsOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TableNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreparingAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadyAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PriorityLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KdsOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyAlert",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Owner = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyAlert", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyBatch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyBatch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyControlledDrugRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Schedule = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Prescriber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Pharmacist = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PermitNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DispenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyControlledDrugRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyDrugInteraction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RxNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrimaryDrug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InteractingDrug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RiskDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Recommendation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Pharmacist = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyDrugInteraction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyInsuranceClaim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RxNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Insurer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyInsuranceClaim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyPatient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InsuranceClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActivePrescriptions = table.Column<int>(type: "int", nullable: false),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChronicDiseases = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastVisit = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyPatient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyPrescription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RxNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DoctorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InsuranceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ItemsCount = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyPrescription", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyReturn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Disposition = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyReturn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZReportSnapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashierName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OpenTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalTax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDiscounts = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalTransactions = table.Column<int>(type: "int", nullable: false),
                    TotalRefunds = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CardSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OpeningCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashIn = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashOut = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashDifference = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CategoryBreakdownJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TopProductsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZReportSnapshot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashierRefundItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierRefundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ItemBarcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReturnQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierRefundItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashierRefundItem_CashierRefund_CashierRefundId",
                        column: x => x.CashierRefundId,
                        principalTable: "CashierRefund",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KdsOrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KdsOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Modifiers = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KdsOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KdsOrderItem_KdsOrder_KdsOrderId",
                        column: x => x.KdsOrderId,
                        principalTable: "KdsOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreparationMinutes = table.Column<int>(type: "int", nullable: true),
                    ExpectedReadyAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantOrder_RestaurantTable_TableId",
                        column: x => x.TableId,
                        principalTable: "RestaurantTable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantOrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RestaurantOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Modifiers = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantOrderItem_RestaurantOrder_RestaurantOrderId",
                        column: x => x.RestaurantOrderId,
                        principalTable: "RestaurantOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashierRefund_InvoiceId",
                table: "CashierRefund",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierRefund_RefundDateUtc",
                table: "CashierRefund",
                column: "RefundDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CashierRefund_RefundNo",
                table: "CashierRefund",
                column: "RefundNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashierRefund_SessionId",
                table: "CashierRefund",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierRefundItem_CashierRefundId",
                table: "CashierRefundItem",
                column: "CashierRefundId");

            migrationBuilder.CreateIndex(
                name: "IX_KdsOrder_CreatedAt",
                table: "KdsOrder",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KdsOrder_OrderNumber",
                table: "KdsOrder",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_KdsOrder_Status",
                table: "KdsOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KdsOrder_StoreDetailsId",
                table: "KdsOrder",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_KdsOrderItem_KdsOrderId",
                table: "KdsOrderItem",
                column: "KdsOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAlert_Code",
                table: "PharmacyAlert",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAlert_Level",
                table: "PharmacyAlert",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAlert_State",
                table: "PharmacyAlert",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAlert_StoreDetailsId",
                table: "PharmacyAlert",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAlert_Type",
                table: "PharmacyAlert",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyBatch_BatchNo",
                table: "PharmacyBatch",
                column: "BatchNo");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyBatch_ExpiryDate",
                table: "PharmacyBatch",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyBatch_Status",
                table: "PharmacyBatch",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyBatch_StoreDetailsId",
                table: "PharmacyBatch",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyControlledDrugRecord_LogNo",
                table: "PharmacyControlledDrugRecord",
                column: "LogNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyControlledDrugRecord_Schedule",
                table: "PharmacyControlledDrugRecord",
                column: "Schedule");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyControlledDrugRecord_Status",
                table: "PharmacyControlledDrugRecord",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyControlledDrugRecord_StoreDetailsId",
                table: "PharmacyControlledDrugRecord",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDrugInteraction_Severity",
                table: "PharmacyDrugInteraction",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDrugInteraction_State",
                table: "PharmacyDrugInteraction",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDrugInteraction_StoreDetailsId",
                table: "PharmacyDrugInteraction",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceClaim_ClaimNo",
                table: "PharmacyInsuranceClaim",
                column: "ClaimNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceClaim_Status",
                table: "PharmacyInsuranceClaim",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceClaim_StoreDetailsId",
                table: "PharmacyInsuranceClaim",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPatient_FullName",
                table: "PharmacyPatient",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPatient_PatientCode",
                table: "PharmacyPatient",
                column: "PatientCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPatient_StoreDetailsId",
                table: "PharmacyPatient",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPrescription_CreatedAt",
                table: "PharmacyPrescription",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPrescription_RxNo",
                table: "PharmacyPrescription",
                column: "RxNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPrescription_Status",
                table: "PharmacyPrescription",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPrescription_StoreDetailsId",
                table: "PharmacyPrescription",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyReturn_ReturnNo",
                table: "PharmacyReturn",
                column: "ReturnNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyReturn_Status",
                table: "PharmacyReturn",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyReturn_StoreDetailsId",
                table: "PharmacyReturn",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrder_CreatedAt",
                table: "RestaurantOrder",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrder_OrderNumber",
                table: "RestaurantOrder",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrder_Status",
                table: "RestaurantOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrder_StoreDetailsId",
                table: "RestaurantOrder",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrder_TableId",
                table: "RestaurantOrder",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderItem_RestaurantOrderId",
                table: "RestaurantOrderItem",
                column: "RestaurantOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTable_StoreDetailsId",
                table: "RestaurantTable",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTable_StoreDetailsId_TableNumber",
                table: "RestaurantTable",
                columns: new[] { "StoreDetailsId", "TableNumber" },
                unique: true,
                filter: "[StoreDetailsId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ZReportSnapshot_ReportDate",
                table: "ZReportSnapshot",
                column: "ReportDate");

            migrationBuilder.CreateIndex(
                name: "IX_ZReportSnapshot_SessionId",
                table: "ZReportSnapshot",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ZReportSnapshot_StoreDetailsId",
                table: "ZReportSnapshot",
                column: "StoreDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashierRefundItem");

            migrationBuilder.DropTable(
                name: "KdsOrderItem");

            migrationBuilder.DropTable(
                name: "PharmacyAlert");

            migrationBuilder.DropTable(
                name: "PharmacyBatch");

            migrationBuilder.DropTable(
                name: "PharmacyControlledDrugRecord");

            migrationBuilder.DropTable(
                name: "PharmacyDrugInteraction");

            migrationBuilder.DropTable(
                name: "PharmacyInsuranceClaim");

            migrationBuilder.DropTable(
                name: "PharmacyPatient");

            migrationBuilder.DropTable(
                name: "PharmacyPrescription");

            migrationBuilder.DropTable(
                name: "PharmacyReturn");

            migrationBuilder.DropTable(
                name: "RestaurantOrderItem");

            migrationBuilder.DropTable(
                name: "ZReportSnapshot");

            migrationBuilder.DropTable(
                name: "CashierRefund");

            migrationBuilder.DropTable(
                name: "KdsOrder");

            migrationBuilder.DropTable(
                name: "RestaurantOrder");

            migrationBuilder.DropTable(
                name: "RestaurantTable");

        }
    }
}
