using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HotFixToTemplateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Footer_Columns_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                table: "Footer_Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Footer_Items_Footer_Rows_PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupId_PdfRowGroupId",
                table: "Footer_Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Footer_Rows_Footer_Columns_PdfColumnGroupPdfTemplateConfigurationId_PdfColumnGroupId",
                table: "Footer_Rows");

            migrationBuilder.DropForeignKey(
                name: "FK_Header_Columns_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                table: "Header_Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Header_Items_Header_Rows_PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupId_PdfRowGroupId",
                table: "Header_Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Header_Rows_Header_Columns_PdfColumnGroupPdfTemplateConfigurationId_PdfColumnGroupId",
                table: "Header_Rows");

            migrationBuilder.RenameColumn(
                name: "PdfColumnGroupPdfTemplateConfigurationId",
                table: "Header_Rows",
                newName: "PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId",
                table: "Header_Items",
                newName: "PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfTemplateConfigurationId",
                table: "Header_Columns",
                newName: "PdfHeaderFooterConfigPdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfColumnGroupPdfTemplateConfigurationId",
                table: "Footer_Rows",
                newName: "PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId",
                table: "Footer_Items",
                newName: "PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfTemplateConfigurationId",
                table: "Footer_Columns",
                newName: "PdfHeaderFooterConfigPdfTemplateConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Footer_Columns_PdfTemplateConfiguration_PdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Footer_Columns",
                column: "PdfHeaderFooterConfigPdfTemplateConfigurationId",
                principalTable: "PdfTemplateConfiguration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Footer_Items_Footer_Rows_PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupI~",
                table: "Footer_Items",
                columns: new[] { "PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId", "PdfRowGroupPdfColumnGroupId", "PdfRowGroupId" },
                principalTable: "Footer_Rows",
                principalColumns: new[] { "PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId", "PdfColumnGroupId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Footer_Rows_Footer_Columns_PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId_PdfColumnGroupId",
                table: "Footer_Rows",
                columns: new[] { "PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId", "PdfColumnGroupId" },
                principalTable: "Footer_Columns",
                principalColumns: new[] { "PdfHeaderFooterConfigPdfTemplateConfigurationId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Header_Columns_PdfTemplateConfiguration_PdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Header_Columns",
                column: "PdfHeaderFooterConfigPdfTemplateConfigurationId",
                principalTable: "PdfTemplateConfiguration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Header_Items_Header_Rows_PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupI~",
                table: "Header_Items",
                columns: new[] { "PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId", "PdfRowGroupPdfColumnGroupId", "PdfRowGroupId" },
                principalTable: "Header_Rows",
                principalColumns: new[] { "PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId", "PdfColumnGroupId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Header_Rows_Header_Columns_PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId_PdfColumnGroupId",
                table: "Header_Rows",
                columns: new[] { "PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId", "PdfColumnGroupId" },
                principalTable: "Header_Columns",
                principalColumns: new[] { "PdfHeaderFooterConfigPdfTemplateConfigurationId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Footer_Columns_PdfTemplateConfiguration_PdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Footer_Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Footer_Items_Footer_Rows_PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupI~",
                table: "Footer_Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Footer_Rows_Footer_Columns_PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId_PdfColumnGroupId",
                table: "Footer_Rows");

            migrationBuilder.DropForeignKey(
                name: "FK_Header_Columns_PdfTemplateConfiguration_PdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Header_Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Header_Items_Header_Rows_PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupI~",
                table: "Header_Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Header_Rows_Header_Columns_PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId_PdfColumnGroupId",
                table: "Header_Rows");

            migrationBuilder.RenameColumn(
                name: "PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Header_Rows",
                newName: "PdfColumnGroupPdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Header_Items",
                newName: "PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Header_Columns",
                newName: "PdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Footer_Rows",
                newName: "PdfColumnGroupPdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfRowGroupPdfColumnGroupPdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Footer_Items",
                newName: "PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId");

            migrationBuilder.RenameColumn(
                name: "PdfHeaderFooterConfigPdfTemplateConfigurationId",
                table: "Footer_Columns",
                newName: "PdfTemplateConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Footer_Columns_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                table: "Footer_Columns",
                column: "PdfTemplateConfigurationId",
                principalTable: "PdfTemplateConfiguration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Footer_Items_Footer_Rows_PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupId_PdfRowGroupId",
                table: "Footer_Items",
                columns: new[] { "PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId", "PdfRowGroupPdfColumnGroupId", "PdfRowGroupId" },
                principalTable: "Footer_Rows",
                principalColumns: new[] { "PdfColumnGroupPdfTemplateConfigurationId", "PdfColumnGroupId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Footer_Rows_Footer_Columns_PdfColumnGroupPdfTemplateConfigurationId_PdfColumnGroupId",
                table: "Footer_Rows",
                columns: new[] { "PdfColumnGroupPdfTemplateConfigurationId", "PdfColumnGroupId" },
                principalTable: "Footer_Columns",
                principalColumns: new[] { "PdfTemplateConfigurationId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Header_Columns_PdfTemplateConfiguration_PdfTemplateConfigurationId",
                table: "Header_Columns",
                column: "PdfTemplateConfigurationId",
                principalTable: "PdfTemplateConfiguration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Header_Items_Header_Rows_PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId_PdfRowGroupPdfColumnGroupId_PdfRowGroupId",
                table: "Header_Items",
                columns: new[] { "PdfRowGroupPdfColumnGroupPdfTemplateConfigurationId", "PdfRowGroupPdfColumnGroupId", "PdfRowGroupId" },
                principalTable: "Header_Rows",
                principalColumns: new[] { "PdfColumnGroupPdfTemplateConfigurationId", "PdfColumnGroupId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Header_Rows_Header_Columns_PdfColumnGroupPdfTemplateConfigurationId_PdfColumnGroupId",
                table: "Header_Rows",
                columns: new[] { "PdfColumnGroupPdfTemplateConfigurationId", "PdfColumnGroupId" },
                principalTable: "Header_Columns",
                principalColumns: new[] { "PdfTemplateConfigurationId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
