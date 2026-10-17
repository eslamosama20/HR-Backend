using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymobIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PublicBookings]') AND name = 'PaymobOrderId')
                BEGIN
                    ALTER TABLE [dbo].[PublicBookings] ADD [PaymobOrderId] int NULL
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PublicBookings]') AND name = 'PaymobOrderId')
                BEGIN
                    ALTER TABLE [dbo].[PublicBookings] DROP COLUMN [PaymobOrderId]
                END
            ");
        }
    }
}
