using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsignedCredit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStateLoanRestrictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StateLoanRestrictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateLoanRestrictions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StateLoanRestrictions_State",
                table: "StateLoanRestrictions",
                column: "State",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StateLoanRestrictions");
        }
    }
}
