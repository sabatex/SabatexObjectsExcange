using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObjectsExchange.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutenficatedNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutenficatedNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ClientAccess = table.Column<string>(type: "TEXT", nullable: true),
                    IsDemo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Counter = table.Column<uint>(type: "INTEGER", nullable: false),
                    MaxOperationPerMounth = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectExchanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Sender = table.Column<Guid>(type: "TEXT", nullable: false),
                    Destination = table.Column<Guid>(type: "TEXT", nullable: false),
                    ObjectId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ObjectType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DateStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SenderDateStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ObjectAsText = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectExchanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjectExchanges_ClientNodes_Destination",
                        column: x => x.Destination,
                        principalTable: "ClientNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObjectExchanges_ClientNodes_Sender",
                        column: x => x.Sender,
                        principalTable: "ClientNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueryObjects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Sender = table.Column<Guid>(type: "TEXT", nullable: false),
                    Destination = table.Column<Guid>(type: "TEXT", nullable: false),
                    ObjectId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ObjectType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryObjects_ClientNodes_Destination",
                        column: x => x.Destination,
                        principalTable: "ClientNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueryObjects_ClientNodes_Sender",
                        column: x => x.Sender,
                        principalTable: "ClientNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObjectExchanges_Destination",
                table: "ObjectExchanges",
                column: "Destination");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectExchanges_Sender",
                table: "ObjectExchanges",
                column: "Sender");

            migrationBuilder.CreateIndex(
                name: "IX_QueryObjects_Destination",
                table: "QueryObjects",
                column: "Destination");

            migrationBuilder.CreateIndex(
                name: "IX_QueryObjects_Sender",
                table: "QueryObjects",
                column: "Sender");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutenficatedNodes");

            migrationBuilder.DropTable(
                name: "ObjectExchanges");

            migrationBuilder.DropTable(
                name: "QueryObjects");

            migrationBuilder.DropTable(
                name: "ClientNodes");
        }
    }
}
