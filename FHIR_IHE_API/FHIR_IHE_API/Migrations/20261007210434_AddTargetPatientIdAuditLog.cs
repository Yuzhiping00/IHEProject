using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FHIR_IHE_API.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetPatientIdAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetPatientId",
                table: "AuditLogs",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetPatientId",
                table: "AuditLogs");
        }
    }
}
