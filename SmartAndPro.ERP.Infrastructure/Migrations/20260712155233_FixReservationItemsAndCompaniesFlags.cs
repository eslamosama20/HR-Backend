using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReservationItemsAndCompaniesFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
DECLARE @UnitId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM MeasurementUnit WHERE EnglishName = '-' OR ArabicName = '-');

-- 1. Fix MainSalesClass properties for Reservation Items
UPDATE MainSalesClass
SET IsReservationItem = 1,
    MeasurementUnitId = @UnitId
WHERE Id IN (
    SELECT SalesClassId FROM RoomType WHERE SalesClassId IS NOT NULL
    UNION
    SELECT SalesClassId FROM ReservationMeal WHERE SalesClassId IS NOT NULL
    UNION
    SELECT SalesClassId FROM ReservationView WHERE SalesClassId IS NOT NULL
    UNION
    SELECT SalesClassId FROM ReservationService WHERE SalesClassId IS NOT NULL
);

-- 2. Fix CustomerProfile properties for Reservation Companies
UPDATE CustomerProfile
SET IsReservationCompany = 1
WHERE Id IN (
    SELECT CustomerProfileId FROM ReservationCompany WHERE CustomerProfileId IS NOT NULL
);
";
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
