using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElGato_API.Migrations
{
    /// <inheritdoc />
    public partial class GoalUserInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BodyType",
                table: "UserInformation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Goal",
                table: "UserInformation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Metric",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyType",
                table: "UserInformation");

            migrationBuilder.DropColumn(
                name: "Goal",
                table: "UserInformation");

            migrationBuilder.DropColumn(
                name: "Metric",
                table: "AspNetUsers");
        }
    }
}
