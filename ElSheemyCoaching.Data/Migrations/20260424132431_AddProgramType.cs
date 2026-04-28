using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElSheemyCoaching.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Programs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Programs");
        }
    }
}
