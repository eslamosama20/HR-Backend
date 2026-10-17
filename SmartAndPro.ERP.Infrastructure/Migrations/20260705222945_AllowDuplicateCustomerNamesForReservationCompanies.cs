using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowDuplicateCustomerNamesForReservationCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- 0. Ensure all existing customers are explicitly marked as regular customers (IsReservationCompany = 0)
                UPDATE CustomerProfile SET IsReservationCompany = 0;

                -- 0.5 Ensure any CustomerProfiles that are ALREADY manually linked to a ReservationCompany have IsReservationCompany = 1
                UPDATE cp
                SET cp.IsReservationCompany = 1
                FROM CustomerProfile cp
                INNER JOIN ReservationCompany rc ON cp.Id = rc.CustomerProfileId;

                -- 1. Link companies that already have a CustomerProfile with the same ArabicName
                UPDATE rc
                SET rc.CustomerProfileId = cp.Id
                FROM ReservationCompany rc
                INNER JOIN CustomerProfile cp ON rc.ArabicName = cp.ArabicName
                WHERE rc.CustomerProfileId IS NULL AND cp.IsReservationCompany = 1;

                -- 2. For the remaining companies, create new CustomerProfiles, handling duplicates by ArabicName
                DECLARE @TempTable TABLE (
                    ArabicName NVARCHAR(450),
                    CustomerId UNIQUEIDENTIFIER
                );

                -- Insert UNIQUE ArabicNames and generate a new ID for each
                INSERT INTO @TempTable (ArabicName, CustomerId)
                SELECT ArabicName, NEWID() 
                FROM (
                    SELECT DISTINCT ArabicName 
                    FROM ReservationCompany 
                    WHERE CustomerProfileId IS NULL AND ArabicName IS NOT NULL
                ) AS UniqueNames;

                -- Insert ONE CustomerProfile per unique ArabicName
                INSERT INTO CustomerProfile (
                    Id, ArabicName, EnglishName, MainAccountId, ContactNumber, Email, BankName, BankAccountNumber, IBAN, TaxNumber, 
                    Street, District, City, Country, BuildingNumber, SubNumber, PostalCode, ShortAddress, AdditionalContactNumber, Fax, Website, Instructions, GaztNumber, 
                    IsFrontal, IsReservationCompany, IsActive, CreatedAt, CreatedBy
                )
                SELECT 
                    t.CustomerId, t.ArabicName, rc.EnglishName, rc.MainAccountId, rc.ContactNumber, rc.Email, rc.BankName, rc.BankAccountNumber, rc.IBAN, rc.TaxNumber, 
                    rc.Street, rc.District, rc.City, rc.Country, rc.BuildingNumber, rc.SubNumber, rc.PostalCode, rc.ShortAddress, rc.AdditionalContactNumber, rc.Fax, rc.Website, rc.Instructions, rc.GaztNumber, 
                    rc.IsFrontal, 1, rc.IsActive, rc.CreatedAt, rc.CreatedBy
                FROM @TempTable t
                CROSS APPLY (
                    SELECT TOP 1 * 
                    FROM ReservationCompany 
                    WHERE ArabicName = t.ArabicName AND CustomerProfileId IS NULL
                ) rc;

                -- Link ALL unlinked ReservationCompanies to the new CustomerProfiles based on ArabicName
                UPDATE rc
                SET rc.CustomerProfileId = t.CustomerId
                FROM ReservationCompany rc
                INNER JOIN @TempTable t ON rc.ArabicName = t.ArabicName
                WHERE rc.CustomerProfileId IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
