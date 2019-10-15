using Microsoft.EntityFrameworkCore.Migrations;

namespace DataTier.Migrations
{
    public partial class TableBusy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBusy",
                table: "Tables",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBusy",
                table: "Tables");
        }
    }
}
