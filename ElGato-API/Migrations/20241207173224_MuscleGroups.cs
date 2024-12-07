using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElGato_API.Migrations
{
    /// <inheritdoc />
    public partial class MuscleGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Group",
                table: "Muscles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Group",
                table: "Muscles");
        }
    }
}
