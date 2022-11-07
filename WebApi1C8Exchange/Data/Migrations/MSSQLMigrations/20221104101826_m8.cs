using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiDocumentsExchange.Data.Migrations.MSSQLMigrations
{
    public partial class m8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_QueryObjects_Destination",
                table: "QueryObjects",
                column: "Destination");

            migrationBuilder.CreateIndex(
                name: "IX_QueryObjects_Sender",
                table: "QueryObjects",
                column: "Sender");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectExchanges_Destination",
                table: "ObjectExchanges",
                column: "Destination");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectExchanges_Sender",
                table: "ObjectExchanges",
                column: "Sender");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectExchanges_ClientNodes_Destination",
                table: "ObjectExchanges",
                column: "Destination",
                principalTable: "ClientNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectExchanges_ClientNodes_Sender",
                table: "ObjectExchanges",
                column: "Sender",
                principalTable: "ClientNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QueryObjects_ClientNodes_Destination",
                table: "QueryObjects",
                column: "Destination",
                principalTable: "ClientNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QueryObjects_ClientNodes_Sender",
                table: "QueryObjects",
                column: "Sender",
                principalTable: "ClientNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectExchanges_ClientNodes_Destination",
                table: "ObjectExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_ObjectExchanges_ClientNodes_Sender",
                table: "ObjectExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_QueryObjects_ClientNodes_Destination",
                table: "QueryObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_QueryObjects_ClientNodes_Sender",
                table: "QueryObjects");

            migrationBuilder.DropIndex(
                name: "IX_QueryObjects_Destination",
                table: "QueryObjects");

            migrationBuilder.DropIndex(
                name: "IX_QueryObjects_Sender",
                table: "QueryObjects");

            migrationBuilder.DropIndex(
                name: "IX_ObjectExchanges_Destination",
                table: "ObjectExchanges");

            migrationBuilder.DropIndex(
                name: "IX_ObjectExchanges_Sender",
                table: "ObjectExchanges");
        }
    }
}
