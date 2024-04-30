using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObjectsExchange.Migrations
{
    /// <inheritdoc />
    public partial class m1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueryObjects");

            migrationBuilder.DropColumn(
                name: "ObjectAsText",
                table: "ObjectExchanges");

            migrationBuilder.DropColumn(
                name: "ObjectType",
                table: "ObjectExchanges");

            migrationBuilder.RenameColumn(
                name: "ObjectId",
                table: "ObjectExchanges",
                newName: "MessageHeader");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ObjectExchanges",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "ObjectExchanges");

            migrationBuilder.RenameColumn(
                name: "MessageHeader",
                table: "ObjectExchanges",
                newName: "ObjectId");

            migrationBuilder.AddColumn<string>(
                name: "ObjectAsText",
                table: "ObjectExchanges",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ObjectType",
                table: "ObjectExchanges",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "QueryObjects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientNodeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Destination = table.Column<Guid>(type: "TEXT", nullable: false),
                    ObjectId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ObjectType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Sender = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryObjects_ClientNodes_ClientNodeId",
                        column: x => x.ClientNodeId,
                        principalTable: "ClientNodes",
                        principalColumn: "Id");
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
                name: "IX_QueryObjects_ClientNodeId",
                table: "QueryObjects",
                column: "ClientNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryObjects_Destination",
                table: "QueryObjects",
                column: "Destination");

            migrationBuilder.CreateIndex(
                name: "IX_QueryObjects_Sender",
                table: "QueryObjects",
                column: "Sender");
        }
    }
}
