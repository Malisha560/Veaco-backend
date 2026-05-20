using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veace.api.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditUpdatedAtToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreditUpdatedAt",
                table: "Customers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditUpdatedAt",
                table: "Customers");
        }
    }
}
