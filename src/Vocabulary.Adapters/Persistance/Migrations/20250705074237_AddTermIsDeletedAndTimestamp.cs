using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vocabulary.Adapters.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddTermIsDeletedAndTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Timestamp",
                schema: "dbo",
                table: "Term",
                type: "INT",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INT",
                oldDefaultValue: 0ul);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "Term",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "BOOLEAN",
                oldDefaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "Timestamp",
                schema: "dbo",
                table: "Term",
                type: "INT",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(long),
                oldType: "INT");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "Term",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");
        }
    }
}
