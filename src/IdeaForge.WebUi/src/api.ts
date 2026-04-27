import type {
  AgentConversationResponse,
  AgentProfileDto,
  CreateAgentProfileRequest,
  CreateClientCompanyRequest,
  CreateClientProjectRequest,
  CreateProjectRepositoryRequest,
  ClientCompanyDto,
  ClientProjectDto,
  ProjectRepositoryDto,
  UpdateClientCompanyRequest,
  UpdateClientProjectRequest,
  UpdateProjectRepositoryRequest
} from "./types";

const apiBaseStorageKey = "ideaforge.apiBaseUrl";
const defaultApiBaseUrl = import.meta.env.VITE_IDEAFORGE_API_BASE_URL ?? "/api";

export function getApiBaseUrl() {
  return localStorage.getItem(apiBaseStorageKey) ?? defaultApiBaseUrl;
}

export function setApiBaseUrl(value: string) {
  const normalized = normalizeBaseUrl(value);
  localStorage.setItem(apiBaseStorageKey, normalized);
  return normalized;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers
    }
  });

  if (!response.ok) {
    throw new Error(await readError(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function readError(response: Response) {
  const text = await response.text();
  if (!text) {
    return `${response.status} ${response.statusText}`;
  }

  try {
    const problem = JSON.parse(text) as { title?: string; detail?: string };
    return problem.detail ?? problem.title ?? text;
  } catch {
    return text;
  }
}

function normalizeBaseUrl(value: string) {
  const trimmed = value.trim();
  if (!trimmed) {
    return defaultApiBaseUrl;
  }

  return trimmed.endsWith("/") ? trimmed.slice(0, -1) : trimmed;
}

export const api = {
  listAgents: () => request<AgentProfileDto[]>("/agents"),
  createAgent: (body: CreateAgentProfileRequest) =>
    request<AgentProfileDto>("/agents", {
      method: "POST",
      body: JSON.stringify(body)
    }),
  chatWithAgent: (key: string, prompt: string) =>
    request<AgentConversationResponse>(`/agents/by-key/${encodeURIComponent(key)}/chat`, {
      method: "POST",
      body: JSON.stringify({ prompt })
    }),
  listCompanies: () => request<ClientCompanyDto[]>("/client-companies"),
  getCompanyByKey: (key: string) => request<ClientCompanyDto>(`/client-companies/by-key/${encodeURIComponent(key)}`),
  createCompany: (body: CreateClientCompanyRequest) =>
    request<ClientCompanyDto>("/client-companies", {
      method: "POST",
      body: JSON.stringify(body)
    }),
  updateCompany: (id: string, body: UpdateClientCompanyRequest) =>
    request<ClientCompanyDto>(`/client-companies/${id}`, {
      method: "PUT",
      body: JSON.stringify(body)
    }),
  listProjects: (companyId: string) => request<ClientProjectDto[]>(`/client-companies/${companyId}/projects`),
  createProject: (companyId: string, body: CreateClientProjectRequest) =>
    request<ClientProjectDto>(`/client-companies/${companyId}/projects`, {
      method: "POST",
      body: JSON.stringify(body)
    }),
  updateProject: (id: string, body: UpdateClientProjectRequest) =>
    request<ClientProjectDto>(`/client-projects/${id}`, {
      method: "PUT",
      body: JSON.stringify(body)
    }),
  listRepositories: (projectId: string) => request<ProjectRepositoryDto[]>(`/client-projects/${projectId}/repositories`),
  createRepository: (projectId: string, body: CreateProjectRepositoryRequest) =>
    request<ProjectRepositoryDto>(`/client-projects/${projectId}/repositories`, {
      method: "POST",
      body: JSON.stringify(body)
    }),
  updateRepository: (id: string, body: UpdateProjectRepositoryRequest) =>
    request<ProjectRepositoryDto>(`/project-repositories/${id}`, {
      method: "PUT",
      body: JSON.stringify(body)
    })
};
