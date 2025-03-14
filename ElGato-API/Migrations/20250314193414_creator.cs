using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElGato_API.Migrations
{
    /// <inheritdoc />
    public partial class creator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Challanges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Creators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pfp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creators", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Challanges_CreatorId",
                table: "Challanges",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Challanges_Creators_CreatorId",
                table: "Challanges",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challanges_Creators_CreatorId",
                table: "Challanges");

            migrationBuilder.DropTable(
                name: "Creators");

            migrationBuilder.DropIndex(
                name: "IX_Challanges_CreatorId",
                table: "Challanges");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Challanges");
        }
    }
}
