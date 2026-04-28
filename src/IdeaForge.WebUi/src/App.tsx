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
  Popconfirm,
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
  DeleteOutlined,
  IdcardOutlined,
  MessageOutlined,
  PlusOutlined,
  ReloadOutlined,
  RobotOutlined,
  SaveOutlined,
  SettingOutlined,
  UserOutlined
} from "@ant-design/icons";
import { useEffect, useRef, useState } from "react";
import { api, getApiBaseUrl, setApiBaseUrl } from "./api";
import {
  AgentProviderKind,
  AgentStatus,
  ClientResourceStatus,
  PersonKind,
  type AgentConversationResponse,
  type AgentProfileDto,
  type ClientCompanyDto,
  type ClientProjectDto,
  type EmployeeDirectoryDto,
  type PersonDirectoryDto,
  type ProjectRepositoryDto
} from "./types";

const { Header, Content } = Layout;
const { Paragraph, Text, Title } = Typography;

type AgentFormValues = {
  key: string;
  displayName: string;
  role?: string;
  personDescription?: string;
  mission?: string;
  specialtiesText?: string;
  provider: AgentProviderKind;
  model?: string;
  systemPrompt?: string;
  codexReasoning?: string;
  codexEffort?: string;
  codexCompute?: string;
};

type PersonFormValues = {
  kind: PersonKind;
  displayName: string;
  description?: string;
  provider?: AgentProviderKind;
  model?: string;
  systemPrompt?: string;
  codexReasoning?: string;
  codexEffort?: string;
  codexCompute?: string;
};

type EmployeeFormValues = {
  personId: string;
  key: string;
  role?: string;
  mission?: string;
  status?: AgentStatus;
  specialtiesText?: string;
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

const personKindLabel: Record<PersonKind, string> = {
  [PersonKind.Unknown]: "Unknown",
  [PersonKind.Human]: "Human",
  [PersonKind.Ai]: "AI"
};

const personKindColor: Record<PersonKind, string> = {
  [PersonKind.Unknown]: "default",
  [PersonKind.Human]: "cyan",
  [PersonKind.Ai]: "geekblue"
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
              先建立人員，再設定員工職務；只有 AI 人員會出現 LLM 與 agent 設定，讓公司營運與執行任務的資料邊界清楚分開。
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
              key: "people",
              label: (
                <Space>
                  <UserOutlined />
                  新增人員
                </Space>
              ),
              children: <PeoplePanel messageApi={messageApi} />
            },
            {
              key: "employees",
              label: (
                <Space>
                  <IdcardOutlined />
                  設定職務
                </Space>
              ),
              children: <EmployeeAssignmentsPanel messageApi={messageApi} />
            },
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
                  Client Systems
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

function PeoplePanel({ messageApi }: { messageApi: ReturnType<typeof message.useMessage>[0] }) {
  const [people, setPeople] = useState<PersonDirectoryDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [savingPerson, setSavingPerson] = useState(false);
  const [deletingPersonId, setDeletingPersonId] = useState<string>();
  const [open, setOpen] = useState(false);
  const [editingPerson, setEditingPerson] = useState<PersonDirectoryDto>();
  const [personForm] = Form.useForm<PersonFormValues>();
  const watchedPersonKind = Form.useWatch("kind", personForm);
  const watchedPersonProvider = Form.useWatch("provider", personForm);
  const personFormIsAi = watchedPersonKind === PersonKind.Ai;
  const personFormIsCodex = personFormIsAi && watchedPersonProvider === AgentProviderKind.Codex;
  const personSaveInFlightRef = useRef(false);

  useEffect(() => {
    void loadPeople();
  }, []);

  async function loadPeople() {
    setLoading(true);
    try {
      setPeople(await api.listPeople());
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    } finally {
      setLoading(false);
    }
  }

  async function savePerson(values: PersonFormValues) {
    if (personSaveInFlightRef.current) {
      return;
    }

    personSaveInFlightRef.current = true;
    setSavingPerson(true);

    try {
      if (editingPerson) {
        await api.updatePerson(editingPerson.id, {
          kind: values.kind,
          displayName: values.displayName,
          description: emptyToUndefined(values.description),
          metadata: editingPerson.metadata ?? {},
          agentSettings: values.kind === PersonKind.Ai ? buildAgentSettings(values) : undefined
        });
        messageApi.success("Person updated");
      } else {
        await api.createPerson({
          kind: values.kind,
          displayName: values.displayName,
          description: emptyToUndefined(values.description),
          metadata: {},
          agentSettings: values.kind === PersonKind.Ai ? buildAgentSettings(values) : undefined
        });
        messageApi.success("Person created");
      }

      setOpen(false);
      setEditingPerson(undefined);
      personForm.resetFields();
      await loadPeople();
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    } finally {
      personSaveInFlightRef.current = false;
      setSavingPerson(false);
    }
  }

  async function deletePerson(person: PersonDirectoryDto) {
    setDeletingPersonId(person.id);
    try {
      await api.deletePerson(person.id);
      if (editingPerson?.id === person.id) {
        setOpen(false);
        setEditingPerson(undefined);
        personForm.resetFields();
      }

      messageApi.success("Person deleted");
      await loadPeople();
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    } finally {
      setDeletingPersonId(undefined);
    }
  }

  function openPersonForm(person?: PersonDirectoryDto) {
    setEditingPerson(person);
    personForm.resetFields();
    personForm.setFieldsValue(
      person
        ? {
            kind: person.kind,
            displayName: person.displayName,
            description: person.description,
            provider: person.agentSettings?.provider ?? AgentProviderKind.Codex,
            model: person.agentSettings?.model ?? "gpt-5.4",
            systemPrompt: person.agentSettings?.systemPrompt,
            codexReasoning: person.agentSettings?.codexReasoning,
            codexEffort: person.agentSettings?.codexEffort,
            codexCompute: person.agentSettings?.codexCompute
          }
        : {
            kind: PersonKind.Human,
            provider: AgentProviderKind.Codex,
            model: "gpt-5.4"
          }
    );
    setOpen(true);
  }

  return (
    <Space direction="vertical" size="large" className="full-width">
      <Row gutter={[16, 16]}>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="People" value={people.length} prefix={<UserOutlined />} />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="Human" value={people.filter((item) => item.kind === PersonKind.Human).length} />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="AI" value={people.filter((item) => item.kind === PersonKind.Ai).length} prefix={<RobotOutlined />} />
          </Card>
        </Col>
      </Row>

      <Card
        title="People directory"
        extra={
          <Space>
            <Button icon={<ReloadOutlined />} onClick={loadPeople}>
              Refresh
            </Button>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => openPersonForm()}>
              New Person
            </Button>
          </Space>
        }
      >
        <Table<PersonDirectoryDto>
          rowKey="id"
          loading={loading}
          dataSource={people}
          pagination={{ pageSize: 8 }}
          columns={[
            {
              title: "Person",
              render: (_, record) => (
                <Space direction="vertical" size={0}>
                  <Text strong>{record.displayName}</Text>
                  <Text type="secondary">{record.description ?? "No description"}</Text>
                </Space>
              )
            },
            {
              title: "Kind",
              render: (_, record) => renderPersonKind(record.kind)
            },
            {
              title: "AI Settings",
              render: (_, record) =>
                record.agentSettings ? (
                  <Space>
                    <Tag>{providerLabel[record.agentSettings.provider]}</Tag>
                    <Text code>{record.agentSettings.model}</Text>
                  </Space>
                ) : (
                  <Text type="secondary">Not an AI person</Text>
                )
            },
            {
              title: "Action",
              render: (_, record) => (
                <Space>
                  <Button size="small" onClick={() => openPersonForm(record)} disabled={deletingPersonId === record.id}>
                    Edit
                  </Button>
                  <Popconfirm
                    title="Delete person"
                    description="Only unassigned people can be deleted."
                    okText="Delete"
                    cancelText="Cancel"
                    okButtonProps={{ danger: true }}
                    onConfirm={() => deletePerson(record)}
                  >
                    <Button danger size="small" icon={<DeleteOutlined />} loading={deletingPersonId === record.id}>
                      Delete
                    </Button>
                  </Popconfirm>
                </Space>
              )
            }
          ]}
        />
      </Card>

      <Modal title={editingPerson ? "Update person" : "Create person"} open={open} onCancel={() => setOpen(false)} footer={null} destroyOnHidden width={720}>
        <Form form={personForm} layout="vertical" onFinish={savePerson}>
          <Form.Item
            name="kind"
            label="Person Type"
            extra="AI people store LLM agent settings on the person profile."
            rules={[{ required: true, message: "Person type is required" }]}
          >
            <Select
              options={[
                { label: "Human", value: PersonKind.Human },
                { label: "AI", value: PersonKind.Ai }
              ]}
            />
          </Form.Item>
          <Form.Item name="displayName" label="Display Name" rules={[{ required: true, message: "Display name is required" }]}>
            <Input placeholder="Jane Chen or Docs Writer" />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} placeholder="Person profile, responsibility, or background." />
          </Form.Item>
          {personFormIsAi && (
            <Card size="small" title="AI Agent Settings" className="embedded-card">
              <Form.Item name="provider" label="Provider" rules={[{ required: true, message: "Provider is required for AI people" }]}>
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
              <Form.Item name="systemPrompt" label="System Prompt">
                <Input.TextArea rows={3} placeholder="Optional instructions for this AI person." />
              </Form.Item>
              {personFormIsCodex && <CodexCliSettingsFields />}
            </Card>
          )}
          <Button type="primary" htmlType="submit" block loading={savingPerson} disabled={savingPerson}>
            {editingPerson ? "Update Person" : "Create Person"}
          </Button>
        </Form>
      </Modal>
    </Space>
  );
}

function EmployeeAssignmentsPanel({ messageApi }: { messageApi: ReturnType<typeof message.useMessage>[0] }) {
  const [people, setPeople] = useState<PersonDirectoryDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDirectoryDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [deletingEmployeeId, setDeletingEmployeeId] = useState<string>();
  const [open, setOpen] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState<EmployeeDirectoryDto>();
  const [employeeForm] = Form.useForm<EmployeeFormValues>();
  const watchedPersonId = Form.useWatch("personId", employeeForm);
  const selectedPerson = editingEmployee?.person ?? people.find((person) => person.id === watchedPersonId);

  useEffect(() => {
    void loadDirectory();
  }, []);

  async function loadDirectory() {
    setLoading(true);
    try {
      const [nextPeople, nextEmployees] = await Promise.all([api.listPeople(), api.listEmployees()]);
      setPeople(nextPeople);
      setEmployees(nextEmployees);
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    } finally {
      setLoading(false);
    }
  }

  async function saveEmployee(values: EmployeeFormValues) {
    try {
      if (editingEmployee) {
        await api.updateEmployee(editingEmployee.id, {
          key: values.key,
          role: emptyToUndefined(values.role),
          mission: emptyToUndefined(values.mission),
          status: values.status ?? AgentStatus.Active,
          specialties: parseList(values.specialtiesText),
          metadata: editingEmployee.metadata ?? {}
        });
        messageApi.success("Employee role updated");
      } else {
        await api.createEmployee({
          personId: values.personId,
          key: values.key,
          role: emptyToUndefined(values.role),
          mission: emptyToUndefined(values.mission),
          specialties: parseList(values.specialtiesText),
          metadata: {}
        });
        messageApi.success("Employee role created");
      }

      setOpen(false);
      setEditingEmployee(undefined);
      employeeForm.resetFields();
      await loadDirectory();
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    }
  }

  async function deleteEmployee(employee: EmployeeDirectoryDto) {
    setDeletingEmployeeId(employee.id);
    try {
      await api.deleteEmployee(employee.id);
      if (editingEmployee?.id === employee.id) {
        setOpen(false);
        setEditingEmployee(undefined);
        employeeForm.resetFields();
      }

      messageApi.success("Employee role deleted");
      await loadDirectory();
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    } finally {
      setDeletingEmployeeId(undefined);
    }
  }

  function openEmployeeForm(employee?: EmployeeDirectoryDto) {
    setEditingEmployee(employee);
    employeeForm.resetFields();
    employeeForm.setFieldsValue(
      employee
        ? {
            personId: employee.person.id,
            key: employee.key,
            role: employee.role,
            mission: employee.mission,
            status: employee.status,
            specialtiesText: employee.specialties.join(", ")
          }
        : {
            status: AgentStatus.Active
          }
    );
    setOpen(true);
  }

  const assignedPersonIds = new Set(employees.map((employee) => employee.person.id));
  const availablePeople = people.filter((person) => editingEmployee?.person.id === person.id || !assignedPersonIds.has(person.id));

  return (
    <Space direction="vertical" size="large" className="full-width">
      <Row gutter={[16, 16]}>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="Employees" value={employees.length} prefix={<IdcardOutlined />} />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="AI Employees" value={employees.filter((item) => item.person.kind === PersonKind.Ai).length} prefix={<RobotOutlined />} />
          </Card>
        </Col>
        <Col xs={24} md={8}>
          <Card className="metric-card">
            <Statistic title="Unassigned People" value={people.length - employees.length} />
          </Card>
        </Col>
      </Row>

      <Card
        title="Employee role assignments"
        extra={
          <Space>
            <Button icon={<ReloadOutlined />} onClick={loadDirectory}>
              Refresh
            </Button>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => openEmployeeForm()} disabled={availablePeople.length === 0}>
              New Role
            </Button>
          </Space>
        }
      >
        <Table<EmployeeDirectoryDto>
          rowKey="id"
          loading={loading}
          dataSource={employees}
          pagination={{ pageSize: 8 }}
          columns={[
            {
              title: "Employee",
              render: (_, record) => (
                <Space direction="vertical" size={0}>
                  <Text strong>{record.person.displayName}</Text>
                  <Text type="secondary">{record.key}</Text>
                </Space>
              )
            },
            {
              title: "Type",
              render: (_, record) => renderPersonKind(record.person.kind)
            },
            {
              title: "Role",
              render: (_, record) => record.role ?? <Text type="secondary">Not set</Text>
            },
            {
              title: "AI Settings",
              render: (_, record) =>
                record.person.agentSettings ? (
                  <Space>
                    <Tag>{providerLabel[record.person.agentSettings.provider]}</Tag>
                    <Text code>{record.person.agentSettings.model}</Text>
                  </Space>
                ) : (
                  <Text type="secondary">No person-level AI settings</Text>
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
                  <Button size="small" onClick={() => openEmployeeForm(record)} disabled={deletingEmployeeId === record.id}>
                    Edit
                  </Button>
                  <Popconfirm
                    title="Delete employee role"
                    description="Execution history for this employee role will also be deleted. Person-level AI settings remain."
                    okText="Delete"
                    cancelText="Cancel"
                    okButtonProps={{ danger: true }}
                    onConfirm={() => deleteEmployee(record)}
                  >
                    <Button danger size="small" icon={<DeleteOutlined />} loading={deletingEmployeeId === record.id}>
                      Delete
                    </Button>
                  </Popconfirm>
                </Space>
              )
            }
          ]}
        />
      </Card>

      <Modal title={editingEmployee ? "Update role" : "Set employee role"} open={open} onCancel={() => setOpen(false)} footer={null} destroyOnHidden width={720}>
        <Form form={employeeForm} layout="vertical" onFinish={saveEmployee}>
          <Form.Item name="personId" label="Person" rules={[{ required: true, message: "Person is required" }]}>
            <Select
              disabled={!!editingEmployee}
              placeholder="Select a person"
              options={availablePeople.map((person) => ({
                label: `${person.displayName} (${personKindLabel[person.kind]})`,
                value: person.id
              }))}
            />
          </Form.Item>
          <Form.Item name="key" label="Employee Key" rules={[{ required: true, message: "Employee key is required" }]}>
            <Input placeholder="docs-writer" />
          </Form.Item>
          <Form.Item name="role" label="Role">
            <Input placeholder="Documentation Agent or Backend Engineer" />
          </Form.Item>
          <Form.Item name="mission" label="Mission">
            <Input.TextArea rows={2} placeholder="Long-term responsibility for this employee." />
          </Form.Item>
          {editingEmployee && <StatusField />}
          <Form.Item name="specialtiesText" label="Specialties">
            <Input placeholder="documentation, api, review" />
          </Form.Item>

          {selectedPerson?.kind === PersonKind.Ai && (
            <Card size="small" className="embedded-card">
              <Text type="secondary">AI provider/model settings are configured on the person profile.</Text>
            </Card>
          )}

          {selectedPerson?.kind === PersonKind.Human && (
            <Card size="small" className="embedded-card">
              <Text type="secondary">This is a human person, so no AI provider/model settings are required.</Text>
            </Card>
          )}

          <Button type="primary" htmlType="submit" block>
            {editingEmployee ? "Update Role" : "Create Role"}
          </Button>
        </Form>
      </Modal>
    </Space>
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
  const watchedAgentProvider = Form.useWatch("provider", agentForm);
  const agentFormIsCodex = watchedAgentProvider === AgentProviderKind.Codex;

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
        displayName: values.displayName,
        personDescription: emptyToUndefined(values.personDescription),
        personMetadata: {},
        role: emptyToUndefined(values.role),
        mission: emptyToUndefined(values.mission),
        specialties: parseList(values.specialtiesText),
        metadata: {},
        provider: values.provider,
        model: emptyToUndefined(values.model),
        systemPrompt: emptyToUndefined(values.systemPrompt),
        codexReasoning: emptyToUndefined(values.codexReasoning),
        codexEffort: emptyToUndefined(values.codexEffort),
        codexCompute: emptyToUndefined(values.codexCompute)
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
        title="AI employees"
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
                  <Text strong>{record.person.displayName}</Text>
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
          <Form.Item name="displayName" label="Person Display Name" rules={[{ required: true, message: "Person display name is required" }]}>
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
          <Form.Item name="personDescription" label="Person Description">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="mission" label="Mission">
            <Input.TextArea rows={2} />
          </Form.Item>
          <Form.Item name="systemPrompt" label="System Prompt">
            <Input.TextArea rows={3} />
          </Form.Item>
          {agentFormIsCodex && <CodexCliSettingsFields />}
          <Button type="primary" htmlType="submit" block>
            Create Agent
          </Button>
        </Form>
      </Modal>

      <Drawer
        title={chatAgent ? `Chat with ${chatAgent.person.displayName}` : "Chat with agent"}
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

  async function selectCompany(company: ClientCompanyDto) {
    setSelectedCompany(company);
    await loadProjects(company.id);
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
        const existingCompany = findCompanyByKey(companies, values.key);
        if (existingCompany) {
          setCompanyOpen(false);
          await selectCompany(existingCompany);
          messageApi.warning(`Company key "${existingCompany.key}" already exists. Selected the existing company.`);
          return;
        }

        await api.createCompany({
          key: values.key?.trim() ?? "",
          name: values.name,
          description: emptyToUndefined(values.description)
        });
        messageApi.success("Company created");
      }

      setCompanyOpen(false);
      await loadCompanies();
    } catch (error) {
      if (!editingCompany && isAlreadyExistsError(error) && values.key) {
        try {
          const existingCompany = await api.getCompanyByKey(values.key);
          setCompanyOpen(false);
          await loadCompanies();
          await selectCompany(existingCompany);
          messageApi.warning(`Company key "${existingCompany.key}" already exists. Selected the existing company.`);
          return;
        } catch {
          // Fall through to the original API error when the duplicate cannot be loaded.
        }
      }

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
        messageApi.success("System updated");
      } else {
        await api.createProject(selectedCompany.id, {
          key: values.key ?? "",
          name: values.name,
          description: emptyToUndefined(values.description)
        });
        messageApi.success("System created");
      }

      setProjectOpen(false);
      await loadProjects(selectedCompany.id);
    } catch (error) {
      messageApi.error(toErrorMessage(error));
    }
  }

  async function saveRepository(values: RepositoryFormValues) {
    if (!selectedProject) {
      messageApi.warning("Select a system first");
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
            <Statistic title="Systems In Focus" value={projects.length} />
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
                          void selectCompany(record);
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
            title="Client Systems"
            actionLabel="New System"
            disabled={!selectedCompany}
            onCreate={() => openProjectForm()}
            onRefresh={() => selectedCompany && loadProjects(selectedCompany.id)}
          >
            <Table<ClientProjectDto>
              rowKey="id"
              dataSource={projects}
              pagination={false}
              size="small"
              locale={{ emptyText: selectedCompany ? <Empty description="No systems yet" /> : <Empty description="Select a company first" /> }}
              rowClassName={(record) => (record.id === selectedProject?.id ? "selected-row" : "")}
              columns={[
                {
                  title: "System",
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
            title="System Repositories"
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
              locale={{ emptyText: selectedProject ? <Empty description="No repositories yet" /> : <Empty description="Select a system first" /> }}
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
            <Form.Item
              name="key"
              label="Key"
              extra="Key must be unique. If it already exists, submit will select the existing company instead."
              rules={[
                { required: true, message: "Company key is required" },
                {
                  warningOnly: true,
                  validator: (_, value) => {
                    const existingCompany = findCompanyByKey(companies, value);
                    return existingCompany
                      ? Promise.reject(new Error(`Key already exists. Existing company: ${existingCompany.name}`))
                      : Promise.resolve();
                  }
                }
              ]}
            >
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

      <Modal title={editingProject ? "Update system" : "Create system"} open={projectOpen} onCancel={() => setProjectOpen(false)} footer={null} destroyOnHidden>
        <Form form={projectForm} layout="vertical" onFinish={saveProject}>
          {!editingProject && (
            <Form.Item
              name="key"
              label="System Key"
              extra="A system is a client-owned application or product under the selected company."
              rules={[{ required: true, message: "System key is required" }]}
            >
              <Input placeholder="commerce-platform" />
            </Form.Item>
          )}
          <Form.Item name="name" label="System Name" rules={[{ required: true, message: "System name is required" }]}>
            <Input placeholder="Commerce Platform" />
          </Form.Item>
          {editingProject && <StatusField />}
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Button type="primary" htmlType="submit" block>
            {editingProject ? "Update System" : "Create System"}
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

function renderPersonKind(kind: PersonKind) {
  return <Tag color={personKindColor[kind]}>{personKindLabel[kind]}</Tag>;
}

function buildAgentSettings(values: PersonFormValues) {
  return {
    provider: values.provider ?? AgentProviderKind.Codex,
    model: emptyToUndefined(values.model),
    systemPrompt: emptyToUndefined(values.systemPrompt),
    codexReasoning: emptyToUndefined(values.codexReasoning),
    codexEffort: emptyToUndefined(values.codexEffort),
    codexCompute: emptyToUndefined(values.codexCompute)
  };
}

function CodexCliSettingsFields() {
  return (
    <>
      <Form.Item name="codexReasoning" label="Codex Reasoning" preserve={false} extra="Passed to Codex CLI as --reasoning; leave empty to use CLI defaults.">
        <Input placeholder="for example: high" />
      </Form.Item>
      <Form.Item name="codexEffort" label="Codex Effort" preserve={false} extra="Passed to Codex CLI as --effort; leave empty to use CLI defaults.">
        <Input placeholder="for example: high" />
      </Form.Item>
      <Form.Item name="codexCompute" label="Codex Compute" preserve={false} extra="Passed to Codex CLI as --compute; leave empty to use CLI defaults.">
        <Input placeholder="for example: aggressive" />
      </Form.Item>
    </>
  );
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

function findCompanyByKey(companies: ClientCompanyDto[], key?: unknown) {
  const normalizedKey = normalizeKey(key);
  return normalizedKey ? companies.find((company) => normalizeKey(company.key) === normalizedKey) : undefined;
}

function normalizeKey(value?: unknown) {
  return String(value ?? "")
    .trim()
    .toLowerCase();
}

function isAlreadyExistsError(error: unknown) {
  return toErrorMessage(error).toLowerCase().includes("already exists");
}

function toErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : "Unexpected error";
}
