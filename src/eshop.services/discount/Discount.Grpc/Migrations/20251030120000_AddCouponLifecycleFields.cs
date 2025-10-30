using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class AddCouponLifecycleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0); // Active

            migrationBuilder.AddColumn<string>(
                name: "StartDate",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndDate",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinimumPurchaseAmount",
                table: "Coupon",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsageCount",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentUsageCount",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedAt",
                table: "Coupon",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedAt",
                table: "Coupon",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "MinimumPurchaseAmount",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "MaxUsageCount",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "CurrentUsageCount",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Coupon");
        }
    }
}
