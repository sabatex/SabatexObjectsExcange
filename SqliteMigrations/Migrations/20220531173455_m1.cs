using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqliteMigrations.Migrations
{
    public partial class m1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "QueryObjects",
                newName: "IsResived");

            migrationBuilder.RenameColumn(
                name: "QueryJson",
                table: "QueryObjects",
                newName: "ObjectType");

            migrationBuilder.AddColumn<Guid>(
                name: "ObjectId",
                table: "QueryObjects",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectId",
                table: "QueryObjects");

            migrationBuilder.RenameColumn(
                name: "ObjectType",
                table: "QueryObjects",
                newName: "QueryJson");

            migrationBuilder.RenameColumn(
                name: "IsResived",
                table: "QueryObjects",
                newName: "Status");
        }
    }
}
