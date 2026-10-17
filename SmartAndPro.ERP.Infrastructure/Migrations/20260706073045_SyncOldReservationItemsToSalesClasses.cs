using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncOldReservationItemsToSalesClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
DECLARE @RootId UNIQUEIDENTIFIER = NEWID();
INSERT INTO RootSalesClass (Id, ArabicName, EnglishName, IsActive, CreatedAt, ReferenceCode, CreatedBy)
VALUES (@RootId, N'إيرادات الحجوزات', 'Reservation Revenues', 1, GETUTCDATE(), 'RR-001', '00000000-0000-0000-0000-000000000000');

DECLARE @HeaderId UNIQUEIDENTIFIER = NEWID();
INSERT INTO HeaderSalesClass (Id, ArabicName, EnglishName, IsActive, CreatedAt, ReferenceCode, RootSalesClassId, CreatedBy)
VALUES (@HeaderId, N'إيرادات الغرف والخدمات', 'Rooms & Services Revenues', 1, GETUTCDATE(), 'HR-001', @RootId, '00000000-0000-0000-0000-000000000000');

DECLARE @DetailId UNIQUEIDENTIFIER = NEWID();
INSERT INTO DetailSalesClass (Id, ArabicName, EnglishName, IsActive, CreatedAt, ReferenceCode, HeaderSalesClassId, CreatedBy)
VALUES (@DetailId, N'أصناف الحجوزات', 'Reservation Items', 1, GETUTCDATE(), 'RS-001', @HeaderId, '00000000-0000-0000-0000-000000000000');

DECLARE @UnitId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MeasurementUnit WHERE EnglishName = '-');
IF @UnitId IS NULL
BEGIN
    SET @UnitId = NEWID();
    INSERT INTO MeasurementUnit (Id, ArabicName, EnglishName, ArabicSymbol, EnglishSymbol, CreatedAt, IsActive, CreatedBy)
    VALUES (@UnitId, N'-', '-', N'-', '-', GETUTCDATE(), 1, '00000000-0000-0000-0000-000000000000');
END

-- Migrate RoomType
DECLARE @Id UNIQUEIDENTIFIER, @ArName NVARCHAR(MAX), @EnName NVARCHAR(MAX);
DECLARE db_cursor CURSOR FOR SELECT Id, ArabicName, EnglishName FROM RoomType WHERE SalesClassId IS NULL;
OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @Id, @ArName, @EnName;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @ExistingSalesClassId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MainSalesClass WHERE (ArabicName = @ArName OR EnglishName = @EnName) AND IsReservationItem = 1);
    
    IF @ExistingSalesClassId IS NULL
    BEGIN
        SET @ExistingSalesClassId = NEWID();
        INSERT INTO MainSalesClass (Id, ArabicName, EnglishName, ReferenceCode, DetailSalesClassId, ClassType, MeasurementUnitId, IsTaxExempted, IsReservationItem, IsActive, CreatedAt, CreatedBy)
        VALUES (@ExistingSalesClassId, @ArName, @EnName, CAST(NEWID() AS NVARCHAR(36)), @DetailId, 0, @UnitId, 0, 1, 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000');
    END

    UPDATE RoomType SET SalesClassId = @ExistingSalesClassId WHERE Id = @Id;

    FETCH NEXT FROM db_cursor INTO @Id, @ArName, @EnName;
END;
CLOSE db_cursor;
DEALLOCATE db_cursor;

-- Migrate ReservationMeal
DECLARE db_cursor_meal CURSOR FOR SELECT Id, ArabicName, EnglishName FROM ReservationMeal WHERE SalesClassId IS NULL;
OPEN db_cursor_meal;
FETCH NEXT FROM db_cursor_meal INTO @Id, @ArName, @EnName;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @ExistingSalesClassIdMeal UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MainSalesClass WHERE (ArabicName = @ArName OR EnglishName = @EnName) AND IsReservationItem = 1);
    
    IF @ExistingSalesClassIdMeal IS NULL
    BEGIN
        SET @ExistingSalesClassIdMeal = NEWID();
        INSERT INTO MainSalesClass (Id, ArabicName, EnglishName, ReferenceCode, DetailSalesClassId, ClassType, MeasurementUnitId, IsTaxExempted, IsReservationItem, IsActive, CreatedAt, CreatedBy)
        VALUES (@ExistingSalesClassIdMeal, @ArName, @EnName, CAST(NEWID() AS NVARCHAR(36)), @DetailId, 0, @UnitId, 0, 1, 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000');
    END

    UPDATE ReservationMeal SET SalesClassId = @ExistingSalesClassIdMeal WHERE Id = @Id;

    FETCH NEXT FROM db_cursor_meal INTO @Id, @ArName, @EnName;
END;
CLOSE db_cursor_meal;
DEALLOCATE db_cursor_meal;

-- Migrate ReservationView
DECLARE db_cursor_view CURSOR FOR SELECT Id, ArabicName, EnglishName FROM ReservationView WHERE SalesClassId IS NULL;
OPEN db_cursor_view;
FETCH NEXT FROM db_cursor_view INTO @Id, @ArName, @EnName;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @ExistingSalesClassIdView UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MainSalesClass WHERE (ArabicName = @ArName OR EnglishName = @EnName) AND IsReservationItem = 1);
    
    IF @ExistingSalesClassIdView IS NULL
    BEGIN
        SET @ExistingSalesClassIdView = NEWID();
        INSERT INTO MainSalesClass (Id, ArabicName, EnglishName, ReferenceCode, DetailSalesClassId, ClassType, MeasurementUnitId, IsTaxExempted, IsReservationItem, IsActive, CreatedAt, CreatedBy)
        VALUES (@ExistingSalesClassIdView, @ArName, @EnName, CAST(NEWID() AS NVARCHAR(36)), @DetailId, 0, @UnitId, 0, 1, 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000');
    END

    UPDATE ReservationView SET SalesClassId = @ExistingSalesClassIdView WHERE Id = @Id;

    FETCH NEXT FROM db_cursor_view INTO @Id, @ArName, @EnName;
END;
CLOSE db_cursor_view;
DEALLOCATE db_cursor_view;

-- Migrate ReservationService
DECLARE db_cursor_service CURSOR FOR SELECT Id, ArabicName, EnglishName FROM ReservationService WHERE SalesClassId IS NULL;
OPEN db_cursor_service;
FETCH NEXT FROM db_cursor_service INTO @Id, @ArName, @EnName;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @ExistingSalesClassIdService UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MainSalesClass WHERE (ArabicName = @ArName OR EnglishName = @EnName) AND IsReservationItem = 1);
    
    IF @ExistingSalesClassIdService IS NULL
    BEGIN
        SET @ExistingSalesClassIdService = NEWID();
        INSERT INTO MainSalesClass (Id, ArabicName, EnglishName, ReferenceCode, DetailSalesClassId, ClassType, MeasurementUnitId, IsTaxExempted, IsReservationItem, IsActive, CreatedAt, CreatedBy)
        VALUES (@ExistingSalesClassIdService, @ArName, @EnName, CAST(NEWID() AS NVARCHAR(36)), @DetailId, 0, @UnitId, 0, 1, 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000');
    END

    UPDATE ReservationService SET SalesClassId = @ExistingSalesClassIdService WHERE Id = @Id;

    FETCH NEXT FROM db_cursor_service INTO @Id, @ArName, @EnName;
END;
CLOSE db_cursor_service;
DEALLOCATE db_cursor_service;
            ";
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
