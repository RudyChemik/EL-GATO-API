using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElGato_API.Migrations
{
    /// <inheritdoc />
    public partial class achievments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnlockedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Threshold = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AchievmentCounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Counter = table.Column<int>(type: "int", nullable: false),
                    AchievmentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievmentCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchievmentCounters_Achievment_AchievmentId",
                        column: x => x.AchievmentId,
                        principalTable: "Achievment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AchievmentCounters_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    AchievmentsId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => new { x.AchievmentsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievment_AchievmentsId",
                        column: x => x.AchievmentsId,
                        principalTable: "Achievment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievmentCounters_AchievmentId",
                table: "AchievmentCounters",
                column: "AchievmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievmentCounters_UserId",
                table: "AchievmentCounters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UsersId",
                table: "UserAchievements",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievmentCounters");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "Achievment");
        }
    }
}
