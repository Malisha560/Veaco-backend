using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veace.api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContactPersonFromVendor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Vendors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Vendors",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
