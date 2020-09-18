using Microsoft.EntityFrameworkCore.Migrations;

namespace CompeTournament.Backend.Migrations
{
    public partial class PredictionRemove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Predictions_Users_ApplicationUserId",
                table: "Predictions");

            migrationBuilder.DropIndex(
                name: "IX_Predictions_ApplicationUserId",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Predictions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Predictions",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_ApplicationUserId",
                table: "Predictions",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Predictions_Users_ApplicationUserId",
                table: "Predictions",
                column: "ApplicationUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
