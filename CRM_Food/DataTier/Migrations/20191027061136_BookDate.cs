using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataTier.Migrations
{
    public partial class BookDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BookDate",
                table: "Tables",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookDate",
                table: "Tables");
        }
    }
}
