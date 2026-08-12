using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookRec.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleBookIdToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleBookId",
                table: "Books",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleBookId",
                table: "Books");
        }
    }
}
