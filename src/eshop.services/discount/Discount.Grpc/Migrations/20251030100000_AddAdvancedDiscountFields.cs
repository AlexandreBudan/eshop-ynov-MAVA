using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedDiscountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0); // Fixed type by default

            migrationBuilder.AddColumn<double>(
                name: "PercentageDiscount",
                table: "Coupon",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxStackPercentage",
                table: "Coupon",
                type: "REAL",
                nullable: false,
                defaultValue: 30.0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "PercentageDiscount",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "MaxStackPercentage",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Coupon");
        }
    }
}
