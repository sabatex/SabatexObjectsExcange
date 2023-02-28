using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sabatex.ObjectsExchange.Server.Data.Migrations
{
    public partial class m2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SenderDateStamp",
                table: "ObjectExchanges",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<uint>(
                name: "Counter",
                table: "ClientNodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<bool>(
                name: "IsDemo",
                table: "ClientNodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<uint>(
                name: "MaxOperationPerMounth",
                table: "ClientNodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderDateStamp",
                table: "ObjectExchanges");

            migrationBuilder.DropColumn(
                name: "Counter",
                table: "ClientNodes");

            migrationBuilder.DropColumn(
                name: "IsDemo",
                table: "ClientNodes");

            migrationBuilder.DropColumn(
                name: "MaxOperationPerMounth",
                table: "ClientNodes");
        }
    }
}
