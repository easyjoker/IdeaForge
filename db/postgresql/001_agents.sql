create table if not exists public.employees
(
    id uuid primary key,
    key text not null,
    name text not null,
    role text null,
    description text null,
    mission text null,
    status smallint not null default 1,
    specialties jsonb not null default '[]'::jsonb,
    metadata jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint uq_employees_key unique (key)
);

create table if not exists public.agent_data
(
    employee_id uuid primary key references public.employees (id) on delete cascade,
    provider smallint not null,
    model text not null default 'gpt-5.4',
    session_id text null,
    system_prompt text null,
    last_used_at timestamptz null,
    session_updated_at timestamptz null
);

create table if not exists public.agent_executions
(
    id uuid primary key,
    employee_id uuid not null references public.employees (id) on delete cascade,
    provider smallint not null,
    model text not null default 'gpt-5.4',
    session_id text null,
    prompt text not null,
    output_summary text null,
    success boolean not null,
    error_message text null,
    started_at timestamptz not null,
    completed_at timestamptz not null
);

create index if not exists ix_employees_status on public.employees (status);
create index if not exists ix_agent_data_provider on public.agent_data (provider);
create index if not exists ix_agent_data_session_id on public.agent_data (session_id);
create index if not exists ix_agent_executions_employee_id on public.agent_executions (employee_id);
create index if not exists ix_agent_executions_session_id on public.agent_executions (session_id);

create or replace function public.set_updated_at()
returns trigger
language plpgsql
as
$$
begin
    new.updated_at = now();
    return new;
end;
$$;

drop trigger if exists trg_employees_set_updated_at on public.employees;

create trigger trg_employees_set_updated_at
before update on public.employees
for each row
execute function public.set_updated_at();
