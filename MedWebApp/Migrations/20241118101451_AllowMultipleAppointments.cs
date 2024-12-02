using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointment_CustomerId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_ProviderId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_ServiceId",
                table: "Appointment");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_CustomerId",
                table: "Appointment",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ProviderId",
                table: "Appointment",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ServiceId",
                table: "Appointment",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointment_CustomerId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_ProviderId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_ServiceId",
                table: "Appointment");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_CustomerId",
                table: "Appointment",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ProviderId",
                table: "Appointment",
                column: "ProviderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ServiceId",
                table: "Appointment",
                column: "ServiceId",
                unique: true);
        }
    }
}
