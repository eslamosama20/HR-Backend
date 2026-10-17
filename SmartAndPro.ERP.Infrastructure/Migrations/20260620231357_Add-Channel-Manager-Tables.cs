using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelManagerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelDeadLetterQueue");

            migrationBuilder.DropTable(
                name: "ChannelOutboxEvents");

            migrationBuilder.DropTable(
                name: "ChannelRateMappings");

            migrationBuilder.DropTable(
                name: "ChannelReservationPayments");

            migrationBuilder.DropTable(
                name: "ChannelRoomMappings");

            migrationBuilder.DropTable(
                name: "ChannelSyncLogs");

            migrationBuilder.DropTable(
                name: "ChannelWebhookQueue");

            migrationBuilder.DropTable(
                name: "ChannelReservationMappings");

            migrationBuilder.DropTable(
                name: "ChannelConnections");
        }
    }
}
