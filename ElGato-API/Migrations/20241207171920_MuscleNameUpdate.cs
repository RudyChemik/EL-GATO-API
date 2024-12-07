using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElGato_API.Migrations
{
    /// <inheritdoc />
    public partial class MuscleNameUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalName",
                table: "Muscles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalName",
                table: "Muscles");
        }
    }
}
