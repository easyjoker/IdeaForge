export enum AgentProviderKind {
  Unknown = 0,
  Copilot = 1,
  Codex = 2
}

export enum AgentStatus {
  Draft = 0,
  Active = 1,
  Disabled = 2,
  Archived = 3
}

export enum PersonKind {
  Unknown = 0,
  Human = 1,
  Ai = 2
}

export enum ClientResourceStatus {
  Draft = 0,
  Active = 1,
  Disabled = 2,
  Archived = 3
}

export type PersonDto = {
  id: string;
  kind: PersonKind;
  displayName: string;
  description?: string;
  metadata: Record<string, string>;
  agentSettings?: AgentDataDto;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type PersonDirectoryDto = PersonDto;

export type EmployeeDto = {
  id: string;
  personId: string;
  key: string;
  role?: string;
  mission?: string;
  status: AgentStatus;
  specialties: string[];
  metadata: Record<string, string>;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type AgentDataDto = {
  provider: AgentProviderKind;
  model: string;
  sessionId?: string;
  systemPrompt?: string;
  lastUsedAtUtc?: string;
  sessionUpdatedAtUtc?: string;
};

export type AgentProfileDto = {
  person: PersonDto;
  employee: EmployeeDto;
  agentData: AgentDataDto;
};

export type AgentSettingsDto = AgentDataDto;

export type EmployeeDirectoryDto = {
  person: PersonDirectoryDto;
  id: string;
  key: string;
  role?: string;
  mission?: string;
  status: AgentStatus;
  specialties: string[];
  metadata: Record<string, string>;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type CreatePersonRequest = {
  kind: PersonKind;
  displayName: string;
  description?: string;
  metadata: Record<string, string>;
  agentSettings?: UpsertAgentSettingsRequest;
};

export type UpdatePersonRequest = CreatePersonRequest;

export type UpsertAgentSettingsRequest = {
  provider: AgentProviderKind;
  model?: string;
  systemPrompt?: string;
};

export type CreateEmployeeRequest = {
  personId: string;
  key: string;
  role?: string;
  mission?: string;
  specialties: string[];
  metadata: Record<string, string>;
};

export type UpdateEmployeeRequest = {
  key: string;
  role?: string;
  mission?: string;
  status: AgentStatus;
  specialties: string[];
  metadata: Record<string, string>;
};

export type CreateAgentProfileRequest = {
  key: string;
  displayName: string;
  personDescription?: string;
  personMetadata: Record<string, string>;
  role?: string;
  mission?: string;
  specialties: string[];
  metadata: Record<string, string>;
  provider: AgentProviderKind;
  model?: string;
  systemPrompt?: string;
};

export type AgentConversationResponse = {
  personId: string;
  personDisplayName: string;
  employeeId: string;
  employeeKey: string;
  employeeName: string;
  provider: AgentProviderKind;
  model: string;
  sessionId?: string;
  output: string;
  errorOutput?: string;
  exitCode: number;
  outputPath?: string;
};

export type ClientCompanyDto = {
  id: string;
  key: string;
  name: string;
  description?: string;
  status: ClientResourceStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type ClientProjectDto = {
  id: string;
  clientCompanyId: string;
  key: string;
  name: string;
  description?: string;
  status: ClientResourceStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type ProjectRepositoryDto = {
  id: string;
  clientProjectId: string;
  key: string;
  name: string;
  remoteRepositoryUrl: string;
  localPath: string;
  gitStrategySkillPath: string;
  description?: string;
  status: ClientResourceStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type CreateClientCompanyRequest = {
  key: string;
  name: string;
  description?: string;
};

export type UpdateClientCompanyRequest = {
  name: string;
  description?: string;
  status: ClientResourceStatus;
};

export type CreateClientProjectRequest = {
  key: string;
  name: string;
  description?: string;
};

export type UpdateClientProjectRequest = {
  name: string;
  description?: string;
  status: ClientResourceStatus;
};

export type CreateProjectRepositoryRequest = {
  key: string;
  name: string;
  remoteRepositoryUrl: string;
  localPath: string;
  gitStrategySkillPath: string;
  description?: string;
};

export type UpdateProjectRepositoryRequest = {
  name: string;
  remoteRepositoryUrl: string;
  localPath: string;
  gitStrategySkillPath: string;
  description?: string;
  status: ClientResourceStatus;
};
