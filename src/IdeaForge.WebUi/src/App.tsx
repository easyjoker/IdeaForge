import {
  App as AntApp,
  Badge,
  Button,
  Card,
  Col,
  ConfigProvider,
  Drawer,
  Empty,
  Form,
  Input,
  Layout,
  Modal,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tabs,
  Tag,
  Typography,
  message,
  theme
} from "antd";
import {
  ApiOutlined,
  ApartmentOutlined,
  CloudServerOutlined,
  MessageOutlined,
  PlusOutlined,
  ReloadOutlined,
  RobotOutlined,
  SaveOutlined,
  SettingOutlined
} from "@ant-design/icons";
import { useEffect, useState } from "react";
import { api, getApiBaseUrl, setApiBaseUrl } from "./api";
import {
  AgentProviderKind,
  AgentStatus,
  ClientResourceStatus,
  type AgentConversationResponse,
  type AgentProfileDto,
  type ClientCompanyDto,
  type ClientProjectDto,
  type ProjectRepositoryDto
} from "./types";

const { Header, Content } = Layout;
const { Paragraph, Text, Title } = Typography;

type AgentFormValues = {
  key: string;
  name: string;
  role?: string;
  description?: string;
  mission?: string;
  specialtiesText?: string;
  provider: AgentProviderKind;
  model?: string;
  systemPrompt?: string;
};

type CompanyFormValues = {
  key?: string;
  name: string;
  description?: string;
  status?: ClientResourceStatus;
};

type ProjectFormValues = {
  key?: string;
  name: string;
  description?: string;
  status?: ClientResourceStatus;
};

type RepositoryFormValues = {
  key?: string;
  name: string;
  remoteRepositoryUrl: string;
  localPath: string;
  gitStrategySkillPath: string;
  description?: string;
  status?: ClientResourceStatus;
};

const providerLabel: Record<AgentProviderKind, string> = {
  [AgentProviderKind.Unknown]: "Unknown",
  [AgentProviderKind.Copilot]: "Copilot",
  [AgentProviderKind.Codex]: "Codex"
};

const statusLabel: Record<AgentStatus | ClientResourceStatus, string> = {
  [AgentStatus.Draft]: "Draft",
  [AgentStatus.Active]: "Active",
  [AgentStatus.Disabled]: "Disabled",
  [AgentStatus.Archived]: "Archived"
};

const statusColor: Record<AgentStatus | ClientResourceStatus, string> = {
  [AgentStatus.Draft]: "gold",
  [AgentStatus.Active]: "green",
  [AgentStatus.Disabled]: "red",
  [AgentStatus.Archived]: "default"
};

export default function App() {
  return (
    <ConfigProvider
      theme={{
        algorithm: theme.defaultAlgorithm,
        token: {
          colorPrimary: "#0f766e",
          colorInfo: "#2563eb",
          borderRadius: 14,
          fontFamily: "'Aptos Display', 'Noto Sans TC', 'Segoe UI Variable Display', sans-serif"
        },
        components: {
          Layout: {
            headerBg: "transparent",
            bodyBg: "transparent"
          },
          Card: {
            borderRadiusLG: 22
          }
        }
      }}
    >
      <AntApp>
        <Shell />
      </AntApp>
    </ConfigProvider>
  );
}

function Shell() {
  const [messageApi, contextHolder] = message.useMessage();
  const [apiBase, setApiBase] = useState(getApiBaseUrl());

  function saveApiBase() {
    const saved = setApiBaseUrl(apiBase);
    setApiBase(saved);
    messageApi.success(`API base saved: ${saved}`);
  }

  return (
    <Layout className="app-shell">
      {contextHolder}
      <div className="ambient ambient-a" />
      <div className="ambient ambient-b" />

      <Header className="topbar">
        <div className="brand-mark">if</div>
        <div>
          <Text className="eyebrow">ideaForge</Text>
          <Title level={3} className="brand-title">
            Agent Company Command Center
          </Title>
        </div>
      </Header>

      <Content className="content">
        <section className="hero">
          <div>
            <Text className="eyebrow">POC workspace</Text>
            <Title className="hero-title">把 Swagger 操作收斂成用戶能直接使用的工作台</Title>
            <Paragraph className="hero-copy">
              管理員工型 agent、客戶公司、專案與 repository 設定，讓下一步任務指派可以落在明確的服務範圍。
            </Paragraph>
          </div>
          <Card className="api-card">
            <Space direction="vertical" size="middle" className="full-width">
              <Space>
                <SettingOutlined />
                <Text strong>API Base</Text>
              </Space>
              <Input
                value={apiBase}
                onChange={(event) => setApiBase(event.target.value)}
                placeholder="/api"
                prefix={<ApiOutlined />}
              />
              <Button type="primary" icon={<SaveOutlined />} onClick={saveApiBase} block>
                Save API Endpoint
              </Button>
            </Space>
          </Card>
        </section>

        <Tabs
          className="workspace-tabs"
          items={[
            {
              key: "agents",
              label: (
                <Space>
                  <RobotOutlined />
                  Agents
                </Space>
              ),
              children: <AgentsPanel messageApi={messageApi} />
            },
            {
              key: "projects",
              label: (
                <Space>
                  <ApartmentOutlined />
                  Client Projects
                </Space>
              ),
              children: <ClientProjectsPanel messageApi={messageApi} />
            }
          ]}
        />
      </Content>
    </Layout>
  );
}

function AgentsPanel({ messageApi }: { messageApi: ReturnType<typeof message.useMessage>[0] }) {
  const [agents, setAgents] = useState<AgentProfileDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [createOpen, setCreateOpen] = useState(false);
  const [chatOpen, setChatOpen] = useState(false);
  const [chatAgent, setChatAgent] = useState<AgentProfileDto>();
  const [chatResult, setChatResult] = useState<AgentConversationResponse>();
  const [chatLoading, setChatLoading] = useState(false);
  const [agentForm] = Form.useForm<AgentFormValues>();
  const [chatForm] = Form.useForm<{ prompt: string }>();

  useEffect(() => {
    void loadAgents();
  }, []);

  async function loadAgents() {
    setLoading(true);
    try {
      setAgents(await api.listAgents());
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    } finally {
      setLoading(false);
    }
  }

  async function createAgent(values: AgentFormValues) {
    try {
      await api.createAgent({
        key: values.key,
        name: values.name,
        role: emptyToUndefined(values.role),
        description: emptyToUndefined(values.description),
        mission: emptyToUndefined(values.mission),
        specialties: parseList(values.specialtiesText),
        metadata: {},
        provider: values.provider,
        model: emptyToUndefined(values.model),
        systemPrompt: emptyToUndefined(values.systemPrompt)
      });
      messageApi.success("Agent created");
      setCreateOpen(false);
      agentForm.resetFields();
      await loadAgents();
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    }
  }

  async function submitChat(values: { prompt: string }) {
    if (!chatAgent) {
      return;
    }

    setChatLoading(true);
    try {
      const result = await api.chatWithAgent(chatAgent.employee.key, values.prompt);
      setChatResult(result);
      messageApi.success("Agent response received");
      await loadAgents();
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    } finally {
      setChatLoading(false);
    }
  }

  return (
    <Space direction="vertical" size="large" className="full-width">
      <Row gutter={[16, 16]}>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="Registered Agents" value={agents.length} prefix={<RobotOutlined />} />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="Codex Agents" value={agents.filter((item) => item.agentData.provider === AgentProviderKind.Codex).length} />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="Copilot Agents" value={agents.filter((item) => item.agentData.provider === AgentProviderKind.Copilot).length} />
          </Card>
        </Col>
      </Row>

      <Card
        title="Employee-style agents"
        extra={
          <Space>
            <Button icon={<ReloadOutlined />} onClick={loadAgents}>
              Refresh
            </Button>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => {
                agentForm.resetFields();
                agentForm.setFieldsValue({ provider: AgentProviderKind.Codex, model: "gpt-5.4" });
                setCreateOpen(true);
              }}
            >
              New Agent
            </Button>
          </Space>
        }
      >
        <Table<AgentProfileDto>
          rowKey={(record) => record.employee.id}
          loading={loading}
          dataSource={agents}
          pagination={{ pageSize: 8 }}
          columns={[
            {
              title: "Employee",
              render: (_, record) => (
                <Space direction="vertical" size={0}>
                  <Text strong>{record.employee.name}</Text>
                  <Text type="secondary">{record.employee.key}</Text>
                </Space>
              )
            },
            {
              title: "Provider",
              render: (_, record) => <Tag color="blue">{providerLabel[record.agentData.provider]}</Tag>
            },
            {
              title: "Model",
              render: (_, record) => <Text code>{record.agentData.model}</Text>
            },
            {
              title: "Status",
              render: (_, record) => renderStatus(record.employee.status)
            },
            {
              title: "Session",
              render: (_, record) =>
                record.agentData.sessionId ? <Text ellipsis className="session-text">{record.agentData.sessionId}</Text> : <Text type="secondary">Not started</Text>
            },
            {
              title: "Action",
              render: (_, record) => (
                <Button
                  icon={<MessageOutlined />}
                  onClick={() => {
                    setChatAgent(record);
                    setChatResult(undefined);
                    chatForm.resetFields();
                    setChatOpen(true);
                  }}
                >
                  Chat
                </Button>
              )
            }
          ]}
        />
      </Card>

      <Modal title="Create agent" open={createOpen} onCancel={() => setCreateOpen(false)} footer={null} destroyOnHidden>
        <Form form={agentForm} layout="vertical" onFinish={createAgent}>
          <Form.Item name="key" label="Key" rules={[{ required: true, message: "Agent key is required" }]}>
            <Input placeholder="docs-writer" />
          </Form.Item>
          <Form.Item name="name" label="Name" rules={[{ required: true, message: "Agent name is required" }]}>
            <Input placeholder="Docs Writer" />
          </Form.Item>
          <Form.Item name="role" label="Role">
            <Input placeholder="Documentation Agent" />
          </Form.Item>
          <Form.Item name="provider" label="Provider" rules={[{ required: true }]}>
            <Select
              options={[
                { label: "Copilot", value: AgentProviderKind.Copilot },
                { label: "Codex", value: AgentProviderKind.Codex }
              ]}
            />
          </Form.Item>
          <Form.Item name="model" label="Model">
            <Input placeholder="gpt-5.4" />
          </Form.Item>
          <Form.Item name="specialtiesText" label="Specialties">
            <Input placeholder="documentation, api, review" />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="mission" label="Mission">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="systemPrompt" label="System Prompt">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Button type="primary" htmlType="submit" block>
            Create Agent
          </Button>
        </Form>
      </Modal>

      <Drawer
        title={chatAgent ? `Chat with ${chatAgent.employee.name}` : "Chat with agent"}
        open={chatOpen}
        onClose={() => setChatOpen(false)}
        width={560}
      >
        <Form form={chatForm} layout="vertical" onFinish={submitChat}>
          <Form.Item name="prompt" label="Prompt" rules={[{ required: true, message: "Prompt is required" }]}>
            <Input.TextArea rows={6} placeholder="請幫我檢查這個 repository 的 README..." />
          </Form.Item>
          <Button type="primary" htmlType="submit" loading={chatLoading} block>
            Send Prompt
          </Button>
        </Form>

        <div className="response-box">
          {chatResult ? (
            <Space direction="vertical" className="full-width">
              <Space>
                <Badge status={chatResult.exitCode === 0 ? "success" : "error"} />
                <Text strong>Exit code {chatResult.exitCode}</Text>
                <Tag>{providerLabel[chatResult.provider]}</Tag>
                <Tag>{chatResult.model}</Tag>
              </Space>
              <pre>{chatResult.output || chatResult.errorOutput || "No output"}</pre>
            </Space>
          ) : (
            <Empty description="Agent output will appear here" />
          )}
        </div>
      </Drawer>
    </Space>
  );
}

function ClientProjectsPanel({ messageApi }: { messageApi: ReturnType<typeof message.useMessage>[0] }) {
  const [companies, setCompanies] = useState<ClientCompanyDto[]>([]);
  const [projects, setProjects] = useState<ClientProjectDto[]>([]);
  const [repositories, setRepositories] = useState<ProjectRepositoryDto[]>([]);
  const [selectedCompany, setSelectedCompany] = useState<ClientCompanyDto>();
  const [selectedProject, setSelectedProject] = useState<ClientProjectDto>();
  const [loading, setLoading] = useState(false);
  const [companyOpen, setCompanyOpen] = useState(false);
  const [projectOpen, setProjectOpen] = useState(false);
  const [repositoryOpen, setRepositoryOpen] = useState(false);
  const [editingCompany, setEditingCompany] = useState<ClientCompanyDto>();
  const [editingProject, setEditingProject] = useState<ClientProjectDto>();
  const [editingRepository, setEditingRepository] = useState<ProjectRepositoryDto>();
  const [companyForm] = Form.useForm<CompanyFormValues>();
  const [projectForm] = Form.useForm<ProjectFormValues>();
  const [repositoryForm] = Form.useForm<RepositoryFormValues>();

  useEffect(() => {
    void loadCompanies();
  }, []);

  async function loadCompanies() {
    setLoading(true);
    try {
      const items = await api.listCompanies();
      setCompanies(items);
      if (items.length > 0) {
        const next = selectedCompany ? items.find((item) => item.id === selectedCompany.id) ?? items[0] : items[0];
        setSelectedCompany(next);
        await loadProjects(next.id);
      } else {
        setSelectedCompany(undefined);
        setSelectedProject(undefined);
        setProjects([]);
        setRepositories([]);
      }
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    } finally {
      setLoading(false);
    }
  }

  async function loadProjects(companyId: string) {
    const items = await api.listProjects(companyId);
    setProjects(items);
    if (items.length > 0) {
      const next = selectedProject ? items.find((item) => item.id === selectedProject.id) ?? items[0] : items[0];
      setSelectedProject(next);
      await loadRepositories(next.id);
    } else {
      setSelectedProject(undefined);
      setRepositories([]);
    }
  }

  async function loadRepositories(projectId: string) {
    setRepositories(await api.listRepositories(projectId));
  }

  async function saveCompany(values: CompanyFormValues) {
    try {
      if (editingCompany) {
        await api.updateCompany(editingCompany.id, {
          name: values.name,
          description: emptyToUndefined(values.description),
          status: values.status ?? ClientResourceStatus.Active
        });
        messageApi.success("Company updated");
      } else {
        await api.createCompany({
          key: values.key ?? "",
          name: values.name,
          description: emptyToUndefined(values.description)
        });
        messageApi.success("Company created");
      }

      setCompanyOpen(false);
      await loadCompanies();
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    }
  }

  async function saveProject(values: ProjectFormValues) {
    if (!selectedCompany) {
      messageApi.warning("Select a company first");
      return;
    }

    try {
      if (editingProject) {
        await api.updateProject(editingProject.id, {
          name: values.name,
          description: emptyToUndefined(values.description),
          status: values.status ?? ClientResourceStatus.Active
        });
        messageApi.success("Project updated");
      } else {
        await api.createProject(selectedCompany.id, {
          key: values.key ?? "",
          name: values.name,
          description: emptyToUndefined(values.description)
        });
        messageApi.success("Project created");
      }

      setProjectOpen(false);
      await loadProjects(selectedCompany.id);
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    }
  }

  async function saveRepository(values: RepositoryFormValues) {
    if (!selectedProject) {
      messageApi.warning("Select a project first");
      return;
    }

    try {
      if (editingRepository) {
        await api.updateRepository(editingRepository.id, {
          name: values.name,
          remoteRepositoryUrl: values.remoteRepositoryUrl,
          localPath: values.localPath,
          gitStrategySkillPath: values.gitStrategySkillPath,
          description: emptyToUndefined(values.description),
          status: values.status ?? ClientResourceStatus.Active
        });
        messageApi.success("Repository updated");
      } else {
        await api.createRepository(selectedProject.id, {
          key: values.key ?? "",
          name: values.name,
          remoteRepositoryUrl: values.remoteRepositoryUrl,
          localPath: values.localPath,
          gitStrategySkillPath: values.gitStrategySkillPath,
          description: emptyToUndefined(values.description)
        });
        messageApi.success("Repository created");
      }

      setRepositoryOpen(false);
      await loadRepositories(selectedProject.id);
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    }
  }

  function openCompanyForm(company?: ClientCompanyDto) {
    setEditingCompany(company);
    companyForm.resetFields();
    if (company) {
      companyForm.setFieldsValue(company);
    }
    setCompanyOpen(true);
  }

  function openProjectForm(project?: ClientProjectDto) {
    setEditingProject(project);
    projectForm.resetFields();
    if (project) {
      projectForm.setFieldsValue(project);
    }
    setProjectOpen(true);
  }

  function openRepositoryForm(repository?: ProjectRepositoryDto) {
    setEditingRepository(repository);
    repositoryForm.resetFields();
    if (repository) {
      repositoryForm.setFieldsValue(repository);
    }
    setRepositoryOpen(true);
  }

  return (
    <Space direction="vertical" size="large" className="full-width">
      <Row gutter={[16, 16]}>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="Client Companies" value={companies.length} prefix={<ApartmentOutlined />} />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="Projects In Focus" value={projects.length} />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="Repositories In Focus" value={repositories.length} prefix={<CloudServerOutlined />} />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        <Col xs={24} xl={8}>
          <WorkspaceCard
            title="Client Companies"
            actionLabel="New Company"
            loading={loading}
            onCreate={() => openCompanyForm()}
            onRefresh={loadCompanies}
          >
            <Table<ClientCompanyDto>
              rowKey="id"
              dataSource={companies}
              pagination={false}
              size="small"
              rowClassName={(record) => (record.id === selectedCompany?.id ? "selected-row" : "")}
              columns={[
                {
                  title: "Company",
                  render: (_, record) => (
                    <Space direction="vertical" size={0}>
                      <Text strong>{record.name}</Text>
                      <Text type="secondary">{record.key}</Text>
                    </Space>
                  )
                },
                {
                  title: "Status",
                  render: (_, record) => renderStatus(record.status)
                },
                {
                  title: "Action",
                  render: (_, record) => (
                    <Space>
                      <Button
                        size="small"
                        type={record.id === selectedCompany?.id ? "primary" : "default"}
                        onClick={() => {
                          setSelectedCompany(record);
                          void loadProjects(record.id);
                        }}
                      >
                        Select
                      </Button>
                      <Button size="small" onClick={() => openCompanyForm(record)}>
                        Edit
                      </Button>
                    </Space>
                  )
                }
              ]}
            />
          </WorkspaceCard>
        </Col>

        <Col xs={24} xl={8}>
          <WorkspaceCard
            title="Client Projects"
            actionLabel="New Project"
            disabled={!selectedCompany}
            onCreate={() => openProjectForm()}
            onRefresh={() => selectedCompany && loadProjects(selectedCompany.id)}
          >
            <Table<ClientProjectDto>
              rowKey="id"
              dataSource={projects}
              pagination={false}
              size="small"
              locale={{ emptyText: selectedCompany ? <Empty description="No projects yet" /> : <Empty description="Select a company first" /> }}
              rowClassName={(record) => (record.id === selectedProject?.id ? "selected-row" : "")}
              columns={[
                {
                  title: "Project",
                  render: (_, record) => (
                    <Space direction="vertical" size={0}>
                      <Text strong>{record.name}</Text>
                      <Text type="secondary">{record.key}</Text>
                    </Space>
                  )
                },
                {
                  title: "Status",
                  render: (_, record) => renderStatus(record.status)
                },
                {
                  title: "Action",
                  render: (_, record) => (
                    <Space>
                      <Button
                        size="small"
                        type={record.id === selectedProject?.id ? "primary" : "default"}
                        onClick={() => {
                          setSelectedProject(record);
                          void loadRepositories(record.id);
                        }}
                      >
                        Select
                      </Button>
                      <Button size="small" onClick={() => openProjectForm(record)}>
                        Edit
                      </Button>
                    </Space>
                  )
                }
              ]}
            />
          </WorkspaceCard>
        </Col>

        <Col xs={24} xl={8}>
          <WorkspaceCard
            title="Project Repositories"
            actionLabel="New Repository"
            disabled={!selectedProject}
            onCreate={() => openRepositoryForm()}
            onRefresh={() => selectedProject && loadRepositories(selectedProject.id)}
          >
            <Table<ProjectRepositoryDto>
              rowKey="id"
              dataSource={repositories}
              pagination={false}
              size="small"
              locale={{ emptyText: selectedProject ? <Empty description="No repositories yet" /> : <Empty description="Select a project first" /> }}
              columns={[
                {
                  title: "Repository",
                  render: (_, record) => (
                    <Space direction="vertical" size={0}>
                      <Text strong>{record.name}</Text>
                      <Text type="secondary">{record.key}</Text>
                      <Text code className="path-text">{record.localPath}</Text>
                    </Space>
                  )
                },
                {
                  title: "Status",
                  render: (_, record) => renderStatus(record.status)
                },
                {
                  title: "Action",
                  render: (_, record) => (
                    <Button size="small" onClick={() => openRepositoryForm(record)}>
                      Edit
                    </Button>
                  )
                }
              ]}
            />
          </WorkspaceCard>
        </Col>
      </Row>

      <Modal title={editingCompany ? "Update company" : "Create company"} open={companyOpen} onCancel={() => setCompanyOpen(false)} footer={null} destroyOnHidden>
        <Form form={companyForm} layout="vertical" onFinish={saveCompany}>
          {!editingCompany && (
            <Form.Item name="key" label="Key" rules={[{ required: true, message: "Company key is required" }]}>
              <Input placeholder="contoso" />
            </Form.Item>
          )}
          <Form.Item name="name" label="Name" rules={[{ required: true, message: "Company name is required" }]}>
            <Input placeholder="Contoso Ltd." />
          </Form.Item>
          {editingCompany && <StatusField />}
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Button type="primary" htmlType="submit" block>
            {editingCompany ? "Update Company" : "Create Company"}
          </Button>
        </Form>
      </Modal>

      <Modal title={editingProject ? "Update project" : "Create project"} open={projectOpen} onCancel={() => setProjectOpen(false)} footer={null} destroyOnHidden>
        <Form form={projectForm} layout="vertical" onFinish={saveProject}>
          {!editingProject && (
            <Form.Item name="key" label="Key" rules={[{ required: true, message: "Project key is required" }]}>
              <Input placeholder="commerce-platform" />
            </Form.Item>
          )}
          <Form.Item name="name" label="Name" rules={[{ required: true, message: "Project name is required" }]}>
            <Input placeholder="Commerce Platform" />
          </Form.Item>
          {editingProject && <StatusField />}
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Button type="primary" htmlType="submit" block>
            {editingProject ? "Update Project" : "Create Project"}
          </Button>
        </Form>
      </Modal>

      <Modal
        title={editingRepository ? "Update repository" : "Create repository"}
        open={repositoryOpen}
        onCancel={() => setRepositoryOpen(false)}
        footer={null}
        destroyOnHidden
        width={680}
      >
        <Form form={repositoryForm} layout="vertical" onFinish={saveRepository}>
          {!editingRepository && (
            <Form.Item name="key" label="Key" rules={[{ required: true, message: "Repository key is required" }]}>
              <Input placeholder="order-service" />
            </Form.Item>
          )}
          <Form.Item name="name" label="Name" rules={[{ required: true, message: "Repository name is required" }]}>
            <Input placeholder="Order Service" />
          </Form.Item>
          <Form.Item name="remoteRepositoryUrl" label="Remote Repository URL" rules={[{ required: true, message: "Remote URL is required" }]}>
            <Input placeholder="https://github.com/contoso/order-service.git" />
          </Form.Item>
          <Form.Item name="localPath" label="Local Path" rules={[{ required: true, message: "Local path is required" }]}>
            <Input placeholder="D:\\projects\\clients\\contoso\\order-service" />
          </Form.Item>
          <Form.Item name="gitStrategySkillPath" label="Git Strategy Skill Path" rules={[{ required: true, message: "Git strategy skill path is required" }]}>
            <Input placeholder=".codex/skills/git-strategy/SKILL.md" />
          </Form.Item>
          {editingRepository && <StatusField />}
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Button type="primary" htmlType="submit" block>
            {editingRepository ? "Update Repository" : "Create Repository"}
          </Button>
        </Form>
      </Modal>
    </Space>
  );
}

function WorkspaceCard({
  title,
  actionLabel,
  children,
  disabled,
  loading,
  onCreate,
  onRefresh
}: {
  title: string;
  actionLabel: string;
  children: React.ReactNode;
  disabled?: boolean;
  loading?: boolean;
  onCreate: () => void;
  onRefresh?: () => void;
}) {
  return (
    <Card
      title={title}
      loading={loading}
      extra={
        <Space>
          {onRefresh && (
            <Button size="small" icon={<ReloadOutlined />} onClick={onRefresh}>
              Refresh
            </Button>
          )}
          <Button size="small" type="primary" icon={<PlusOutlined />} disabled={disabled} onClick={onCreate}>
            {actionLabel}
          </Button>
        </Space>
      }
    >
      {children}
    </Card>
  );
}

function StatusField() {
  return (
    <Form.Item name="status" label="Status" rules={[{ required: true }]}>
      <Select
        options={[
          { label: "Draft", value: ClientResourceStatus.Draft },
          { label: "Active", value: ClientResourceStatus.Active },
          { label: "Disabled", value: ClientResourceStatus.Disabled },
          { label: "Archived", value: ClientResourceStatus.Archived }
        ]}
      />
    </Form.Item>
  );
}

function renderStatus(status: AgentStatus | ClientResourceStatus) {
  return <Tag color={statusColor[status]}>{statusLabel[status]}</Tag>;
}

function parseList(value?: string) {
  return (value ?? "")
    .split(",")
    .map((item) => item.trim())
    .filter(Boolean);
}

function emptyToUndefined(value?: string) {
  const trimmed = value?.trim();
  return trimmed ? trimmed : undefined;
}

function toErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : "Unexpected error";
}
