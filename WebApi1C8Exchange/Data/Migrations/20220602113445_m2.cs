using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiDocumentsExchange.Data.Migrations
{
    public partial class m2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ObjectExchanges");

            migrationBuilder.AddColumn<bool>(
                name: "IsDone",
                table: "ObjectExchanges",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDone",
                table: "ObjectExchanges");

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "ObjectExchanges",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
