using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsignedCredit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalProcessingState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessingStep",
                table: "Proposals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Proposals",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingStep",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Proposals");
        }
    }
}
