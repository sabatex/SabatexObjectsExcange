using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObjectsExchange.Migrations
{
    /// <inheritdoc />
    public partial class m2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageCounters_ClientNodes_Id",
                table: "MessageCounters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_MessageCounters_ClientNodes_Id",
                table: "MessageCounters",
                column: "Id",
                principalTable: "ClientNodes",
                principalColumn: "Id");
        }
    }
}
