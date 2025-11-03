using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coupon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    ProductId = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PercentageDiscount = table.Column<double>(type: "REAL", nullable: false),
                    CouponCode = table.Column<string>(type: "TEXT", nullable: true),
                    IsStackable = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxStackPercentage = table.Column<double>(type: "REAL", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Scope = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicableCategories = table.Column<string>(type: "TEXT", nullable: true),
                    CampaignType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsTiered = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAutomatic = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MinimumPurchaseAmount = table.Column<double>(type: "REAL", nullable: false),
                    MaxUsageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentUsageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscountTier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CouponId = table.Column<int>(type: "INTEGER", nullable: false),
                    MinAmount = table.Column<double>(type: "REAL", nullable: false),
                    MaxAmount = table.Column<double>(type: "REAL", nullable: true),
                    PercentageDiscount = table.Column<double>(type: "REAL", nullable: false),
                    FixedDiscount = table.Column<double>(type: "REAL", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountTier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountTier_Coupon_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Coupon",
                columns: new[] { "Id", "Amount", "ApplicableCategories", "CampaignType", "CouponCode", "CreatedAt", "CurrentUsageCount", "Description", "EndDate", "IsAutomatic", "IsStackable", "IsTiered", "MaxStackPercentage", "MaxUsageCount", "MinimumPurchaseAmount", "PercentageDiscount", "Priority", "ProductId", "ProductName", "Scope", "StartDate", "Status", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 150.0, null, 0, null, new DateTime(2025, 10, 30, 0, 0, 0, 0, DateTimeKind.Utc), 0, "IPhone X New", null, false, true, false, 30.0, 0, 0.0, 0.0, 0, "", "IPhone X", 0, null, 0, 0, new DateTime(2025, 10, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 100.0, null, 0, null, new DateTime(2025, 10, 30, 0, 0, 0, 0, DateTimeKind.Utc), 0, "Samsung 10 New", null, false, true, false, 30.0, 0, 0.0, 0.0, 0, "", "Samsung 10", 0, null, 0, 0, new DateTime(2025, 10, 30, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscountTier_CouponId",
                table: "DiscountTier",
                column: "CouponId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountTier");

            migrationBuilder.DropTable(
                name: "Coupon");
        }
    }
}
