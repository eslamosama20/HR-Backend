using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mergedAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DomainDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoginActionLogs",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsFailedTrail = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginActionLogs", x => new { x.UserId, x.LoginTime });
                });

            migrationBuilder.CreateTable(
                name: "OTPStatuses",
                columns: table => new
                {
                    StatusID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArabicDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPStatuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "OTPType",
                columns: table => new
                {
                    ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPType", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "passwordsLogs",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwordsLogs", x => new { x.UserId, x.CreatedAt });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArabicFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArabicMiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArabicLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishMiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnglishLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DomainDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_DomainDetails_DomainDetailsId",
                        column: x => x.DomainDetailsId,
                        principalTable: "DomainDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DomainDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_DomainDetails_DomainDetailsId",
                        column: x => x.DomainDetailsId,
                        principalTable: "DomainDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OTPs",
                columns: table => new
                {
                    OTPReference = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OTPReferenceKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OTPPayload = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumberOfFailedTrails = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ExpiryDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    StatusID = table.Column<short>(type: "smallint", nullable: true),
                    OtpTypeID = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPs", x => x.OTPReference);
                    table.ForeignKey(
                        name: "FK_OTPs_OTPStatuses_StatusID",
                        column: x => x.StatusID,
                        principalTable: "OTPStatuses",
                        principalColumn: "StatusID");
                    table.ForeignKey(
                        name: "FK_OTPs_OTPType_OtpTypeID",
                        column: x => x.OtpTypeID,
                        principalTable: "OTPType",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainDetails_Name",
                table: "DomainDetails",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OTPs_OtpTypeID",
                table: "OTPs",
                column: "OtpTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_OTPs_StatusID",
                table: "OTPs",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_DomainDetailsId",
                table: "Permissions",
                column: "DomainDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_DomainDetailsId",
                table: "Roles",
                column: "DomainDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginActionLogs");

            migrationBuilder.DropTable(
                name: "OTPs");

            migrationBuilder.DropTable(
                name: "passwordsLogs");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "OTPStatuses");

            migrationBuilder.DropTable(
                name: "OTPType");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DomainDetails");
        }
    }
}
