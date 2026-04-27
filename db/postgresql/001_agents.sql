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

create table if not exists public.client_company
(
    id uuid primary key,
    key text not null,
    name text not null,
    description text null,
    status smallint not null default 1,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint uq_client_company_key unique (key)
);

create table if not exists public.client_project
(
    id uuid primary key,
    client_company_id uuid not null references public.client_company (id) on delete cascade,
    key text not null,
    name text not null,
    description text null,
    status smallint not null default 1,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint uq_client_project_company_key unique (client_company_id, key)
);

create table if not exists public.project_repository
(
    id uuid primary key,
    client_project_id uuid not null references public.client_project (id) on delete cascade,
    key text not null,
    name text not null,
    remote_repository_url text not null,
    local_path text not null,
    git_strategy_skill_path text not null,
    description text null,
    status smallint not null default 1,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint uq_project_repository_project_key unique (client_project_id, key),
    constraint uq_project_repository_local_path unique (local_path)
);

create index if not exists ix_employees_status on public.employees (status);
create index if not exists ix_agent_data_provider on public.agent_data (provider);
create index if not exists ix_agent_data_session_id on public.agent_data (session_id);
create index if not exists ix_agent_executions_employee_id on public.agent_executions (employee_id);
create index if not exists ix_agent_executions_session_id on public.agent_executions (session_id);
create index if not exists ix_client_company_status on public.client_company (status);
create index if not exists ix_client_project_client_company_id on public.client_project (client_company_id);
create index if not exists ix_client_project_status on public.client_project (status);
create index if not exists ix_project_repository_client_project_id on public.project_repository (client_project_id);
create index if not exists ix_project_repository_status on public.project_repository (status);

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
drop trigger if exists trg_client_company_set_updated_at on public.client_company;
drop trigger if exists trg_client_project_set_updated_at on public.client_project;
drop trigger if exists trg_project_repository_set_updated_at on public.project_repository;

create trigger trg_employees_set_updated_at
before update on public.employees
for each row
execute function public.set_updated_at();

create trigger trg_client_company_set_updated_at
before update on public.client_company
for each row
execute function public.set_updated_at();

create trigger trg_client_project_set_updated_at
before update on public.client_project
for each row
execute function public.set_updated_at();

create trigger trg_project_repository_set_updated_at
before update on public.project_repository
for each row
execute function public.set_updated_at();
