using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxBanksCount')
                    ALTER TABLE [Roles] ADD [MaxBanksCount] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxBranchesCount')
                    ALTER TABLE [Roles] ADD [MaxBranchesCount] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxCompaniesCount')
                    ALTER TABLE [Roles] ADD [MaxCompaniesCount] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxCustomersCount')
                    ALTER TABLE [Roles] ADD [MaxCustomersCount] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxStoresCount')
                    ALTER TABLE [Roles] ADD [MaxStoresCount] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxSuppliersCount')
                    ALTER TABLE [Roles] ADD [MaxSuppliersCount] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxTreasuriesCount')
                    ALTER TABLE [Roles] ADD [MaxTreasuriesCount] int NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxUsersCount')
                    ALTER TABLE [Roles] ADD [MaxUsersCount] int NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxBanksCount')
                    ALTER TABLE [Roles] DROP COLUMN [MaxBanksCount];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxBranchesCount')
                    ALTER TABLE [Roles] DROP COLUMN [MaxBranchesCount];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxCompaniesCount')
                    ALTER TABLE [Roles] DROP COLUMN [MaxCompaniesCount];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxCustomersCount')
                    ALTER TABLE [Roles] DROP COLUMN [MaxCustomersCount];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxStoresCount')
                    ALTER TABLE [Roles] DROP COLUMN [MaxStoresCount];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxSuppliersCount')
                    ALTER TABLE [Roles] DROP COLUMN [MaxSuppliersCount];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxTreasuriesCount')
                    ALTER TABLE [Roles] DROP COLUMN [MaxTreasuriesCount];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Roles]') AND name = 'MaxUsersCount')
                    ALTER TABLE [Roles] DROP COLUMN [MaxUsersCount];
            ");
        }
    }
}
