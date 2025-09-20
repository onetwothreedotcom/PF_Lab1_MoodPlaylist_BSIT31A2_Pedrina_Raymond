using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoodPlaylistGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalMediaSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocalFileName",
                table: "Songs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalFilePath",
                table: "Songs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LocalFileSizeBytes",
                table: "Songs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalFileType",
                table: "Songs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalFileName",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "LocalFilePath",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "LocalFileSizeBytes",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "LocalFileType",
                table: "Songs");
        }
    }
}
