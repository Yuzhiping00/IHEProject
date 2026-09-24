using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FHIR_IHE_API.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientProviderToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "AuditLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProviderId",
                table: "AuditLogs",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "AuditLogs");
        }
    }
}
