using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameToProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Provider",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Provider");
        }
    }
}
