using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostgreSQLMigrations.Migrations
{
    public partial class m2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueryObjects_ObjectExchanges_OwnerId",
                table: "QueryObjects");

            migrationBuilder.DropIndex(
                name: "IX_QueryObjects_OwnerId",
                table: "QueryObjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectExchanges",
                table: "ObjectExchanges");

            migrationBuilder.AddColumn<int>(
                name: "OwnerDestinationId",
                table: "QueryObjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId1",
                table: "QueryObjects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "OwnerSenderId",
                table: "QueryObjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ObjectExchanges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectExchanges",
                table: "ObjectExchanges",
                columns: new[] { "Id", "SenderId", "DestinationId" });

            migrationBuilder.CreateIndex(
                name: "IX_QueryObjects_OwnerId1_OwnerSenderId_OwnerDestinationId",
                table: "QueryObjects",
                columns: new[] { "OwnerId1", "OwnerSenderId", "OwnerDestinationId" });

            migrationBuilder.AddForeignKey(
                name: "FK_QueryObjects_ObjectExchanges_OwnerId1_OwnerSenderId_OwnerDe~",
                table: "QueryObjects",
                columns: new[] { "OwnerId1", "OwnerSenderId", "OwnerDestinationId" },
                principalTable: "ObjectExchanges",
                principalColumns: new[] { "Id", "SenderId", "DestinationId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueryObjects_ObjectExchanges_OwnerId1_OwnerSenderId_OwnerDe~",
                table: "QueryObjects");

            migrationBuilder.DropIndex(
                name: "IX_QueryObjects_OwnerId1_OwnerSenderId_OwnerDestinationId",
                table: "QueryObjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectExchanges",
                table: "ObjectExchanges");

            migrationBuilder.DropColumn(
                name: "OwnerDestinationId",
                table: "QueryObjects");

            migrationBuilder.DropColumn(
                name: "OwnerId1",
                table: "QueryObjects");

            migrationBuilder.DropColumn(
                name: "OwnerSenderId",
                table: "QueryObjects");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ObjectExchanges");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectExchanges",
                table: "ObjectExchanges",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_QueryObjects_OwnerId",
                table: "QueryObjects",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_QueryObjects_ObjectExchanges_OwnerId",
                table: "QueryObjects",
                column: "OwnerId",
                principalTable: "ObjectExchanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
