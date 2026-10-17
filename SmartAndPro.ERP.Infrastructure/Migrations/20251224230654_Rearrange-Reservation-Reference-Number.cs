using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RearrangeReservationReferenceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ------------------------------------------------
                -- 1️⃣ ترتيب الحجوزات العادية (RES)
                ------------------------------------------------
                ;WITH NormalReservations AS
                (
                    SELECT 
                        Id,
                        ROW_NUMBER() OVER (ORDER BY CreatedAt) AS Seq
                    FROM Reservation
                    WHERE IsFrontal = 0
                )
                UPDATE r
                SET ReferenceNumber =
                    CONCAT('RES-', RIGHT('00000' + CAST(n.Seq AS VARCHAR(5)), 5))
                FROM Reservation r
                INNER JOIN NormalReservations n ON r.Id = n.Id;

                ------------------------------------------------
                -- 2️⃣ ترتيب الحجوزات الفرونتال (FRES)
                ------------------------------------------------
                ;WITH FrontalReservations AS
                (
                    SELECT 
                        Id,
                        ROW_NUMBER() OVER (ORDER BY CreatedAt) AS Seq
                    FROM Reservation
                    WHERE IsFrontal = 1
                )
                UPDATE r
                SET ReferenceNumber =
                    CONCAT('FRES-', CAST(f.Seq AS VARCHAR))
                FROM Reservation r
                INNER JOIN FrontalReservations f ON r.Id = f.Id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
