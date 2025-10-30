using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class AddContextualRulesAndTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add contextual rules fields to Coupon table
            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0); // Product scope

            migrationBuilder.AddColumn<string>(
                name: "ApplicableCategories",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CampaignType",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0); // None

            migrationBuilder.AddColumn<bool>(
                name: "IsTiered",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutomatic",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Create DiscountTier table
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

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "ApplicableCategories",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "CampaignType",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "IsTiered",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "IsAutomatic",
                table: "Coupon");
        }
    }
}
