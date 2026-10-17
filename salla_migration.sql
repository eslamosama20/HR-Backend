BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [CashierRefund] (
        [Id] uniqueidentifier NOT NULL,
        [RefundNo] nvarchar(50) NOT NULL,
        [InvoiceId] int NOT NULL,
        [InvoiceNo] nvarchar(50) NOT NULL,
        [SessionId] int NOT NULL,
        [CustomerName] nvarchar(200) NOT NULL,
        [CashierName] nvarchar(120) NOT NULL,
        [Reason] nvarchar(50) NOT NULL,
        [ReasonText] nvarchar(200) NOT NULL,
        [RefundMethod] nvarchar(50) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ItemsCount] int NOT NULL,
        [RefundSubtotal] decimal(18,2) NOT NULL,
        [RefundTaxAmount] decimal(18,2) NOT NULL,
        [RefundTotalAmount] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [RefundDateUtc] datetime2 NOT NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CashierRefund] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [KdsOrder] (
        [Id] uniqueidentifier NOT NULL,
        [OrderNumber] nvarchar(50) NOT NULL,
        [TableNo] nvarchar(20) NULL,
        [Source] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [PreparingAt] datetime2 NULL,
        [ReadyAt] datetime2 NULL,
        [PriorityLevel] nvarchar(20) NOT NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_KdsOrder] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [PharmacyAlert] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [Level] nvarchar(20) NOT NULL,
        [State] nvarchar(20) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [Branch] nvarchar(100) NULL,
        [Owner] nvarchar(120) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyAlert] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [PharmacyBatch] (
        [Id] uniqueidentifier NOT NULL,
        [ItemCode] nvarchar(100) NOT NULL,
        [ItemName] nvarchar(200) NOT NULL,
        [BatchNo] nvarchar(50) NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [MinQuantity] decimal(18,2) NOT NULL,
        [StoreName] nvarchar(100) NULL,
        [Status] nvarchar(20) NOT NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyBatch] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [PharmacyControlledDrugRecord] (
        [Id] uniqueidentifier NOT NULL,
        [LogNo] nvarchar(50) NOT NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [DrugName] nvarchar(200) NOT NULL,
        [Schedule] nvarchar(10) NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Prescriber] nvarchar(200) NOT NULL,
        [Pharmacist] nvarchar(120) NOT NULL,
        [PermitNo] nvarchar(50) NULL,
        [Status] nvarchar(20) NOT NULL,
        [Reason] nvarchar(300) NULL,
        [Notes] nvarchar(500) NULL,
        [DispenseDate] datetime2 NOT NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyControlledDrugRecord] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [PharmacyDrugInteraction] (
        [Id] uniqueidentifier NOT NULL,
        [RxNo] nvarchar(50) NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [PrimaryDrug] nvarchar(200) NOT NULL,
        [InteractingDrug] nvarchar(200) NOT NULL,
        [Severity] nvarchar(20) NOT NULL,
        [State] nvarchar(30) NOT NULL,
        [RiskDescription] nvarchar(500) NULL,
        [Recommendation] nvarchar(500) NULL,
        [Pharmacist] nvarchar(120) NOT NULL,
        [CheckedAt] datetime2 NOT NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyDrugInteraction] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [PharmacyInsuranceClaim] (
        [Id] uniqueidentifier NOT NULL,
        [ClaimNo] nvarchar(50) NOT NULL,
        [RxNo] nvarchar(50) NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [Insurer] nvarchar(200) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [ApprovedAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [ReferenceNo] nvarchar(100) NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [Notes] nvarchar(500) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyInsuranceClaim] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [PharmacyPatient] (
        [Id] uniqueidentifier NOT NULL,
        [PatientCode] nvarchar(50) NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [Age] int NOT NULL,
        [Gender] nvarchar(10) NOT NULL,
        [Phone] nvarchar(20) NULL,
        [InsuranceClass] nvarchar(100) NULL,
        [ActivePrescriptions] int NOT NULL,
        [Allergies] nvarchar(max) NULL,
        [ChronicDiseases] nvarchar(max) NULL,
        [LastVisit] datetime2 NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyPatient] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [PharmacyPrescription] (
        [Id] uniqueidentifier NOT NULL,
        [RxNo] nvarchar(50) NOT NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [DoctorName] nvarchar(200) NOT NULL,
        [InsuranceName] nvarchar(200) NULL,
        [ItemsCount] int NOT NULL,
        [Priority] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyPrescription] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [PharmacyReturn] (
        [Id] uniqueidentifier NOT NULL,
        [ReturnNo] nvarchar(50) NOT NULL,
        [Source] nvarchar(20) NOT NULL,
        [ItemCode] nvarchar(100) NOT NULL,
        [ItemName] nvarchar(200) NOT NULL,
        [BatchNo] nvarchar(50) NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Reason] nvarchar(300) NOT NULL,
        [Disposition] nvarchar(30) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [RequestedBy] nvarchar(120) NOT NULL,
        [ReturnDate] datetime2 NOT NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyReturn] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [RestaurantTable] (
        [Id] uniqueidentifier NOT NULL,
        [TableNumber] nvarchar(20) NOT NULL,
        [Area] nvarchar(50) NOT NULL,
        [Capacity] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [SortOrder] int NOT NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_RestaurantTable] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [ZReportSnapshot] (
        [Id] uniqueidentifier NOT NULL,
        [SessionId] int NOT NULL,
        [ReportDate] datetime2 NOT NULL,
        [CashierName] nvarchar(120) NOT NULL,
        [BranchName] nvarchar(200) NOT NULL,
        [OpenTime] datetime2 NOT NULL,
        [CloseTime] datetime2 NULL,
        [OpeningBalance] decimal(18,2) NOT NULL,
        [TotalSales] decimal(18,2) NOT NULL,
        [TotalTax] decimal(18,2) NOT NULL,
        [TotalDiscounts] decimal(18,2) NOT NULL,
        [NetSales] decimal(18,2) NOT NULL,
        [TotalTransactions] int NOT NULL,
        [TotalRefunds] decimal(18,2) NOT NULL,
        [CashSales] decimal(18,2) NOT NULL,
        [CardSales] decimal(18,2) NOT NULL,
        [CreditSales] decimal(18,2) NOT NULL,
        [OpeningCash] decimal(18,2) NOT NULL,
        [CashIn] decimal(18,2) NOT NULL,
        [CashOut] decimal(18,2) NOT NULL,
        [ExpectedCash] decimal(18,2) NOT NULL,
        [ActualCash] decimal(18,2) NOT NULL,
        [CashDifference] decimal(18,2) NOT NULL,
        [CategoryBreakdownJson] nvarchar(max) NULL,
        [TopProductsJson] nvarchar(max) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZReportSnapshot] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [CashierRefundItem] (
        [Id] uniqueidentifier NOT NULL,
        [CashierRefundId] uniqueidentifier NOT NULL,
        [SaleItemId] int NOT NULL,
        [ItemCode] nvarchar(100) NOT NULL,
        [ItemName] nvarchar(200) NOT NULL,
        [ItemBarcode] nvarchar(100) NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [ReturnQty] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxPercentage] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [RefundAmount] decimal(18,2) NOT NULL,
        [UnitName] nvarchar(50) NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CashierRefundItem] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CashierRefundItem_CashierRefund_CashierRefundId] FOREIGN KEY ([CashierRefundId]) REFERENCES [CashierRefund] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [KdsOrderItem] (
        [Id] uniqueidentifier NOT NULL,
        [KdsOrderId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Qty] int NOT NULL,
        [Notes] nvarchar(300) NULL,
        [Modifiers] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_KdsOrderItem] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KdsOrderItem_KdsOrder_KdsOrderId] FOREIGN KEY ([KdsOrderId]) REFERENCES [KdsOrder] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [RestaurantOrder] (
        [Id] uniqueidentifier NOT NULL,
        [OrderNumber] nvarchar(50) NOT NULL,
        [TableId] uniqueidentifier NULL,
        [Source] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [Tax] decimal(18,2) NOT NULL,
        [Discount] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [PreparationMinutes] int NULL,
        [ExpectedReadyAt] datetime2 NULL,
        [SessionId] int NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_RestaurantOrder] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RestaurantOrder_RestaurantTable_TableId] FOREIGN KEY ([TableId]) REFERENCES [RestaurantTable] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE TABLE [RestaurantOrderItem] (
        [Id] uniqueidentifier NOT NULL,
        [RestaurantOrderId] uniqueidentifier NOT NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [Qty] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Notes] nvarchar(300) NULL,
        [Modifiers] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_RestaurantOrderItem] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RestaurantOrderItem_RestaurantOrder_RestaurantOrderId] FOREIGN KEY ([RestaurantOrderId]) REFERENCES [RestaurantOrder] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_CashierRefund_InvoiceId] ON [CashierRefund] ([InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_CashierRefund_RefundDateUtc] ON [CashierRefund] ([RefundDateUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CashierRefund_RefundNo] ON [CashierRefund] ([RefundNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_CashierRefund_SessionId] ON [CashierRefund] ([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_CashierRefundItem_CashierRefundId] ON [CashierRefundItem] ([CashierRefundId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_KdsOrder_CreatedAt] ON [KdsOrder] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_KdsOrder_OrderNumber] ON [KdsOrder] ([OrderNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_KdsOrder_Status] ON [KdsOrder] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_KdsOrder_StoreDetailsId] ON [KdsOrder] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_KdsOrderItem_KdsOrderId] ON [KdsOrderItem] ([KdsOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyAlert_Code] ON [PharmacyAlert] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyAlert_Level] ON [PharmacyAlert] ([Level]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyAlert_State] ON [PharmacyAlert] ([State]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyAlert_StoreDetailsId] ON [PharmacyAlert] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyAlert_Type] ON [PharmacyAlert] ([Type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyBatch_BatchNo] ON [PharmacyBatch] ([BatchNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyBatch_ExpiryDate] ON [PharmacyBatch] ([ExpiryDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyBatch_Status] ON [PharmacyBatch] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyBatch_StoreDetailsId] ON [PharmacyBatch] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyControlledDrugRecord_LogNo] ON [PharmacyControlledDrugRecord] ([LogNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyControlledDrugRecord_Schedule] ON [PharmacyControlledDrugRecord] ([Schedule]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyControlledDrugRecord_Status] ON [PharmacyControlledDrugRecord] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyControlledDrugRecord_StoreDetailsId] ON [PharmacyControlledDrugRecord] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyDrugInteraction_Severity] ON [PharmacyDrugInteraction] ([Severity]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyDrugInteraction_State] ON [PharmacyDrugInteraction] ([State]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyDrugInteraction_StoreDetailsId] ON [PharmacyDrugInteraction] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyInsuranceClaim_ClaimNo] ON [PharmacyInsuranceClaim] ([ClaimNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyInsuranceClaim_Status] ON [PharmacyInsuranceClaim] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyInsuranceClaim_StoreDetailsId] ON [PharmacyInsuranceClaim] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyPatient_FullName] ON [PharmacyPatient] ([FullName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyPatient_PatientCode] ON [PharmacyPatient] ([PatientCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyPatient_StoreDetailsId] ON [PharmacyPatient] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescription_CreatedAt] ON [PharmacyPrescription] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyPrescription_RxNo] ON [PharmacyPrescription] ([RxNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescription_Status] ON [PharmacyPrescription] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescription_StoreDetailsId] ON [PharmacyPrescription] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyReturn_ReturnNo] ON [PharmacyReturn] ([ReturnNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyReturn_Status] ON [PharmacyReturn] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyReturn_StoreDetailsId] ON [PharmacyReturn] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrder_CreatedAt] ON [RestaurantOrder] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrder_OrderNumber] ON [RestaurantOrder] ([OrderNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrder_Status] ON [RestaurantOrder] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrder_StoreDetailsId] ON [RestaurantOrder] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrder_TableId] ON [RestaurantOrder] ([TableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrderItem_RestaurantOrderId] ON [RestaurantOrderItem] ([RestaurantOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_RestaurantTable_StoreDetailsId] ON [RestaurantTable] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RestaurantTable_StoreDetailsId_TableNumber] ON [RestaurantTable] ([StoreDetailsId], [TableNumber]) WHERE [StoreDetailsId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_ZReportSnapshot_ReportDate] ON [ZReportSnapshot] ([ReportDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_ZReportSnapshot_SessionId] ON [ZReportSnapshot] ([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    CREATE INDEX [IX_ZReportSnapshot_StoreDetailsId] ON [ZReportSnapshot] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260409112830_AddPharmacyAndPosEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260409112830_AddPharmacyAndPosEntities', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    ALTER TABLE [PurchaseItems] ADD [BulkPrice] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    ALTER TABLE [PurchaseItems] ADD [RetailPrice] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    ALTER TABLE [PurchaseItems] ADD [WholesalePrice] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    ALTER TABLE [PermitEntry] ADD [BulkPrice] money NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    ALTER TABLE [PermitEntry] ADD [RetailPrice] money NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    ALTER TABLE [PermitEntry] ADD [WholesalePrice] money NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [ImageUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    CREATE TABLE [CashierRefund] (
        [Id] uniqueidentifier NOT NULL,
        [RefundNo] nvarchar(50) NOT NULL,
        [InvoiceId] int NOT NULL,
        [InvoiceNo] nvarchar(50) NOT NULL,
        [SessionId] int NOT NULL,
        [CustomerName] nvarchar(200) NOT NULL,
        [CashierName] nvarchar(120) NOT NULL,
        [Reason] nvarchar(50) NOT NULL,
        [ReasonText] nvarchar(200) NOT NULL,
        [RefundMethod] nvarchar(50) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ItemsCount] int NOT NULL,
        [RefundSubtotal] decimal(18,2) NOT NULL,
        [RefundTaxAmount] decimal(18,2) NOT NULL,
        [RefundTotalAmount] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [RefundDateUtc] datetime2 NOT NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CashierRefund] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    CREATE TABLE [CashierRefundItem] (
        [Id] uniqueidentifier NOT NULL,
        [CashierRefundId] uniqueidentifier NOT NULL,
        [SaleItemId] int NOT NULL,
        [ItemCode] nvarchar(100) NOT NULL,
        [ItemName] nvarchar(200) NOT NULL,
        [ItemBarcode] nvarchar(100) NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [ReturnQty] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxPercentage] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [RefundAmount] decimal(18,2) NOT NULL,
        [UnitName] nvarchar(50) NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CashierRefundItem] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CashierRefundItem_CashierRefund_CashierRefundId] FOREIGN KEY ([CashierRefundId]) REFERENCES [CashierRefund] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    CREATE INDEX [IX_CashierRefund_InvoiceId] ON [CashierRefund] ([InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    CREATE INDEX [IX_CashierRefund_RefundDateUtc] ON [CashierRefund] ([RefundDateUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CashierRefund_RefundNo] ON [CashierRefund] ([RefundNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    CREATE INDEX [IX_CashierRefund_SessionId] ON [CashierRefund] ([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    CREATE INDEX [IX_CashierRefundItem_CashierRefundId] ON [CashierRefundItem] ([CashierRefundId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411200056_Add-Cashir-Purchases'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260411200056_Add-Cashir-Purchases', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [KdsOrder];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [KdsOrderItem];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [PharmacyAlert];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [PharmacyBatch];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [PharmacyControlledDrugRecord];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [PharmacyDrugInteraction];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [PharmacyInsuranceClaim];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [PharmacyPatient];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [PharmacyPrescription];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [PharmacyReturn];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [RestaurantOrder];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [RestaurantOrderItem];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [RestaurantTable];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    DROP TABLE [ZReportSnapshot];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE TABLE [SallaOrders] (
        [Id] uniqueidentifier NOT NULL,
        [SallaOrderId] bigint NOT NULL,
        [ReferenceNumber] nvarchar(50) NOT NULL,
        [Status] int NOT NULL,
        [PaymentStatus] int NOT NULL,
        [FulfillmentStatus] int NOT NULL,
        [SubTotal] decimal(18,2) NOT NULL,
        [ShippingAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(10) NOT NULL,
        [CustomerName] nvarchar(200) NOT NULL,
        [CustomerEmail] nvarchar(200) NULL,
        [CustomerPhone] nvarchar(30) NULL,
        [SallaCustomerId] bigint NULL,
        [ShippingCity] nvarchar(100) NULL,
        [ShippingDistrict] nvarchar(200) NULL,
        [ShippingStreet] nvarchar(300) NULL,
        [ShippingPostalCode] nvarchar(20) NULL,
        [ShippingLat] float NULL,
        [ShippingLng] float NULL,
        [PaymentMethod] nvarchar(50) NULL,
        [SallaPaymentId] nvarchar(100) NULL,
        [SaleInvoiceId] int NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [SyncStatus] int NOT NULL,
        [SyncError] nvarchar(1000) NULL,
        [SyncedAt] datetime2 NULL,
        [SallaCreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SallaOrders] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE TABLE [SallaProductMappings] (
        [Id] uniqueidentifier NOT NULL,
        [SallaProductId] bigint NOT NULL,
        [SallaSku] nvarchar(100) NULL,
        [SallaProductName] nvarchar(300) NOT NULL,
        [SallaPrice] decimal(18,2) NULL,
        [ProductUnitId] uniqueidentifier NULL,
        [ProductUnitName] nvarchar(300) NULL,
        [LastSyncedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SallaProductMappings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE TABLE [SallaSyncLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EventType] nvarchar(100) NOT NULL,
        [Direction] int NOT NULL,
        [Status] int NOT NULL,
        [RequestPayload] nvarchar(max) NULL,
        [ResponsePayload] nvarchar(max) NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [RelatedEntityId] uniqueidentifier NULL,
        [SallaResourceId] bigint NULL,
        [DurationMs] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SallaSyncLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE TABLE [SallaOrderItems] (
        [Id] uniqueidentifier NOT NULL,
        [SallaOrderId] uniqueidentifier NOT NULL,
        [SallaProductId] bigint NOT NULL,
        [SallaSku] nvarchar(100) NULL,
        [ProductName] nvarchar(300) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [OptionsSummary] nvarchar(500) NULL,
        [ProductUnitId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SallaOrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SallaOrderItems_SallaOrders_SallaOrderId] FOREIGN KEY ([SallaOrderId]) REFERENCES [SallaOrders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaOrderItems_ProductUnitId] ON [SallaOrderItems] ([ProductUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaOrderItems_SallaOrderId] ON [SallaOrderItems] ([SallaOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaOrderItems_SallaProductId] ON [SallaOrderItems] ([SallaProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaOrders_ReferenceNumber] ON [SallaOrders] ([ReferenceNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaOrders_SaleInvoiceId] ON [SallaOrders] ([SaleInvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaOrders_SallaCreatedAt] ON [SallaOrders] ([SallaCreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SallaOrders_SallaOrderId] ON [SallaOrders] ([SallaOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaOrders_Status] ON [SallaOrders] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaOrders_SyncStatus] ON [SallaOrders] ([SyncStatus]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaProductMappings_ProductUnitId] ON [SallaProductMappings] ([ProductUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SallaProductMappings_SallaProductId] ON [SallaProductMappings] ([SallaProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaSyncLogs_CreatedAt] ON [SallaSyncLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaSyncLogs_EventType] ON [SallaSyncLogs] ([EventType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaSyncLogs_SallaResourceId] ON [SallaSyncLogs] ([SallaResourceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    CREATE INDEX [IX_SallaSyncLogs_Status] ON [SallaSyncLogs] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    ALTER TABLE [CashierRefundItem] ADD CONSTRAINT [FK_CashierRefundItem_CashierRefund_CashierRefundId] FOREIGN KEY ([CashierRefundId]) REFERENCES [CashierRefund] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412230842_AddSallaIntegration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260412230842_AddSallaIntegration', N'8.0.3');
END;
GO

COMMIT;
GO

