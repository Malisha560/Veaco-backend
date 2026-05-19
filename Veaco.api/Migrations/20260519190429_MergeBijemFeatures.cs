using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Veace.api.Migrations
{
    /// <inheritdoc />
    public partial class MergeBijemFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {



        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleParts_Vendors_VendorId",
                table: "VehicleParts");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "PurchaseItems");

          

            migrationBuilder.DropTable(
                name: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_VehicleParts_VendorId",
                table: "VehicleParts");

            
        }
    }
}
