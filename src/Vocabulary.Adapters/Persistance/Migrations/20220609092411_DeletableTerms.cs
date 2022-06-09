using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vocabulary.BlazorServer.Migrations
{
    public partial class DeletableTerms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "Term",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<ulong>(
                name: "Timestamp",
                schema: "dbo",
                table: "Term",
                type: "INT",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "Term");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                schema: "dbo",
                table: "Term");
        }
    }
}
