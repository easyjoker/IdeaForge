using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodexCliTuningSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codex_compute",
                schema: "public",
                table: "agent_data",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codex_effort",
                schema: "public",
                table: "agent_data",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codex_reasoning",
                schema: "public",
                table: "agent_data",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codex_compute",
                schema: "public",
                table: "agent_data");

            migrationBuilder.DropColumn(
                name: "codex_effort",
                schema: "public",
                table: "agent_data");

            migrationBuilder.DropColumn(
                name: "codex_reasoning",
                schema: "public",
                table: "agent_data");
        }
    }
}
