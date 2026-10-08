using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FHIR_IHE_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_PatientId_TimestampUtc_Id",
                table: "AuditLogs",
                columns: new[] { "PatientId", "TimestampUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ProviderId_TimestampUtc_Id",
                table: "AuditLogs",
                columns: new[] { "ProviderId", "TimestampUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ResourceType_ResourceId",
                table: "AuditLogs",
                columns: new[] { "ResourceType", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TargetPatientId_TimestampUtc_Id",
                table: "AuditLogs",
                columns: new[] { "TargetPatientId", "TimestampUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TimestampUtc_Id",
                table: "AuditLogs",
                columns: new[] { "TimestampUtc", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_PatientId_TimestampUtc_Id",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ProviderId_TimestampUtc_Id",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ResourceType_ResourceId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TargetPatientId_TimestampUtc_Id",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TimestampUtc_Id",
                table: "AuditLogs");
        }
    }
}
