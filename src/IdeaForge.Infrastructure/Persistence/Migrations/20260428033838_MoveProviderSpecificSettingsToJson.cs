using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveProviderSpecificSettingsToJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                alter table public.agent_data
                add column if not exists provider_settings jsonb not null default '{}'::jsonb;

                update public.agent_data
                set provider_settings =
                    jsonb_strip_nulls(
                        coalesce(provider_settings, '{}'::jsonb) ||
                        jsonb_build_object(
                            'codex.reasoning', codex_reasoning,
                            'codex.effort', codex_effort,
                            'codex.compute', codex_compute
                        )
                    )
                where codex_reasoning is not null
                   or codex_effort is not null
                   or codex_compute is not null;

                alter table public.agent_data drop column if exists codex_reasoning;
                alter table public.agent_data drop column if exists codex_effort;
                alter table public.agent_data drop column if exists codex_compute;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                alter table public.agent_data
                add column if not exists codex_reasoning text null;

                alter table public.agent_data
                add column if not exists codex_effort text null;

                alter table public.agent_data
                add column if not exists codex_compute text null;

                update public.agent_data
                set
                    codex_reasoning = provider_settings ->> 'codex.reasoning',
                    codex_effort = provider_settings ->> 'codex.effort',
                    codex_compute = provider_settings ->> 'codex.compute';

                alter table public.agent_data drop column if exists provider_settings;
                """);
        }
    }
}
