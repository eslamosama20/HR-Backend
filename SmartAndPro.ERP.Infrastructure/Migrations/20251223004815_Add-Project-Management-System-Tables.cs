using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectManagementSystemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CompanyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDepartments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDepartments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProjectNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectAssignmentPhases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExecutingEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExecutingEntityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupervisingEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupervisingEntityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignmentApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionDuration = table.Column<int>(type: "int", nullable: true),
                    ExecutionDurationUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAssignmentPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAssignmentPhases_ProjectCompanies_ExecutingEntityId",
                        column: x => x.ExecutingEntityId,
                        principalTable: "ProjectCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectAssignmentPhases_ProjectCompanies_SupervisingEntityId",
                        column: x => x.SupervisingEntityId,
                        principalTable: "ProjectCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectAssignmentPhases_ProjectDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "ProjectDepartments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectAssignmentPhases_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetaData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDocuments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectExecutionPhases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrantAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExecutionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CompletionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    TransferIssued = table.Column<bool>(type: "bit", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractDataSent = table.Column<bool>(type: "bit", nullable: false),
                    ContractSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalReportSent = table.Column<bool>(type: "bit", nullable: false),
                    FinalReportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectExecutionPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectExecutionPhases_ProjectCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "ProjectCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectExecutionPhases_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActionBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectHistories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPayments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssessedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AssessmentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AssessmentNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAssessments_ProjectDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "ProjectDocuments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectAssessments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPriceQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuoteNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QuoteAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPriceQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPriceQuotes_ProjectCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "ProjectCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectPriceQuotes_ProjectDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "ProjectDocuments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectPriceQuotes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssessments_DocumentId",
                table: "ProjectAssessments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssessments_ProjectId",
                table: "ProjectAssessments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignmentPhases_DepartmentId",
                table: "ProjectAssignmentPhases",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignmentPhases_ExecutingEntityId",
                table: "ProjectAssignmentPhases",
                column: "ExecutingEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignmentPhases_ProjectId",
                table: "ProjectAssignmentPhases",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignmentPhases_SupervisingEntityId",
                table: "ProjectAssignmentPhases",
                column: "SupervisingEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_ProjectId",
                table: "ProjectDocuments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectExecutionPhases_CompanyId",
                table: "ProjectExecutionPhases",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectExecutionPhases_ProjectId",
                table: "ProjectExecutionPhases",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectHistories_ProjectId",
                table: "ProjectHistories",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPayments_ProjectId",
                table: "ProjectPayments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPriceQuotes_CompanyId",
                table: "ProjectPriceQuotes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPriceQuotes_DocumentId",
                table: "ProjectPriceQuotes",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPriceQuotes_ProjectId",
                table: "ProjectPriceQuotes",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectAssessments");

            migrationBuilder.DropTable(
                name: "ProjectAssignmentPhases");

            migrationBuilder.DropTable(
                name: "ProjectExecutionPhases");

            migrationBuilder.DropTable(
                name: "ProjectHistories");

            migrationBuilder.DropTable(
                name: "ProjectPayments");

            migrationBuilder.DropTable(
                name: "ProjectPriceQuotes");

            migrationBuilder.DropTable(
                name: "ProjectDepartments");

            migrationBuilder.DropTable(
                name: "ProjectCompanies");

            migrationBuilder.DropTable(
                name: "ProjectDocuments");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
