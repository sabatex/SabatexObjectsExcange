using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObjectsExchange.Migrations
{
    public partial class m2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientNodeId",
                table: "QueryObjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientNodeId",
                table: "ObjectExchanges",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueryObjects_ClientNodeId",
                table: "QueryObjects",
                column: "ClientNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectExchanges_ClientNodeId",
                table: "ObjectExchanges",
                column: "ClientNodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectExchanges_ClientNodes_ClientNodeId",
                table: "ObjectExchanges",
                column: "ClientNodeId",
                principalTable: "ClientNodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QueryObjects_ClientNodes_ClientNodeId",
                table: "QueryObjects",
                column: "ClientNodeId",
                principalTable: "ClientNodes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectExchanges_ClientNodes_ClientNodeId",
                table: "ObjectExchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_QueryObjects_ClientNodes_ClientNodeId",
                table: "QueryObjects");

            migrationBuilder.DropIndex(
                name: "IX_QueryObjects_ClientNodeId",
                table: "QueryObjects");

            migrationBuilder.DropIndex(
                name: "IX_ObjectExchanges_ClientNodeId",
                table: "ObjectExchanges");

            migrationBuilder.DropColumn(
                name: "ClientNodeId",
                table: "QueryObjects");

            migrationBuilder.DropColumn(
                name: "ClientNodeId",
                table: "ObjectExchanges");
        }
    }
}
