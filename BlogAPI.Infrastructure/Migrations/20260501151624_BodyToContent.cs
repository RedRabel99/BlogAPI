using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BodyToContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Body",
                table: "Posts",
                newName: "Content");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Posts",
                newName: "Body");
        }
    }
}
