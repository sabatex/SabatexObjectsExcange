using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServerMigrations.Migrations
{
    public partial class m1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "QueryObjects");

            migrationBuilder.RenameColumn(
                name: "QueryJson",
                table: "QueryObjects",
                newName: "ObjectType");

            migrationBuilder.AddColumn<bool>(
                name: "IsResived",
                table: "QueryObjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ObjectId",
                table: "QueryObjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResived",
                table: "QueryObjects");

            migrationBuilder.DropColumn(
                name: "ObjectId",
                table: "QueryObjects");

            migrationBuilder.RenameColumn(
                name: "ObjectType",
                table: "QueryObjects",
                newName: "QueryJson");

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "QueryObjects",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
