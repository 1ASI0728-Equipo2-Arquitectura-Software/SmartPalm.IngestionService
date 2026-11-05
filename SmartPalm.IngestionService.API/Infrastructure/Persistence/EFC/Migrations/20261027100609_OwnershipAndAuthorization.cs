using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartPalm.IngestionService.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class OwnershipAndAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "owner_user_id",
                schema: "ingestion",
                table: "sensor_readings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "plantation_id",
                schema: "ingestion",
                table: "sensor_readings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "sector_id",
                schema: "ingestion",
                table: "sensor_readings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "account_plan_projections",
                schema: "ingestion",
                columns: table => new
                {
                    owner_user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_plan_projections", x => x.owner_user_id);
                });

            migrationBuilder.CreateTable(
                name: "authorized_device_projections",
                schema: "ingestion",
                columns: table => new
                {
                    device_mac = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gateway_mac = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner_user_id = table.Column<long>(type: "bigint", nullable: false),
                    plantation_id = table.Column<long>(type: "bigint", nullable: false),
                    sector_id = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authorized_device_projections", x => x.device_mac);
                });

            migrationBuilder.CreateTable(
                name: "gateway_authorization_projections",
                schema: "ingestion",
                columns: table => new
                {
                    gateway_mac = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner_user_id = table.Column<long>(type: "bigint", nullable: false),
                    credential_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    plan_active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gateway_authorization_projections", x => x.gateway_mac);
                });

            migrationBuilder.CreateIndex(
                name: "ix_authorized_device_projections_gateway_mac",
                schema: "ingestion",
                table: "authorized_device_projections",
                column: "gateway_mac");

            migrationBuilder.CreateIndex(
                name: "ix_authorized_device_projections_plantation_id_sector_id",
                schema: "ingestion",
                table: "authorized_device_projections",
                columns: new[] { "plantation_id", "sector_id" });

            migrationBuilder.CreateIndex(
                name: "ix_gateway_authorization_projections_owner_user_id",
                schema: "ingestion",
                table: "gateway_authorization_projections",
                column: "owner_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_plan_projections",
                schema: "ingestion");

            migrationBuilder.DropTable(
                name: "authorized_device_projections",
                schema: "ingestion");

            migrationBuilder.DropTable(
                name: "gateway_authorization_projections",
                schema: "ingestion");

            migrationBuilder.DropColumn(
                name: "owner_user_id",
                schema: "ingestion",
                table: "sensor_readings");

            migrationBuilder.DropColumn(
                name: "plantation_id",
                schema: "ingestion",
                table: "sensor_readings");

            migrationBuilder.DropColumn(
                name: "sector_id",
                schema: "ingestion",
                table: "sensor_readings");
        }
    }
}
