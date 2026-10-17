BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
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
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
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
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
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
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE TABLE [PharmacyDoctor] (
        [Id] uniqueidentifier NOT NULL,
        [SeqNo] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Specialization] nvarchar(200) NULL,
        [LicenseNo] nvarchar(50) NULL,
        [Mobile] nvarchar(20) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyDoctor] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
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
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
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
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE TABLE [PharmacyInsuranceCompany] (
        [Id] uniqueidentifier NOT NULL,
        [SeqNo] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [DefaultCoveragePercentage] decimal(5,2) NOT NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyInsuranceCompany] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE TABLE [PharmacyPatient] (
        [Id] uniqueidentifier NOT NULL,
        [SeqNo] int NOT NULL IDENTITY,
        [PatientCode] nvarchar(50) NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [Age] int NOT NULL,
        [Gender] nvarchar(10) NOT NULL,
        [Dob] datetime2 NULL,
        [NationalId] nvarchar(20) NULL,
        [Phone] nvarchar(20) NULL,
        [Mobile] nvarchar(20) NULL,
        [Address] nvarchar(500) NULL,
        [Email] nvarchar(200) NULL,
        [InsuranceClass] nvarchar(100) NULL,
        [HasInsurance] bit NOT NULL,
        [ActivePrescriptions] int NOT NULL,
        [Allergies] nvarchar(500) NULL,
        [ChronicDiseases] nvarchar(500) NULL,
        [LastVisit] datetime2 NULL,
        [Notes] nvarchar(500) NULL,
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
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
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
        [Status] nvarchar(20) NOT NULL,
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
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE TABLE [PharmacyPrescription] (
        [Id] uniqueidentifier NOT NULL,
        [SeqNo] int NOT NULL IDENTITY,
        [RxNo] nvarchar(50) NOT NULL,
        [IssueDate] datetime2 NOT NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [PatientId] uniqueidentifier NULL,
        [DoctorName] nvarchar(200) NULL,
        [DoctorId] uniqueidentifier NULL,
        [BranchId] nvarchar(50) NULL,
        [BranchName] nvarchar(200) NULL,
        [StoreId] nvarchar(50) NULL,
        [StoreName] nvarchar(200) NULL,
        [PaymentType] nvarchar(20) NOT NULL,
        [Priority] nvarchar(20) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [InsuranceEnabled] bit NOT NULL,
        [InsuranceCompanyId] uniqueidentifier NULL,
        [InsuranceName] nvarchar(200) NULL,
        [InsuranceMemberNo] nvarchar(100) NULL,
        [InsuranceApprovalNo] nvarchar(100) NULL,
        [InsuranceCoveragePercentage] decimal(5,2) NOT NULL,
        [InsurancePatientSharePercentage] decimal(5,2) NOT NULL,
        [InsuranceCoveredAmount] decimal(18,2) NOT NULL,
        [InsurancePatientPayableAmount] decimal(18,2) NOT NULL,
        [InsuranceStatus] nvarchar(30) NULL,
        [ItemsCount] int NOT NULL,
        [SubTotal] decimal(18,2) NOT NULL,
        [DiscountTotal] decimal(18,2) NOT NULL,
        [TaxTotal] decimal(18,2) NOT NULL,
        [GrandTotal] decimal(18,2) NOT NULL,
        [PatientPayable] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CancelReason] nvarchar(500) NULL,
        [PosTerminalId] nvarchar(50) NULL,
        [SalesInvoiceId] int NULL,
        [PosQueueNo] nvarchar(50) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyPrescription] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PharmacyPrescription_PharmacyDoctor_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [PharmacyDoctor] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PharmacyPrescription_PharmacyInsuranceCompany_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [PharmacyInsuranceCompany] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PharmacyPrescription_PharmacyPatient_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [PharmacyPatient] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE TABLE [PharmacyAttachment] (
        [Id] uniqueidentifier NOT NULL,
        [SeqNo] int NOT NULL IDENTITY,
        [PrescriptionId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(500) NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [Size] bigint NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [Notes] nvarchar(500) NULL,
        [FilePath] nvarchar(1000) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyAttachment] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PharmacyAttachment_PharmacyPrescription_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [PharmacyPrescription] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE TABLE [PharmacyPrescriptionItem] (
        [Id] uniqueidentifier NOT NULL,
        [SeqNo] int NOT NULL IDENTITY,
        [PrescriptionId] uniqueidentifier NOT NULL,
        [PrescriptionRxNo] nvarchar(50) NULL,
        [LineNo] int NOT NULL,
        [ItemId] int NOT NULL,
        [ItemCode] nvarchar(100) NOT NULL,
        [ItemName] nvarchar(200) NOT NULL,
        [RequestedQty] decimal(18,2) NOT NULL,
        [DispensedQty] decimal(18,2) NOT NULL,
        [StoreId] nvarchar(50) NULL,
        [BatchId] nvarchar(50) NULL,
        [BatchNo] nvarchar(50) NULL,
        [ExpiryDate] datetime2 NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [AllowSubstitute] bit NOT NULL,
        [IsControlled] bit NOT NULL,
        [AvailabilityStatus] nvarchar(30) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyPrescriptionItem] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PharmacyPrescriptionItem_PharmacyPrescription_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [PharmacyPrescription] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyAttachment_PrescriptionId] ON [PharmacyAttachment] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyAttachment_SeqNo] ON [PharmacyAttachment] ([SeqNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyDoctor_LicenseNo] ON [PharmacyDoctor] ([LicenseNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyDoctor_Name] ON [PharmacyDoctor] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyDoctor_SeqNo] ON [PharmacyDoctor] ([SeqNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyInsuranceCompany_Name] ON [PharmacyInsuranceCompany] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyInsuranceCompany_SeqNo] ON [PharmacyInsuranceCompany] ([SeqNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPatient_Mobile] ON [PharmacyPatient] ([Mobile]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPatient_NationalId] ON [PharmacyPatient] ([NationalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPatient_PatientCode] ON [PharmacyPatient] ([PatientCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyPatient_SeqNo] ON [PharmacyPatient] ([SeqNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescription_DoctorId] ON [PharmacyPrescription] ([DoctorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescription_InsuranceCompanyId] ON [PharmacyPrescription] ([InsuranceCompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescription_PatientId] ON [PharmacyPrescription] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescription_RxNo] ON [PharmacyPrescription] ([RxNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyPrescription_SeqNo] ON [PharmacyPrescription] ([SeqNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescription_Status] ON [PharmacyPrescription] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescriptionItem_PrescriptionId] ON [PharmacyPrescriptionItem] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyPrescriptionItem_SeqNo] ON [PharmacyPrescriptionItem] ([SeqNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416003447_Add-Pharmacy-Tables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260416003447_Add-Pharmacy-Tables', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE TABLE [WasafatyAuthTokens] (
        [Id] uniqueidentifier NOT NULL,
        [AccessToken] nvarchar(max) NOT NULL,
        [RefreshToken] nvarchar(max) NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WasafatyAuthTokens] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE TABLE [WasafatyPrescriptions] (
        [Id] uniqueidentifier NOT NULL,
        [FhirResourceId] nvarchar(200) NOT NULL,
        [PatientNationalId] nvarchar(20) NOT NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [DoctorName] nvarchar(200) NULL,
        [DoctorLicenseNo] nvarchar(50) NULL,
        [PrescriberFacilityName] nvarchar(300) NULL,
        [InsuranceName] nvarchar(200) NULL,
        [InsurancePolicyNo] nvarchar(100) NULL,
        [InsuranceCoveragePercent] decimal(5,2) NOT NULL,
        [ItemsCount] int NOT NULL,
        [Status] int NOT NULL,
        [ClaimStatus] int NOT NULL,
        [NphiesClaimId] nvarchar(100) NULL,
        [InternalPrescriptionId] uniqueidentifier NULL,
        [CashierPaymentId] uniqueidentifier NULL,
        [SaleInvoiceId] int NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [OtpVerified] bit NOT NULL,
        [ReceivedAt] datetime2 NOT NULL,
        [DispensedAt] datetime2 NULL,
        [RawFhirJson] nvarchar(max) NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WasafatyPrescriptions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE TABLE [WasafatySyncLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EventType] nvarchar(100) NOT NULL,
        [Direction] int NOT NULL,
        [Status] int NOT NULL,
        [RequestPayload] nvarchar(4000) NULL,
        [ResponsePayload] nvarchar(4000) NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [FhirResourceId] nvarchar(200) NULL,
        [DurationMs] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WasafatySyncLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE TABLE [WasafatyPrescriptionItems] (
        [Id] uniqueidentifier NOT NULL,
        [WasafatyPrescriptionId] uniqueidentifier NOT NULL,
        [DrugCode] nvarchar(50) NOT NULL,
        [DrugName] nvarchar(300) NOT NULL,
        [GenericName] nvarchar(300) NULL,
        [DosageForm] nvarchar(50) NULL,
        [Strength] nvarchar(50) NULL,
        [DosageInstruction] nvarchar(500) NULL,
        [Frequency] nvarchar(100) NULL,
        [DurationDays] int NULL,
        [RequestedQty] decimal(18,2) NOT NULL,
        [DispensedQty] decimal(18,2) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [InsuranceCoveredAmount] decimal(18,2) NOT NULL,
        [PatientPayAmount] decimal(18,2) NOT NULL,
        [AllowSubstitute] bit NOT NULL,
        [IsControlled] bit NOT NULL,
        [ErpProductId] int NULL,
        [BatchNo] nvarchar(50) NULL,
        [ExpiryDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WasafatyPrescriptionItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WasafatyPrescriptionItems_WasafatyPrescriptions_WasafatyPrescriptionId] FOREIGN KEY ([WasafatyPrescriptionId]) REFERENCES [WasafatyPrescriptions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptionItems_DrugCode] ON [WasafatyPrescriptionItems] ([DrugCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptionItems_WasafatyPrescriptionId] ON [WasafatyPrescriptionItems] ([WasafatyPrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_CashierPaymentId] ON [WasafatyPrescriptions] ([CashierPaymentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_WasafatyPrescriptions_FhirResourceId] ON [WasafatyPrescriptions] ([FhirResourceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_InternalPrescriptionId] ON [WasafatyPrescriptions] ([InternalPrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_PatientNationalId] ON [WasafatyPrescriptions] ([PatientNationalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_ReceivedAt] ON [WasafatyPrescriptions] ([ReceivedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_Status] ON [WasafatyPrescriptions] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatySyncLogs_CreatedAt] ON [WasafatySyncLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatySyncLogs_EventType] ON [WasafatySyncLogs] ([EventType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    CREATE INDEX [IX_WasafatySyncLogs_Status] ON [WasafatySyncLogs] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416115121_AddWasafatyIntegration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260416115121_AddWasafatyIntegration', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE TABLE [WasafatyAuthTokens] (
        [Id] uniqueidentifier NOT NULL,
        [AccessToken] nvarchar(max) NOT NULL,
        [RefreshToken] nvarchar(max) NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WasafatyAuthTokens] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE TABLE [WasafatyPrescriptions] (
        [Id] uniqueidentifier NOT NULL,
        [FhirResourceId] nvarchar(200) NOT NULL,
        [PatientNationalId] nvarchar(20) NOT NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [DoctorName] nvarchar(200) NULL,
        [DoctorLicenseNo] nvarchar(50) NULL,
        [PrescriberFacilityName] nvarchar(300) NULL,
        [InsuranceName] nvarchar(200) NULL,
        [InsurancePolicyNo] nvarchar(100) NULL,
        [InsuranceCoveragePercent] decimal(5,2) NOT NULL,
        [ItemsCount] int NOT NULL,
        [Status] int NOT NULL,
        [ClaimStatus] int NOT NULL,
        [NphiesClaimId] nvarchar(100) NULL,
        [InternalPrescriptionId] uniqueidentifier NULL,
        [CashierPaymentId] uniqueidentifier NULL,
        [SaleInvoiceId] int NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [OtpVerified] bit NOT NULL,
        [ReceivedAt] datetime2 NOT NULL,
        [DispensedAt] datetime2 NULL,
        [RawFhirJson] nvarchar(max) NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WasafatyPrescriptions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE TABLE [WasafatySyncLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EventType] nvarchar(100) NOT NULL,
        [Direction] int NOT NULL,
        [Status] int NOT NULL,
        [RequestPayload] nvarchar(4000) NULL,
        [ResponsePayload] nvarchar(4000) NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [FhirResourceId] nvarchar(200) NULL,
        [DurationMs] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WasafatySyncLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE TABLE [WasafatyPrescriptionItems] (
        [Id] uniqueidentifier NOT NULL,
        [WasafatyPrescriptionId] uniqueidentifier NOT NULL,
        [DrugCode] nvarchar(50) NOT NULL,
        [DrugName] nvarchar(300) NOT NULL,
        [GenericName] nvarchar(300) NULL,
        [DosageForm] nvarchar(50) NULL,
        [Strength] nvarchar(50) NULL,
        [DosageInstruction] nvarchar(500) NULL,
        [Frequency] nvarchar(100) NULL,
        [DurationDays] int NULL,
        [RequestedQty] decimal(18,2) NOT NULL,
        [DispensedQty] decimal(18,2) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [InsuranceCoveredAmount] decimal(18,2) NOT NULL,
        [PatientPayAmount] decimal(18,2) NOT NULL,
        [AllowSubstitute] bit NOT NULL,
        [IsControlled] bit NOT NULL,
        [ErpProductId] int NULL,
        [BatchNo] nvarchar(50) NULL,
        [ExpiryDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WasafatyPrescriptionItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WasafatyPrescriptionItems_WasafatyPrescriptions_WasafatyPrescriptionId] FOREIGN KEY ([WasafatyPrescriptionId]) REFERENCES [WasafatyPrescriptions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptionItems_DrugCode] ON [WasafatyPrescriptionItems] ([DrugCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptionItems_WasafatyPrescriptionId] ON [WasafatyPrescriptionItems] ([WasafatyPrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_CashierPaymentId] ON [WasafatyPrescriptions] ([CashierPaymentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_WasafatyPrescriptions_FhirResourceId] ON [WasafatyPrescriptions] ([FhirResourceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_InternalPrescriptionId] ON [WasafatyPrescriptions] ([InternalPrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_PatientNationalId] ON [WasafatyPrescriptions] ([PatientNationalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_ReceivedAt] ON [WasafatyPrescriptions] ([ReceivedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatyPrescriptions_Status] ON [WasafatyPrescriptions] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatySyncLogs_CreatedAt] ON [WasafatySyncLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatySyncLogs_EventType] ON [WasafatySyncLogs] ([EventType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    CREATE INDEX [IX_WasafatySyncLogs_Status] ON [WasafatySyncLogs] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418010301_Add-Wasfaty-Tables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260418010301_Add-Wasfaty-Tables', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418143118_AddPharmacyFieldsToItemPurchaseStore'
)
BEGIN
    ALTER TABLE [StoreDetails] ADD [StoreCategory] nvarchar(max) NOT NULL DEFAULT N'General';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418143118_AddPharmacyFieldsToItemPurchaseStore'
)
BEGIN
    ALTER TABLE [PurchaseItems] ADD [BatchNo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418143118_AddPharmacyFieldsToItemPurchaseStore'
)
BEGIN
    ALTER TABLE [PurchaseItems] ADD [ExpiryDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418143118_AddPharmacyFieldsToItemPurchaseStore'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [IsPharmacyItem] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418143118_AddPharmacyFieldsToItemPurchaseStore'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [ItemType] nvarchar(max) NOT NULL DEFAULT N'General';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418143118_AddPharmacyFieldsToItemPurchaseStore'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [NeedsBatch] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418143118_AddPharmacyFieldsToItemPurchaseStore'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [NeedsExpiry] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418143118_AddPharmacyFieldsToItemPurchaseStore'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [SfdaCode] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418143118_AddPharmacyFieldsToItemPurchaseStore'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260418143118_AddPharmacyFieldsToItemPurchaseStore', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StoreDetails]') AND [c].[name] = N'StoreCategory');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [StoreDetails] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [StoreDetails] ALTER COLUMN [StoreCategory] nvarchar(20) NOT NULL;
    ALTER TABLE [StoreDetails] ADD DEFAULT N'general' FOR [StoreCategory];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MainPurchaseClass]') AND [c].[name] = N'SfdaCode');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [MainPurchaseClass] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [MainPurchaseClass] ALTER COLUMN [SfdaCode] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MainPurchaseClass]') AND [c].[name] = N'NeedsExpiry');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [MainPurchaseClass] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [MainPurchaseClass] ADD DEFAULT CAST(0 AS bit) FOR [NeedsExpiry];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MainPurchaseClass]') AND [c].[name] = N'NeedsBatch');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [MainPurchaseClass] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [MainPurchaseClass] ADD DEFAULT CAST(0 AS bit) FOR [NeedsBatch];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MainPurchaseClass]') AND [c].[name] = N'ItemType');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [MainPurchaseClass] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [MainPurchaseClass] ALTER COLUMN [ItemType] nvarchar(20) NOT NULL;
    ALTER TABLE [MainPurchaseClass] ADD DEFAULT N'general' FOR [ItemType];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MainPurchaseClass]') AND [c].[name] = N'IsPharmacyItem');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [MainPurchaseClass] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [MainPurchaseClass] ADD DEFAULT CAST(0 AS bit) FOR [IsPharmacyItem];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    CREATE TABLE [PharmacyAllocation] (
        [Id] uniqueidentifier NOT NULL,
        [PrescriptionId] uniqueidentifier NOT NULL,
        [AllocationReferenceId] nvarchar(100) NOT NULL,
        [CommitMode] nvarchar(30) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [ConfirmedBy] nvarchar(200) NULL,
        [Notes] nvarchar(500) NULL,
        [PreparedAt] datetime2 NULL,
        [CommittedAt] datetime2 NULL,
        [PayloadJson] nvarchar(max) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyAllocation] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PharmacyAllocation_PharmacyPrescription_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [PharmacyPrescription] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    CREATE TABLE [PharmacyAllocationLine] (
        [Id] uniqueidentifier NOT NULL,
        [AllocationId] uniqueidentifier NOT NULL,
        [PrescriptionItemId] uniqueidentifier NULL,
        [MainPurchaseClassId] int NOT NULL,
        [StoreId] uniqueidentifier NOT NULL,
        [BatchNo] nvarchar(100) NULL,
        [ExpiryDate] datetime2 NULL,
        [RequestedQuantity] decimal(18,2) NOT NULL,
        [CommittedQuantity] decimal(18,2) NOT NULL,
        [AvailableQuantityAtCommit] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyAllocationLine] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PharmacyAllocationLine_PharmacyAllocation_AllocationId] FOREIGN KEY ([AllocationId]) REFERENCES [PharmacyAllocation] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    CREATE TABLE [PharmacyPickup] (
        [Id] uniqueidentifier NOT NULL,
        [PrescriptionId] uniqueidentifier NOT NULL,
        [AllocationId] uniqueidentifier NULL,
        [AllocationReferenceId] nvarchar(100) NULL,
        [AllocationCommitMode] nvarchar(30) NULL,
        [RxNo] nvarchar(50) NOT NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [PaymentType] nvarchar(20) NOT NULL,
        [InsuranceStatus] nvarchar(30) NULL,
        [PrescriptionStatus] nvarchar(30) NOT NULL,
        [StoreId] uniqueidentifier NOT NULL,
        [StoreName] nvarchar(200) NULL,
        [PosTerminalId] nvarchar(50) NULL,
        [CashierUserId] nvarchar(100) NULL,
        [SentAt] datetime2 NOT NULL,
        [PickupStatus] nvarchar(30) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [InsuranceCovered] decimal(18,2) NOT NULL,
        [PatientPayable] decimal(18,2) NOT NULL,
        [PosInvoiceId] int NULL,
        [PosInvoiceNo] nvarchar(50) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyPickup] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PharmacyPickup_PharmacyAllocation_AllocationId] FOREIGN KEY ([AllocationId]) REFERENCES [PharmacyAllocation] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PharmacyPickup_PharmacyPrescription_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [PharmacyPrescription] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyAllocation_AllocationReferenceId] ON [PharmacyAllocation] ([AllocationReferenceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyAllocation_PrescriptionId] ON [PharmacyAllocation] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyAllocationLine_AllocationId] ON [PharmacyAllocationLine] ([AllocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyPickup_AllocationId] ON [PharmacyPickup] ([AllocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyPickup_PickupStatus] ON [PharmacyPickup] ([PickupStatus]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    CREATE INDEX [IX_PharmacyPickup_PrescriptionId] ON [PharmacyPickup] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420092132_AddPharmacyDispenseAndInventoryEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260420092132_AddPharmacyDispenseAndInventoryEntities', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    ALTER TABLE [StoreDetails] ADD [StoreCategory] nvarchar(20) NOT NULL DEFAULT N'general';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    ALTER TABLE [PurchaseItems] ADD [BatchNo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    ALTER TABLE [PurchaseItems] ADD [ExpiryDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [IsPharmacyItem] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [ItemType] nvarchar(20) NOT NULL DEFAULT N'General';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [NeedsBatch] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [NeedsExpiry] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    ALTER TABLE [MainPurchaseClass] ADD [SfdaCode] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    CREATE TABLE [PharmacyAllocation] (
        [Id] uniqueidentifier NOT NULL,
        [PrescriptionId] uniqueidentifier NOT NULL,
        [AllocationReferenceId] nvarchar(100) NOT NULL,
        [CommitMode] nvarchar(30) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [ConfirmedBy] nvarchar(200) NULL,
        [Notes] nvarchar(500) NULL,
        [PreparedAt] datetime2 NULL,
        [CommittedAt] datetime2 NULL,
        [PayloadJson] nvarchar(max) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyAllocation] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PharmacyAllocation_PharmacyPrescription_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [PharmacyPrescription] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    CREATE TABLE [PharmacyAllocationLine] (
        [Id] uniqueidentifier NOT NULL,
        [AllocationId] uniqueidentifier NOT NULL,
        [PrescriptionItemId] uniqueidentifier NULL,
        [MainPurchaseClassId] int NOT NULL,
        [StoreId] uniqueidentifier NOT NULL,
        [BatchNo] nvarchar(100) NULL,
        [ExpiryDate] datetime2 NULL,
        [RequestedQuantity] decimal(18,2) NOT NULL,
        [CommittedQuantity] decimal(18,2) NOT NULL,
        [AvailableQuantityAtCommit] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyAllocationLine] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PharmacyAllocationLine_PharmacyAllocation_AllocationId] FOREIGN KEY ([AllocationId]) REFERENCES [PharmacyAllocation] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    CREATE TABLE [PharmacyPickup] (
        [Id] uniqueidentifier NOT NULL,
        [PrescriptionId] uniqueidentifier NOT NULL,
        [AllocationId] uniqueidentifier NULL,
        [AllocationReferenceId] nvarchar(100) NULL,
        [AllocationCommitMode] nvarchar(30) NULL,
        [RxNo] nvarchar(50) NOT NULL,
        [PatientName] nvarchar(200) NOT NULL,
        [PaymentType] nvarchar(20) NOT NULL,
        [InsuranceStatus] nvarchar(30) NULL,
        [PrescriptionStatus] nvarchar(30) NOT NULL,
        [StoreId] uniqueidentifier NOT NULL,
        [StoreName] nvarchar(200) NULL,
        [PosTerminalId] nvarchar(50) NULL,
        [CashierUserId] nvarchar(100) NULL,
        [SentAt] datetime2 NOT NULL,
        [PickupStatus] nvarchar(30) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [InsuranceCovered] decimal(18,2) NOT NULL,
        [PatientPayable] decimal(18,2) NOT NULL,
        [PosInvoiceId] int NULL,
        [PosInvoiceNo] nvarchar(50) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PharmacyPickup] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PharmacyPickup_PharmacyAllocation_AllocationId] FOREIGN KEY ([AllocationId]) REFERENCES [PharmacyAllocation] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PharmacyPickup_PharmacyPrescription_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [PharmacyPrescription] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PharmacyAllocation_AllocationReferenceId] ON [PharmacyAllocation] ([AllocationReferenceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyAllocation_PrescriptionId] ON [PharmacyAllocation] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyAllocationLine_AllocationId] ON [PharmacyAllocationLine] ([AllocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPickup_AllocationId] ON [PharmacyPickup] ([AllocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPickup_PickupStatus] ON [PharmacyPickup] ([PickupStatus]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPickup_PrescriptionId] ON [PharmacyPickup] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420221916_Add-Additional-Pharmacy-Tables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260420221916_Add-Additional-Pharmacy-Tables', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [RoomType] ADD [ImageUrls] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [RoomType] ADD [LogoPublicId] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [RoomType] ADD [LogoUrl] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [ReservationService] ADD [LogoPublicId] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [ReservationService] ADD [LogoUrl] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [Hotel] ADD [LogoPublicId] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [Hotel] ADD [LogoUrl] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [CompanyDetails] ADD [LogoUrl] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [CompanyDetails] ADD [StampUrl] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [Assets] ADD [Base64Url] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    ALTER TABLE [Assets] ADD [PublicId] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421011551_Add-Cloudinary-Image-Integration-Feilds'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260421011551_Add-Cloudinary-Image-Integration-Feilds', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421102421_FixPharmacyIdTypesToGuid'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyPrescriptionItem]') AND [c].[name] = N'ItemId');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyPrescriptionItem] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [PharmacyPrescriptionItem] ALTER COLUMN [ItemId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421102421_FixPharmacyIdTypesToGuid'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyAllocationLine]') AND [c].[name] = N'MainPurchaseClassId');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyAllocationLine] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [PharmacyAllocationLine] ALTER COLUMN [MainPurchaseClassId] uniqueidentifier NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421102421_FixPharmacyIdTypesToGuid'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260421102421_FixPharmacyIdTypesToGuid', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyPrescriptionItem]') AND [c].[name] = N'ItemId');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyPrescriptionItem] DROP CONSTRAINT [' + @var8 + '];');
    EXEC(N'UPDATE [PharmacyPrescriptionItem] SET [ItemId] = 0 WHERE [ItemId] IS NULL');
    ALTER TABLE [PharmacyPrescriptionItem] ALTER COLUMN [ItemId] int NOT NULL;
    ALTER TABLE [PharmacyPrescriptionItem] ADD DEFAULT 0 FOR [ItemId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [CashierPaymentId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [ClaimResponseJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [CoveragePercentage] decimal(5,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [InsuranceCompanyId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [InsuranceCoveredAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [InsuranceMemberNo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [InsurancePolicyNo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [NphiesAdjudicatedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [NphiesClaimId] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [NphiesStatus] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [NphiesSubmittedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [PatientShareAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [PrescriptionId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [SaleInvoiceId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyAllocationLine]') AND [c].[name] = N'MainPurchaseClassId');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyAllocationLine] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [PharmacyAllocationLine] ALTER COLUMN [MainPurchaseClassId] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    CREATE INDEX [IX_PharmacyInsuranceClaim_InsuranceCompanyId] ON [PharmacyInsuranceClaim] ([InsuranceCompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    CREATE INDEX [IX_PharmacyInsuranceClaim_NphiesClaimId] ON [PharmacyInsuranceClaim] ([NphiesClaimId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    CREATE INDEX [IX_PharmacyInsuranceClaim_PrescriptionId] ON [PharmacyInsuranceClaim] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD CONSTRAINT [FK_PharmacyInsuranceClaim_PharmacyInsuranceCompany_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [PharmacyInsuranceCompany] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD CONSTRAINT [FK_PharmacyInsuranceClaim_PharmacyPrescription_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [PharmacyPrescription] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421105430_AddNphiesFieldsToInsuranceClaim'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260421105430_AddNphiesFieldsToInsuranceClaim', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [FK_PharmacyInsuranceClaim_PharmacyInsuranceCompany_InsuranceCompanyId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [FK_PharmacyInsuranceClaim_PharmacyPrescription_PrescriptionId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DROP INDEX [IX_PharmacyInsuranceClaim_InsuranceCompanyId] ON [PharmacyInsuranceClaim];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DROP INDEX [IX_PharmacyInsuranceClaim_NphiesClaimId] ON [PharmacyInsuranceClaim];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DROP INDEX [IX_PharmacyInsuranceClaim_PrescriptionId] ON [PharmacyInsuranceClaim];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'CashierPaymentId');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [CashierPaymentId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'ClaimResponseJson');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [ClaimResponseJson];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'CoveragePercentage');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [CoveragePercentage];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'InsuranceCompanyId');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [InsuranceCompanyId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'InsuranceCoveredAmount');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [InsuranceCoveredAmount];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'InsuranceMemberNo');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [InsuranceMemberNo];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'InsurancePolicyNo');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [InsurancePolicyNo];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'NphiesAdjudicatedAt');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [NphiesAdjudicatedAt];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'NphiesClaimId');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [NphiesClaimId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'NphiesStatus');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [NphiesStatus];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'NphiesSubmittedAt');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [NphiesSubmittedAt];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'PatientShareAmount');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [PatientShareAmount];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'PrescriptionId');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [PrescriptionId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyInsuranceClaim]') AND [c].[name] = N'SaleInvoiceId');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyInsuranceClaim] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [PharmacyInsuranceClaim] DROP COLUMN [SaleInvoiceId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE TABLE [ZidAuthTokens] (
        [Id] uniqueidentifier NOT NULL,
        [AccessToken] nvarchar(max) NOT NULL,
        [RefreshToken] nvarchar(max) NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidAuthTokens] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE TABLE [ZidOrders] (
        [Id] uniqueidentifier NOT NULL,
        [ZidOrderId] bigint NOT NULL,
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
        [ZidCustomerId] bigint NULL,
        [ShippingCity] nvarchar(max) NULL,
        [ShippingDistrict] nvarchar(max) NULL,
        [ShippingStreet] nvarchar(max) NULL,
        [ShippingPostalCode] nvarchar(max) NULL,
        [ShippingLat] float NULL,
        [ShippingLng] float NULL,
        [PaymentMethod] nvarchar(50) NULL,
        [ZidPaymentId] nvarchar(100) NULL,
        [SaleInvoiceId] int NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [SyncStatus] int NOT NULL,
        [SyncError] nvarchar(1000) NULL,
        [SyncedAt] datetime2 NULL,
        [ZidCreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidOrders] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE TABLE [ZidProductMappings] (
        [Id] uniqueidentifier NOT NULL,
        [ZidProductId] bigint NOT NULL,
        [ZidSku] nvarchar(100) NULL,
        [ZidProductName] nvarchar(300) NOT NULL,
        [ZidPrice] decimal(18,2) NULL,
        [MainPurchaseClassId] uniqueidentifier NULL,
        [MainPurchaseClassName] nvarchar(300) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [LastSyncedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidProductMappings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE TABLE [ZidSyncLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EventType] nvarchar(100) NOT NULL,
        [Direction] int NOT NULL,
        [Status] int NOT NULL,
        [RequestPayload] nvarchar(max) NULL,
        [ResponsePayload] nvarchar(max) NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [RelatedEntityId] uniqueidentifier NULL,
        [ZidResourceId] bigint NULL,
        [DurationMs] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidSyncLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE TABLE [ZidOrderItems] (
        [Id] uniqueidentifier NOT NULL,
        [ZidOrderId] uniqueidentifier NOT NULL,
        [ZidProductId] bigint NOT NULL,
        [ZidSku] nvarchar(100) NULL,
        [ProductName] nvarchar(300) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [OptionsSummary] nvarchar(500) NULL,
        [MainPurchaseClassId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidOrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ZidOrderItems_ZidOrders_ZidOrderId] FOREIGN KEY ([ZidOrderId]) REFERENCES [ZidOrders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidOrderItems_MainPurchaseClassId] ON [ZidOrderItems] ([MainPurchaseClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidOrderItems_ZidOrderId] ON [ZidOrderItems] ([ZidOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidOrderItems_ZidProductId] ON [ZidOrderItems] ([ZidProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidOrders_Status] ON [ZidOrders] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidOrders_StoreDetailsId] ON [ZidOrders] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidOrders_SyncStatus] ON [ZidOrders] ([SyncStatus]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidOrders_ZidCreatedAt] ON [ZidOrders] ([ZidCreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ZidOrders_ZidOrderId] ON [ZidOrders] ([ZidOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidProductMappings_MainPurchaseClassId] ON [ZidProductMappings] ([MainPurchaseClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidProductMappings_StoreDetailsId] ON [ZidProductMappings] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ZidProductMappings_ZidProductId] ON [ZidProductMappings] ([ZidProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidSyncLogs_CreatedAt] ON [ZidSyncLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidSyncLogs_EventType] ON [ZidSyncLogs] ([EventType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidSyncLogs_Status] ON [ZidSyncLogs] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    CREATE INDEX [IX_ZidSyncLogs_ZidResourceId] ON [ZidSyncLogs] ([ZidResourceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260421111635_AddZidIntegrationTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260421111635_AddZidIntegrationTables', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyPrescriptionItem]') AND [c].[name] = N'ItemId');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyPrescriptionItem] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [PharmacyPrescriptionItem] DROP COLUMN [ItemId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyPrescriptionItem] ADD [ItemId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [CashierPaymentId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [ClaimResponseJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [CoveragePercentage] decimal(5,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [InsuranceCompanyId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [InsuranceCoveredAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [InsuranceMemberNo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [InsurancePolicyNo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [NphiesAdjudicatedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [NphiesClaimId] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [NphiesStatus] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [NphiesSubmittedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [PatientShareAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [PrescriptionId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD [SaleInvoiceId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PharmacyAllocationLine]') AND [c].[name] = N'MainPurchaseClassId');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [PharmacyAllocationLine] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [PharmacyAllocationLine] DROP COLUMN [MainPurchaseClassId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyAllocationLine] ADD [MainPurchaseClassId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE TABLE [ZidAuthTokens] (
        [Id] uniqueidentifier NOT NULL,
        [AccessToken] nvarchar(max) NOT NULL,
        [RefreshToken] nvarchar(max) NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidAuthTokens] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE TABLE [ZidOrders] (
        [Id] uniqueidentifier NOT NULL,
        [ZidOrderId] bigint NOT NULL,
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
        [ZidCustomerId] bigint NULL,
        [ShippingCity] nvarchar(max) NULL,
        [ShippingDistrict] nvarchar(max) NULL,
        [ShippingStreet] nvarchar(max) NULL,
        [ShippingPostalCode] nvarchar(max) NULL,
        [ShippingLat] float NULL,
        [ShippingLng] float NULL,
        [PaymentMethod] nvarchar(50) NULL,
        [ZidPaymentId] nvarchar(100) NULL,
        [SaleInvoiceId] int NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [SyncStatus] int NOT NULL,
        [SyncError] nvarchar(1000) NULL,
        [SyncedAt] datetime2 NULL,
        [ZidCreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidOrders] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE TABLE [ZidProductMappings] (
        [Id] uniqueidentifier NOT NULL,
        [ZidProductId] bigint NOT NULL,
        [ZidSku] nvarchar(100) NULL,
        [ZidProductName] nvarchar(300) NOT NULL,
        [ZidPrice] decimal(18,2) NULL,
        [MainPurchaseClassId] uniqueidentifier NULL,
        [MainPurchaseClassName] nvarchar(300) NULL,
        [StoreDetailsId] uniqueidentifier NULL,
        [LastSyncedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidProductMappings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE TABLE [ZidSyncLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EventType] nvarchar(100) NOT NULL,
        [Direction] int NOT NULL,
        [Status] int NOT NULL,
        [RequestPayload] nvarchar(max) NULL,
        [ResponsePayload] nvarchar(max) NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [RelatedEntityId] uniqueidentifier NULL,
        [ZidResourceId] bigint NULL,
        [DurationMs] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidSyncLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE TABLE [ZidOrderItems] (
        [Id] uniqueidentifier NOT NULL,
        [ZidOrderId] uniqueidentifier NOT NULL,
        [ZidProductId] bigint NOT NULL,
        [ZidSku] nvarchar(100) NULL,
        [ProductName] nvarchar(300) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [OptionsSummary] nvarchar(500) NULL,
        [MainPurchaseClassId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ZidOrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ZidOrderItems_ZidOrders_ZidOrderId] FOREIGN KEY ([ZidOrderId]) REFERENCES [ZidOrders] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyPrescriptionItem_ItemId] ON [PharmacyPrescriptionItem] ([ItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyInsuranceClaim_InsuranceCompanyId] ON [PharmacyInsuranceClaim] ([InsuranceCompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyInsuranceClaim_NphiesClaimId] ON [PharmacyInsuranceClaim] ([NphiesClaimId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyInsuranceClaim_PrescriptionId] ON [PharmacyInsuranceClaim] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_PharmacyAllocationLine_MainPurchaseClassId] ON [PharmacyAllocationLine] ([MainPurchaseClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidOrderItems_MainPurchaseClassId] ON [ZidOrderItems] ([MainPurchaseClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidOrderItems_ZidOrderId] ON [ZidOrderItems] ([ZidOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidOrderItems_ZidProductId] ON [ZidOrderItems] ([ZidProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidOrders_Status] ON [ZidOrders] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidOrders_StoreDetailsId] ON [ZidOrders] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidOrders_SyncStatus] ON [ZidOrders] ([SyncStatus]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidOrders_ZidCreatedAt] ON [ZidOrders] ([ZidCreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ZidOrders_ZidOrderId] ON [ZidOrders] ([ZidOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidProductMappings_MainPurchaseClassId] ON [ZidProductMappings] ([MainPurchaseClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidProductMappings_StoreDetailsId] ON [ZidProductMappings] ([StoreDetailsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ZidProductMappings_ZidProductId] ON [ZidProductMappings] ([ZidProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidSyncLogs_CreatedAt] ON [ZidSyncLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidSyncLogs_EventType] ON [ZidSyncLogs] ([EventType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidSyncLogs_Status] ON [ZidSyncLogs] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    CREATE INDEX [IX_ZidSyncLogs_ZidResourceId] ON [ZidSyncLogs] ([ZidResourceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyAllocationLine] ADD CONSTRAINT [FK_PharmacyAllocationLine_MainPurchaseClass_MainPurchaseClassId] FOREIGN KEY ([MainPurchaseClassId]) REFERENCES [MainPurchaseClass] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD CONSTRAINT [FK_PharmacyInsuranceClaim_PharmacyInsuranceCompany_InsuranceCompanyId] FOREIGN KEY ([InsuranceCompanyId]) REFERENCES [PharmacyInsuranceCompany] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyInsuranceClaim] ADD CONSTRAINT [FK_PharmacyInsuranceClaim_PharmacyPrescription_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [PharmacyPrescription] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    ALTER TABLE [PharmacyPrescriptionItem] ADD CONSTRAINT [FK_PharmacyPrescriptionItem_MainPurchaseClass_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [MainPurchaseClass] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422011739_Add-Tameen-And-Zid-Integration-Tables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260422011739_Add-Tameen-And-Zid-Integration-Tables', N'8.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425151700_AddPharmacyReturnReferenceFields'
)
BEGIN
    ALTER TABLE [PharmacyReturn] ADD [InvoiceId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425151700_AddPharmacyReturnReferenceFields'
)
BEGIN
    ALTER TABLE [PharmacyReturn] ADD [ReferenceNo] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425151700_AddPharmacyReturnReferenceFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425151700_AddPharmacyReturnReferenceFields', N'8.0.3');
END;
GO

COMMIT;
GO

