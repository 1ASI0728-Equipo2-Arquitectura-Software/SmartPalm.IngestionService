using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartPalm.IngestionService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ingestion");

            migrationBuilder.CreateTable(
                name: "agronomic_thresholds",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gateway_mac_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    iot_device_mac_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    min = table.Column<double>(type: "double precision", nullable: false),
                    max = table.Column<double>(type: "double precision", nullable: false),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agronomic_thresholds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingestion_batches",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    gateway_mac_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    synchronized_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reading_count = table.Column<int>(type: "integer", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingestion_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sector_device_projections",
                schema: "ingestion",
                columns: table => new
                {
                    sector_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iot_device_mac_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sector_device_projections", x => x.sector_id);
                });

            migrationBuilder.CreateTable(
                name: "sensor_readings",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gateway_mac_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    iot_device_mac_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    measured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sensor_readings", x => x.id);
                    table.ForeignKey(
                        name: "fk_sensor_readings_ingestion_batches_batch_id",
                        column: x => x.batch_id,
                        principalSchema: "ingestion",
                        principalTable: "ingestion_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agronomic_thresholds_gateway_mac_address",
                schema: "ingestion",
                table: "agronomic_thresholds",
                column: "gateway_mac_address");

            migrationBuilder.CreateIndex(
                name: "ix_agronomic_thresholds_iot_device_mac_address_type",
                schema: "ingestion",
                table: "agronomic_thresholds",
                columns: new[] { "iot_device_mac_address", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ingestion_batches_fingerprint",
                schema: "ingestion",
                table: "ingestion_batches",
                column: "fingerprint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_utc_occurred_on_utc",
                schema: "ingestion",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "occurred_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_sector_device_projections_iot_device_mac_address",
                schema: "ingestion",
                table: "sector_device_projections",
                column: "iot_device_mac_address");

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_batch_id",
                schema: "ingestion",
                table: "sensor_readings",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_gateway_mac_address_measured_at",
                schema: "ingestion",
                table: "sensor_readings",
                columns: new[] { "gateway_mac_address", "measured_at" });

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_iot_device_mac_address_measured_at",
                schema: "ingestion",
                table: "sensor_readings",
                columns: new[] { "iot_device_mac_address", "measured_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agronomic_thresholds",
                schema: "ingestion");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "ingestion");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "ingestion");

            migrationBuilder.DropTable(
                name: "sector_device_projections",
                schema: "ingestion");

            migrationBuilder.DropTable(
                name: "sensor_readings",
                schema: "ingestion");

            migrationBuilder.DropTable(
                name: "ingestion_batches",
                schema: "ingestion");
        }
    }
}
