using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vocabulary.BlazorServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Term",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequence = table.Column<int>(type: "INT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    AdditionalName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ValidationRules = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Term", x => x.Id);
                    table.UniqueConstraint("AK_Term_Sequence", x => x.Sequence);
                });

            migrationBuilder.CreateTable(
                name: "Link",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Href = table.Column<string>(type: "TEXT", maxLength: 2047, nullable: false),
                    ResourceDescription = table.Column<string>(type: "TEXT", nullable: true),
                    TermId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Link", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Link_Term_TermId",
                        column: x => x.TermId,
                        principalSchema: "dbo",
                        principalTable: "Term",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Synonym",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TermId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Synonym", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Synonym_Term_TermId",
                        column: x => x.TermId,
                        principalSchema: "dbo",
                        principalTable: "Term",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TermCategory",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TermId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermCategory", x => new { x.CategoryId, x.TermId });
                    table.ForeignKey(
                        name: "FK_TermCategory_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TermLink_Terms_TermId",
                        column: x => x.TermId,
                        principalSchema: "dbo",
                        principalTable: "Term",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Category",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("a3667d54-1ba8-4c80-a593-2873a810a990"), "General" });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Term",
                columns: new[] { "Id", "AdditionalName", "Description", "Name", "Sequence", "ValidationRules" },
                values: new object[] { new Guid("2aded0e1-92d3-493f-abb4-d2381662cfda"), null, "Процесс преобразования Токена в PAN.", "Детокенизация", 1, null });

            migrationBuilder.InsertData(
                table: "TermCategory",
                columns: new[] { "CategoryId", "TermId" },
                values: new object[] { new Guid("a3667d54-1ba8-4c80-a593-2873a810a990"), new Guid("2aded0e1-92d3-493f-abb4-d2381662cfda") });

            migrationBuilder.CreateIndex(
                name: "IX_Link_TermId",
                schema: "dbo",
                table: "Link",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_Synonym_TermId",
                schema: "dbo",
                table: "Synonym",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_TermCategory_TermId",
                table: "TermCategory",
                column: "TermId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Link",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Synonym",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TermCategory");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Term",
                schema: "dbo");
        }
    }
}
