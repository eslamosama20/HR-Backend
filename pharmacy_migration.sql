BEGIN TRANSACTION;
GO

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
GO

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
GO

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
GO

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
GO

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
GO

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
GO

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
GO

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
GO

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
GO

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
    [InsuranceName] nvarchar(200) NULL,
    [InsuranceEnabled] bit NOT NULL,
    [InsuranceCompanyId] uniqueidentifier NULL,
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
GO

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
GO

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
GO

CREATE INDEX [IX_PharmacyAttachment_PrescriptionId] ON [PharmacyAttachment] ([PrescriptionId]);
GO

CREATE UNIQUE INDEX [IX_PharmacyAttachment_SeqNo] ON [PharmacyAttachment] ([SeqNo]);
GO

CREATE INDEX [IX_PharmacyDoctor_LicenseNo] ON [PharmacyDoctor] ([LicenseNo]);
GO

CREATE INDEX [IX_PharmacyDoctor_Name] ON [PharmacyDoctor] ([Name]);
GO

CREATE UNIQUE INDEX [IX_PharmacyDoctor_SeqNo] ON [PharmacyDoctor] ([SeqNo]);
GO

CREATE INDEX [IX_PharmacyInsuranceCompany_Name] ON [PharmacyInsuranceCompany] ([Name]);
GO

CREATE UNIQUE INDEX [IX_PharmacyInsuranceCompany_SeqNo] ON [PharmacyInsuranceCompany] ([SeqNo]);
GO

CREATE INDEX [IX_PharmacyPatient_Mobile] ON [PharmacyPatient] ([Mobile]);
GO

CREATE INDEX [IX_PharmacyPatient_NationalId] ON [PharmacyPatient] ([NationalId]);
GO

CREATE INDEX [IX_PharmacyPatient_PatientCode] ON [PharmacyPatient] ([PatientCode]);
GO

CREATE UNIQUE INDEX [IX_PharmacyPatient_SeqNo] ON [PharmacyPatient] ([SeqNo]);
GO

CREATE INDEX [IX_PharmacyPrescription_DoctorId] ON [PharmacyPrescription] ([DoctorId]);
GO

CREATE INDEX [IX_PharmacyPrescription_InsuranceCompanyId] ON [PharmacyPrescription] ([InsuranceCompanyId]);
GO

CREATE INDEX [IX_PharmacyPrescription_PatientId] ON [PharmacyPrescription] ([PatientId]);
GO

CREATE INDEX [IX_PharmacyPrescription_RxNo] ON [PharmacyPrescription] ([RxNo]);
GO

CREATE UNIQUE INDEX [IX_PharmacyPrescription_SeqNo] ON [PharmacyPrescription] ([SeqNo]);
GO

CREATE INDEX [IX_PharmacyPrescription_Status] ON [PharmacyPrescription] ([Status]);
GO

CREATE INDEX [IX_PharmacyPrescriptionItem_PrescriptionId] ON [PharmacyPrescriptionItem] ([PrescriptionId]);
GO

CREATE UNIQUE INDEX [IX_PharmacyPrescriptionItem_SeqNo] ON [PharmacyPrescriptionItem] ([SeqNo]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260414105125_UpdatePharmacyEntitiesV2', N'8.0.3');
GO

COMMIT;
GO

