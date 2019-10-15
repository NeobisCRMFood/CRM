using Microsoft.EntityFrameworkCore.Migrations;

namespace DataTier.Migrations
{
    public partial class TotalPriceNotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Orders",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Orders",
                nullable: true,
                oldClrType: typeof(decimal));
        }
    }
}
