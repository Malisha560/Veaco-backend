using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veace.api.Migrations
{
    /// <inheritdoc />
    public partial class FewUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_Parts_PartId",
                table: "PurchaseItems");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Parts",
                newName: "PartName");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "VehicleParts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "VehicleParts",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Parts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "Parts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleParts_VendorId",
                table: "VehicleParts",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_VendorId",
                table: "Parts",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Vendors_VendorId",
                table: "Parts",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_VehicleParts_PartId",
                table: "PurchaseItems",
                column: "PartId",
                principalTable: "VehicleParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleParts_Vendors_VendorId",
                table: "VehicleParts",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Vendors_VendorId",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_VehicleParts_PartId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleParts_Vendors_VendorId",
                table: "VehicleParts");

            migrationBuilder.DropIndex(
                name: "IX_VehicleParts_VendorId",
                table: "VehicleParts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_VendorId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "VehicleParts");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "VehicleParts");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "Parts");

            migrationBuilder.RenameColumn(
                name: "PartName",
                table: "Parts",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Parts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_Parts_PartId",
                table: "PurchaseItems",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
