using Microsoft.EntityFrameworkCore;

namespace IdeaForge.Infrastructure.Persistence;

public sealed class IdeaForgeDbContext : DbContext
{
    public IdeaForgeDbContext(DbContextOptions<IdeaForgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();

    public DbSet<PersonEntity> Persons => Set<PersonEntity>();

    public DbSet<AgentDataEntity> AgentData => Set<AgentDataEntity>();

    public DbSet<AgentExecutionEntity> AgentExecutions => Set<AgentExecutionEntity>();

    public DbSet<ClientCompanyEntity> ClientCompanies => Set<ClientCompanyEntity>();

    public DbSet<ClientProjectEntity> ClientProjects => Set<ClientProjectEntity>();

    public DbSet<ProjectRepositoryEntity> ProjectRepositories => Set<ProjectRepositoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurePersons(modelBuilder);
        ConfigureEmployees(modelBuilder);
        ConfigureAgentData(modelBuilder);
        ConfigureAgentExecutions(modelBuilder);
        ConfigureClientCompanies(modelBuilder);
        ConfigureClientProjects(modelBuilder);
        ConfigureProjectRepositories(modelBuilder);
    }

    private static void ConfigurePersons(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PersonEntity>();
        entity.ToTable("persons", "public");
        entity.HasKey(person => person.Id);
        entity.HasIndex(person => person.Kind).HasDatabaseName("ix_persons_kind");

        entity.Property(person => person.Id).HasColumnName("id");
        entity.Property(person => person.Kind).HasColumnName("kind").HasDefaultValue((short)1);
        entity.Property(person => person.DisplayName).HasColumnName("display_name").IsRequired();
        entity.Property(person => person.Description).HasColumnName("description");
        entity.Property(person => person.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb").IsRequired();
        entity.Property(person => person.CreatedAtUtc).HasColumnName("created_at").HasDefaultValueSql("now()");
        entity.Property(person => person.UpdatedAtUtc).HasColumnName("updated_at").HasDefaultValueSql("now()");
    }

    private static void ConfigureEmployees(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<EmployeeEntity>();
        entity.ToTable("employees", "public");
        entity.HasKey(employee => employee.Id);
        entity.HasIndex(employee => employee.Key).IsUnique().HasDatabaseName("uq_employees_key");
        entity.HasIndex(employee => employee.PersonId).HasDatabaseName("ix_employees_person_id");
        entity.HasIndex(employee => employee.Status).HasDatabaseName("ix_employees_status");
        entity.HasOne<PersonEntity>().WithMany().HasForeignKey(employee => employee.PersonId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_employees_persons_person_id");

        entity.Property(employee => employee.Id).HasColumnName("id");
        entity.Property(employee => employee.PersonId).HasColumnName("person_id");
        entity.Property(employee => employee.Key).HasColumnName("key").IsRequired();
        entity.Property(employee => employee.Role).HasColumnName("role");
        entity.Property(employee => employee.Mission).HasColumnName("mission");
        entity.Property(employee => employee.Status).HasColumnName("status").HasDefaultValue((short)1);
        entity.Property(employee => employee.SpecialtiesJson).HasColumnName("specialties").HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb").IsRequired();
        entity.Property(employee => employee.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb").IsRequired();
        entity.Property(employee => employee.CreatedAtUtc).HasColumnName("created_at").HasDefaultValueSql("now()");
        entity.Property(employee => employee.UpdatedAtUtc).HasColumnName("updated_at").HasDefaultValueSql("now()");
    }

    private static void ConfigureAgentData(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AgentDataEntity>();
        entity.ToTable("agent_data", "public");
        entity.HasKey(agentData => agentData.PersonId);
        entity.HasIndex(agentData => agentData.Provider).HasDatabaseName("ix_agent_data_provider");
        entity.HasIndex(agentData => agentData.SessionId).HasDatabaseName("ix_agent_data_session_id");
        entity.HasOne<PersonEntity>().WithOne().HasForeignKey<AgentDataEntity>(agentData => agentData.PersonId).OnDelete(DeleteBehavior.Cascade);

        entity.Property(agentData => agentData.PersonId).HasColumnName("person_id");
        entity.Property(agentData => agentData.Provider).HasColumnName("provider");
        entity.Property(agentData => agentData.Model).HasColumnName("model").HasDefaultValue("gpt-5.4").IsRequired();
        entity.Property(agentData => agentData.SessionId).HasColumnName("session_id");
        entity.Property(agentData => agentData.SystemPrompt).HasColumnName("system_prompt");
        entity.Property(agentData => agentData.LastUsedAtUtc).HasColumnName("last_used_at");
        entity.Property(agentData => agentData.SessionUpdatedAtUtc).HasColumnName("session_updated_at");
    }

    private static void ConfigureAgentExecutions(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AgentExecutionEntity>();
        entity.ToTable("agent_executions", "public");
        entity.HasKey(execution => execution.Id);
        entity.HasIndex(execution => execution.EmployeeId).HasDatabaseName("ix_agent_executions_employee_id");
        entity.HasIndex(execution => execution.SessionId).HasDatabaseName("ix_agent_executions_session_id");
        entity.HasOne<EmployeeEntity>().WithMany().HasForeignKey(execution => execution.EmployeeId).OnDelete(DeleteBehavior.Cascade);

        entity.Property(execution => execution.Id).HasColumnName("id");
        entity.Property(execution => execution.EmployeeId).HasColumnName("employee_id");
        entity.Property(execution => execution.Provider).HasColumnName("provider");
        entity.Property(execution => execution.Model).HasColumnName("model").HasDefaultValue("gpt-5.4").IsRequired();
        entity.Property(execution => execution.SessionId).HasColumnName("session_id");
        entity.Property(execution => execution.Prompt).HasColumnName("prompt").IsRequired();
        entity.Property(execution => execution.OutputSummary).HasColumnName("output_summary");
        entity.Property(execution => execution.Success).HasColumnName("success");
        entity.Property(execution => execution.ErrorMessage).HasColumnName("error_message");
        entity.Property(execution => execution.StartedAtUtc).HasColumnName("started_at");
        entity.Property(execution => execution.CompletedAtUtc).HasColumnName("completed_at");
    }

    private static void ConfigureClientCompanies(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ClientCompanyEntity>();
        entity.ToTable("client_company", "public");
        entity.HasKey(company => company.Id);
        entity.HasIndex(company => company.Key).IsUnique().HasDatabaseName("uq_client_company_key");
        entity.HasIndex(company => company.Status).HasDatabaseName("ix_client_company_status");

        entity.Property(company => company.Id).HasColumnName("id");
        entity.Property(company => company.Key).HasColumnName("key").IsRequired();
        entity.Property(company => company.Name).HasColumnName("name").IsRequired();
        entity.Property(company => company.Description).HasColumnName("description");
        entity.Property(company => company.Status).HasColumnName("status").HasDefaultValue((short)1);
        entity.Property(company => company.CreatedAtUtc).HasColumnName("created_at").HasDefaultValueSql("now()");
        entity.Property(company => company.UpdatedAtUtc).HasColumnName("updated_at").HasDefaultValueSql("now()");
    }

    private static void ConfigureClientProjects(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ClientProjectEntity>();
        entity.ToTable("client_project", "public");
        entity.HasKey(project => project.Id);
        entity.HasIndex(project => new { project.ClientCompanyId, project.Key }).IsUnique().HasDatabaseName("uq_client_project_company_key");
        entity.HasIndex(project => project.ClientCompanyId).HasDatabaseName("ix_client_project_client_company_id");
        entity.HasIndex(project => project.Status).HasDatabaseName("ix_client_project_status");
        entity.HasOne<ClientCompanyEntity>().WithMany().HasForeignKey(project => project.ClientCompanyId).OnDelete(DeleteBehavior.Cascade);

        entity.Property(project => project.Id).HasColumnName("id");
        entity.Property(project => project.ClientCompanyId).HasColumnName("client_company_id");
        entity.Property(project => project.Key).HasColumnName("key").IsRequired();
        entity.Property(project => project.Name).HasColumnName("name").IsRequired();
        entity.Property(project => project.Description).HasColumnName("description");
        entity.Property(project => project.Status).HasColumnName("status").HasDefaultValue((short)1);
        entity.Property(project => project.CreatedAtUtc).HasColumnName("created_at").HasDefaultValueSql("now()");
        entity.Property(project => project.UpdatedAtUtc).HasColumnName("updated_at").HasDefaultValueSql("now()");
    }

    private static void ConfigureProjectRepositories(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ProjectRepositoryEntity>();
        entity.ToTable("project_repository", "public");
        entity.HasKey(repository => repository.Id);
        entity.HasIndex(repository => new { repository.ClientProjectId, repository.Key }).IsUnique().HasDatabaseName("uq_project_repository_project_key");
        entity.HasIndex(repository => repository.LocalPath).IsUnique().HasDatabaseName("uq_project_repository_local_path");
        entity.HasIndex(repository => repository.ClientProjectId).HasDatabaseName("ix_project_repository_client_project_id");
        entity.HasIndex(repository => repository.Status).HasDatabaseName("ix_project_repository_status");
        entity.HasOne<ClientProjectEntity>().WithMany().HasForeignKey(repository => repository.ClientProjectId).OnDelete(DeleteBehavior.Cascade);

        entity.Property(repository => repository.Id).HasColumnName("id");
        entity.Property(repository => repository.ClientProjectId).HasColumnName("client_project_id");
        entity.Property(repository => repository.Key).HasColumnName("key").IsRequired();
        entity.Property(repository => repository.Name).HasColumnName("name").IsRequired();
        entity.Property(repository => repository.RemoteRepositoryUrl).HasColumnName("remote_repository_url").IsRequired();
        entity.Property(repository => repository.LocalPath).HasColumnName("local_path").IsRequired();
        entity.Property(repository => repository.GitStrategySkillPath).HasColumnName("git_strategy_skill_path").IsRequired();
        entity.Property(repository => repository.Description).HasColumnName("description");
        entity.Property(repository => repository.Status).HasColumnName("status").HasDefaultValue((short)1);
        entity.Property(repository => repository.CreatedAtUtc).HasColumnName("created_at").HasDefaultValueSql("now()");
        entity.Property(repository => repository.UpdatedAtUtc).HasColumnName("updated_at").HasDefaultValueSql("now()");
    }
}
