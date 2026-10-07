using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsignedCredit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxProcessedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt",
                table: "OutboxMessages",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedAt",
                table: "OutboxMessages");
        }
    }
}
