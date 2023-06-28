using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObjectsExchange.Migrations
{
    public partial class m1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
 
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Dascription = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "Dascription" },
                values: new object[] { 1, "Demo" });
           
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "ClientNodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_ClientNodes_ClientId",
                table: "ClientNodes",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientNodes_Clients_ClientId",
                table: "ClientNodes",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientNodes_Clients_ClientId",
                table: "ClientNodes");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_ClientNodes_ClientId",
                table: "ClientNodes");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ClientNodes");
        }
    }
}
