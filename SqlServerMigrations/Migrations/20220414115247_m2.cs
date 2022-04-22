using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServerMigrations.Migrations
{
    public partial class m2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectExchanges",
                table: "ObjectExchanges");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectExchanges",
                table: "ObjectExchanges",
                columns: new[] { "Id", "SenderId", "DestinationId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectExchanges",
                table: "ObjectExchanges");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectExchanges",
                table: "ObjectExchanges",
                column: "Id");
        }
    }
}
