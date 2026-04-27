using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitPersonEmployeeAgentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                create table if not exists public.persons
                (
                    id uuid primary key,
                    kind smallint not null default 1,
                    display_name text not null,
                    description text null,
                    metadata jsonb not null default '{}'::jsonb,
                    created_at timestamptz not null default now(),
                    updated_at timestamptz not null default now()
                );

                alter table public.employees
                add column if not exists person_id uuid null;

                with source as
                (
                    select
                        e.id as employee_id,
                        e.id as person_id,
                        e.name as display_name,
                        e.description,
                        e.created_at,
                        e.updated_at
                    from public.employees e
                    where e.person_id is null
                ),
                inserted as
                (
                    insert into public.persons
                    (
                        id, kind, display_name, description, metadata, created_at, updated_at
                    )
                    select
                        source.person_id,
                        2,
                        source.display_name,
                        source.description,
                        '{}'::jsonb,
                        source.created_at,
                        source.updated_at
                    from source
                    on conflict (id) do nothing
                    returning id
                )
                update public.employees e
                set person_id = source.person_id
                from source
                where e.id = source.employee_id;

                alter table public.employees
                alter column person_id set not null;

                create index if not exists ix_persons_kind on public.persons (kind);
                create index if not exists ix_employees_person_id on public.employees (person_id);

                do $$
                begin
                    if not exists
                    (
                        select 1
                        from pg_constraint
                        where conname = 'fk_employees_persons_person_id'
                    )
                    then
                        alter table public.employees
                        add constraint fk_employees_persons_person_id
                        foreign key (person_id) references public.persons (id) on delete restrict;
                    end if;
                end
                $$;

                drop trigger if exists trg_persons_set_updated_at on public.persons;
                create trigger trg_persons_set_updated_at
                before update on public.persons
                for each row
                execute function public.set_updated_at();

                alter table public.employees drop column if exists description;
                alter table public.employees drop column if exists name;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                alter table public.employees
                add column if not exists name text not null default '';

                alter table public.employees
                add column if not exists description text null;

                update public.employees e
                set
                    name = p.display_name,
                    description = p.description
                from public.persons p
                where e.person_id = p.id;

                alter table public.employees
                drop constraint if exists fk_employees_persons_person_id;

                drop index if exists public.ix_employees_person_id;
                drop trigger if exists trg_persons_set_updated_at on public.persons;

                alter table public.employees
                drop column if exists person_id;

                drop table if exists public.persons;
                """);
        }
    }
}
