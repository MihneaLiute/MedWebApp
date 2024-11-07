using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Provider",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Provider_UserId",
                table: "Provider",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_AspNetUsers_UserId",
                table: "Provider",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Provider_AspNetUsers_UserId",
                table: "Provider");

            migrationBuilder.DropIndex(
                name: "IX_Provider_UserId",
                table: "Provider");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Provider",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
