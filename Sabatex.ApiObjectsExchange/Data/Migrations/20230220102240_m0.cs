using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sabatex.ObjectsExchange.Server.Data.Migrations
{
    public partial class m0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "ClientNodes",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "ClientNodes");
        }
    }
}
