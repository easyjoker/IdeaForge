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

export enum ClientResourceStatus {
  Draft = 0,
  Active = 1,
  Disabled = 2,
  Archived = 3
}

export type EmployeeDto = {
  id: string;
  key: string;
  name: string;
  role?: string;
  description?: string;
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
  employee: EmployeeDto;
  agentData: AgentDataDto;
};

export type CreateAgentProfileRequest = {
  key: string;
  name: string;
  role?: string;
  description?: string;
  mission?: string;
  specialties: string[];
  metadata: Record<string, string>;
  provider: AgentProviderKind;
  model?: string;
  systemPrompt?: string;
};

export type AgentConversationResponse = {
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
