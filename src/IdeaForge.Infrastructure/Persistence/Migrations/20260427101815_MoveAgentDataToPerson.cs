using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveAgentDataToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                alter table public.agent_data
                add column if not exists person_id uuid null;

                update public.agent_data a
                set person_id = e.person_id
                from public.employees e
                where a.person_id is null
                  and a.employee_id = e.id;

                delete from public.agent_data
                where person_id is null;

                delete from public.agent_data a
                using public.agent_data b
                where a.ctid < b.ctid
                  and a.person_id = b.person_id;

                alter table public.agent_data
                drop constraint if exists "FK_agent_data_employees_employee_id";

                alter table public.agent_data
                drop constraint if exists agent_data_employee_id_fkey;

                alter table public.agent_data
                drop constraint if exists agent_data_pkey;

                alter table public.agent_data
                alter column person_id set not null;

                alter table public.agent_data
                drop column if exists employee_id;

                do $$
                begin
                    if not exists
                    (
                        select 1
                        from pg_constraint
                        where conname = 'agent_data_pkey'
                    )
                    then
                        alter table public.agent_data
                        add constraint agent_data_pkey primary key (person_id);
                    end if;

                    if not exists
                    (
                        select 1
                        from pg_constraint
                        where conname = 'fk_agent_data_persons_person_id'
                    )
                    then
                        alter table public.agent_data
                        add constraint fk_agent_data_persons_person_id
                        foreign key (person_id) references public.persons (id) on delete cascade;
                    end if;
                end
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                alter table public.agent_data
                add column if not exists employee_id uuid null;

                update public.agent_data a
                set employee_id = e.id
                from public.employees e
                where a.employee_id is null
                  and a.person_id = e.person_id;

                delete from public.agent_data
                where employee_id is null;

                delete from public.agent_data a
                using public.agent_data b
                where a.ctid < b.ctid
                  and a.employee_id = b.employee_id;

                alter table public.agent_data
                drop constraint if exists fk_agent_data_persons_person_id;

                alter table public.agent_data
                drop constraint if exists "FK_agent_data_persons_person_id";

                alter table public.agent_data
                drop constraint if exists agent_data_pkey;

                alter table public.agent_data
                alter column employee_id set not null;

                alter table public.agent_data
                drop column if exists person_id;

                do $$
                begin
                    if not exists
                    (
                        select 1
                        from pg_constraint
                        where conname = 'agent_data_pkey'
                    )
                    then
                        alter table public.agent_data
                        add constraint agent_data_pkey primary key (employee_id);
                    end if;

                    if not exists
                    (
                        select 1
                        from pg_constraint
                        where conname = 'agent_data_employee_id_fkey'
                    )
                    then
                        alter table public.agent_data
                        add constraint agent_data_employee_id_fkey
                        foreign key (employee_id) references public.employees (id) on delete cascade;
                    end if;
                end
                $$;
                """);
        }
    }
}
