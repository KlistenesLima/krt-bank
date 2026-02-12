<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/Angular-17-DD0031?style=for-the-badge&logo=angular" />
  <img src="https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" />
  <img src="https://img.shields.io/badge/Apache_Kafka-231F20?style=for-the-badge&logo=apachekafka" />
  <img src="https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white" />
  <img src="https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white" />
  <img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" />
  <img src="https://img.shields.io/badge/Grafana-F46800?style=for-the-badge&logo=grafana&logoColor=white" />
</p>

# KRT Bank — Digital Banking Platform

Plataforma bancária digital completa construída com arquitetura de microserviços, demonstrando padrões enterprise-grade em .NET 8, Angular 17 e infraestrutura distribuída com 11 containers Docker.

> **Projeto acadêmico** desenvolvido para demonstrar domínio em arquitetura de sistemas distribuídos, DDD, Event-Driven Architecture e observabilidade em produção.

---

## Arquitetura

```
┌──────────────┐     ┌──────────────┐     ┌───────────────────────┐
│  Angular 17  │────▶│  Gateway     │────▶│  Payments API         │
│  (SPA)       │     │  (YARP)      │     │  POST /api/pix        │
└──────────────┘     └──────────────┘     └───────┬───────────────┘
                                                   │
                              ┌─────────────────┬──┴─────────────────┐
                              ▼                 ▼                    ▼
                    ┌──────────────┐   ┌──────────────┐    ┌────────────────┐
                    │ PostgreSQL   │   │ Redis Cache  │    │ Kafka          │
                    │ (EF Core)    │   │              │    │ (Event Bus)    │
                    └──────────────┘   └──────────────┘    └───────┬────────┘
                                                                   │
                    ┌──────────────────────────────────────────────┘
                    ▼                    ▼                    ▼
          ┌──────────────┐    ┌──────────────┐    ┌──────────────────┐
          │ FraudApproved│    │ FraudRejected│    │ AuditLog         │
          │ Consumer     │    │ Consumer     │    │ Consumer         │
          │ (Saga)       │    │              │    │ (Compliance)     │
          └──────┬───────┘    └──────────────┘    └──────────────────┘
                 │
                 ▼ Kafka → RabbitMQ Bridge
       ┌─────────────────────────────────────┐
       │       RabbitMQ Task Queues          │
       │  ┌─────────────┐ ┌───────────────┐ │
       │  │ Notification│ │ Receipt       │ │
       │  │ Worker      │ │ Worker        │ │
       │  │ (email/sms) │ │ (PDF → B2)    │ │
       │  └─────────────┘ └───────────────┘ │
       │  ┌─────────────────────────────┐   │
       │  │ Dead Letter Queue (DLQ)     │   │
       │  └─────────────────────────────┘   │
       └─────────────────────────────────────┘
```

---

## Stack Tecnológica

| Camada | Tecnologias |
|--------|-------------|
| **Frontend** | Angular 17, Angular Material, Chart.js, RxJS, Dark Mode |
| **API Gateway** | ASP.NET 8 + YARP Reverse Proxy, Rate Limiting |
| **Backend** | ASP.NET 8 Web API, MediatR (CQRS), FluentValidation |
| **Domain** | DDD (Aggregates, Value Objects, Domain Events), Rich Entities |
| **Persistência** | PostgreSQL 16 (EF Core 8), Redis 7 (cache distribuído) |
| **Event Streaming** | Apache Kafka — Outbox Pattern, Consumer Groups, Event Sourcing |
| **Task Queue** | RabbitMQ — Priority Queues, DLQ, Fair Dispatch, 2-Stage Pipeline |
| **Anti-Fraude** | FraudAnalysisWorker (BackgroundService), Risk Scoring 0-100 |
| **Comprovantes** | QuestPDF (geração profissional) + Backblaze B2 (cloud storage S3) |
| **Observabilidade** | OpenTelemetry → Grafana Cloud (Traces/Metrics/Logs), Dashboards customizados |
| **Autenticação** | Keycloak 23 (OpenID Connect), JWT Bearer, Role-Based Access |
| **Real-time** | SignalR WebSocket (atualização de saldo) |
| **Testes** | xUnit (unitários + integração), Cypress (E2E), 117+ testes automatizados |
| **CI/CD** | GitHub Actions (5 jobs), Docker Compose (11 containers) |

---

## Containers Docker

| # | Container | Porta | Função |
|---|-----------|-------|--------|
| 1 | **PostgreSQL 16** | 5433 | Banco principal (Onboarding + Payments) |
| 2 | **Redis 7** | 6380 | Cache distribuído (sessions, rate limiting) |
| 3 | **Apache Kafka** | 29092 | Event streaming (domain events imutáveis) |
| 4 | **Zookeeper** | 32181 | Coordenação do cluster Kafka |
| 5 | **RabbitMQ** | 5672 / 15672 | Task queues (notificações + comprovantes) |
| 6 | **Keycloak 23** | 8080 | Identity Provider (OpenID Connect) |
| 7 | **SEQ** | 8081 | Structured logging UI |
| 8 | **Onboarding API** | 5001 | Contas, autenticação, KYC |
| 9 | **Payments API** | 5002 | PIX, boletos, extratos, comprovantes |
| 10 | **Gateway (YARP)** | 5000 | Reverse proxy + rate limiting |
| 11 | **Angular SPA** | 4200 | Frontend (UI bancária) |

---

## Padrões Arquiteturais

### Domain-Driven Design (DDD)
Aggregate Roots, Value Objects, Domain Events e Repository Pattern com Unit of Work. Camadas Application, Domain e Infrastructure completamente isoladas seguindo Clean Architecture.

### CQRS com MediatR
Commands e Queries separados com Pipeline Behaviors para validação (FluentValidation), logging e idempotência.

### Saga Pattern Orquestrado
Transferências PIX executam uma saga de 2 passos (Débito → Crédito) com compensação automática em caso de falha.

### Outbox Pattern
Eventos de domínio são persistidos na mesma transação do banco, garantindo zero perda de mensagens. O OutboxProcessor publica no Kafka via polling (5s).

### Mensageria Dual: Kafka + RabbitMQ

**Por que dois brokers?** Cada um resolve uma classe diferente de problema:

| Aspecto | Kafka (Event Bus) | RabbitMQ (Message Bus) |
|---------|-------------------|----------------------|
| **Papel** | Event Streaming | Task Queue |
| **Semântica** | "O que aconteceu" (fato) | "O que precisa ser feito" (tarefa) |
| **Retenção** | Imutável, configurável | Deletado após consumo (ack) |
| **Replay** | Sim (reset de offset) | Não |
| **Consumer Groups** | Múltiplos (cada um recebe tudo) | Competição (1 consumer por msg) |
| **Prioridade** | Não | Sim (0-9) |
| **Dead Letter Queue** | Requer implementação custom | Nativo |

**Tópicos Kafka:** `fraud.approved`, `fraud.rejected`, `pix.transfer-completed`, `audit.log` e mais 5 tópicos de domínio.

**Filas RabbitMQ:** `krt.notifications.email`, `krt.notifications.sms`, `krt.notifications.push`, `krt.receipts.generate`, `krt.receipts.upload` + Dead Letter Queue (`krt.dead-letters`).

### Pipeline de Comprovantes (2 Estágios)
```
Kafka (pix.transfer-completed) → RabbitMQ (receipts.generate)
  → ReceiptWorker gera PDF com QuestPDF
    → RabbitMQ (receipts.upload) → Upload para Backblaze B2
      → Presigned URL para download seguro
```

---

## Estrutura do Projeto

```
krt-bank/
├── .github/workflows/
│   └── ci.yml                                # Pipeline CI/CD (5 jobs)
├── infra/
│   ├── grafana/                              # Dashboards + provisioning
│   ├── keycloak/krt-bank-realm.json          # Realm config (roles, clients)
│   └── prometheus/                           # Scrape config + alert rules
├── src/
│   ├── BuildingBlocks/
│   │   ├── KRT.BuildingBlocks.Domain/        # Entity, ValueObject, Result
│   │   ├── KRT.BuildingBlocks.EventBus/
│   │   │   └── Kafka/                        # KafkaEventBus + KafkaConsumerBase
│   │   ├── KRT.BuildingBlocks.Infrastructure/
│   │   │   ├── Behaviors/                    # MediatR Pipeline Behaviors
│   │   │   ├── Data/                         # Generic Repository + UnitOfWork
│   │   │   ├── Idempotency/                  # Idempotency Keys
│   │   │   ├── Observability/                # OpenTelemetry + KrtMetrics (11 métricas)
│   │   │   └── Outbox/                       # Outbox Pattern (transactional messaging)
│   │   └── KRT.BuildingBlocks.MessageBus/
│   │       ├── Notifications/                # NotificationWorker (email/sms/push)
│   │       ├── Receipts/                     # PixReceiptDocument (QuestPDF)
│   │       ├── Storage/                      # ICloudStorage → Backblaze B2 (S3)
│   │       └── ReceiptWorker.cs              # PDF generation + B2 upload pipeline
│   ├── Services/
│   │   ├── KRT.Gateway/                      # YARP Reverse Proxy + Rate Limiting
│   │   ├── KRT.Onboarding/                   # Contas, Autenticação, KYC
│   │   │   ├── KRT.Onboarding.Api/           # AccountsController, AuthController
│   │   │   ├── KRT.Onboarding.Application/   # MediatR Commands/Queries
│   │   │   ├── KRT.Onboarding.Domain/        # Account Entity (DDD Aggregate Root)
│   │   │   ├── KRT.Onboarding.Infra.Cache/   # Redis Cache
│   │   │   ├── KRT.Onboarding.Infra.Data/    # EF Core + PostgreSQL
│   │   │   └── KRT.Onboarding.Infra.IoC/     # Dependency Injection
│   │   └── KRT.Payments/
│   │       ├── KRT.Payments.Api/             # Controllers (PIX, Boletos, Admin, etc.)
│   │       ├── KRT.Payments.Application/     # MediatR Handlers + Kafka Consumers
│   │       ├── KRT.Payments.Domain/          # Entities, Value Objects, Enums
│   │       ├── KRT.Payments.Infra.Data/      # EF Core + PaymentsDbContext
│   │       ├── KRT.Payments.Infra.Http/      # HttpClient + Polly (resiliência)
│   │       └── KRT.Payments.Infra.IoC/       # DI + Kafka/RabbitMQ registration
│   └── Web/
│       └── KRT.Web/                          # Angular 17 SPA
│           ├── cypress/e2e/                  # Cypress E2E specs
│           └── src/app/
│               ├── pages/                    # 16 page components
│               └── shared/components/        # Componentes reutilizáveis
├── tests/
│   ├── KRT.IntegrationTests/                 # Repository integration tests
│   └── KRT.UnitTests/                        # Domain + Application unit tests
├── docker-compose.yml                        # 11 containers
└── README.md
```

---

## Quick Start

### Pré-requisitos

- **Docker Desktop** ≥ 4.0 (com Docker Compose V2)
- **RAM disponível:** ≥ 4 GB para os 11 containers

Para desenvolvimento local (opcional): .NET SDK 8.0, Node.js ≥ 18, Angular CLI 17.

### Subir tudo com um comando

```bash
git clone https://github.com/klistenes/krt-bank.git
cd krt-bank
docker-compose up -d --build
```

Aguarde ~2-3 minutos (primeiro build). Depois acesse:

| Serviço | URL |
|---------|-----|
| **App (Angular)** | http://localhost:4200 |
| **Swagger Payments** | http://localhost:5002/swagger |
| **Swagger Onboarding** | http://localhost:5001/swagger |
| **RabbitMQ Management** | http://localhost:15672 (krt/krt123) |
| **Keycloak Admin** | http://localhost:8080 |
| **SEQ (Logs)** | http://localhost:8081 |
| **Grafana Cloud** | https://klisteneswar3.grafana.net |

### Parar

```bash
docker-compose down       # Parar containers
docker-compose down -v    # Parar + limpar dados
```

---

## APIs e Endpoints

### Onboarding API (`:5001`) — Contas e Autenticação

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/auth/register` | Criar conta (Keycloak + PostgreSQL) |
| POST | `/api/v1/auth/login` | Login CPF + senha → JWT + dados da conta |
| GET | `/api/v1/accounts/{id}` | Dados da conta |
| GET | `/api/v1/accounts` | Listar todas as contas |
| GET | `/api/v1/accounts/by-document/{cpf}` | Buscar conta por CPF |
| GET | `/api/v1/accounts/{id}/balance` | Saldo disponível |
| GET | `/api/v1/accounts/{id}/statement` | Extrato da conta |
| POST | `/api/v1/accounts/{id}/debit` | Debitar conta (usado pela Saga PIX) |
| POST | `/api/v1/accounts/{id}/credit` | Creditar conta (usado pela Saga PIX) |

Registro unificado: cria usuário no **Keycloak** (OpenID Connect) e conta bancária no **PostgreSQL** em uma única operação. Endpoints de débito/crédito são internos (service-to-service) usados pelo Saga Pattern.

### Payments API (`:5002`) — PIX, Boletos e Serviços

**PIX:**

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/pix/transfer` | Enviar PIX (análise anti-fraude assíncrona) |
| GET | `/api/v1/pix/{id}` | Status da transação + fraud score |
| GET | `/api/v1/pix/history/{accountId}` | Histórico PIX (paginado) |
| POST | `/api/v1/pix/qrcode/generate` | Gerar QR Code PIX |
| POST | `/api/v1/pix/qrcode/image` | Imagem do QR Code (PNG) |
| GET | `/api/v1/pix/receipt/{id}` | Download comprovante PDF |
| GET | `/api/v1/pix/limits/{accountId}` | Consultar limites PIX |
| PUT | `/api/v1/pix/limits/{accountId}` | Atualizar limites |

**PIX Agendado:**

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/scheduled-pix` | Agendar PIX (único ou recorrente) |
| GET | `/api/v1/scheduled-pix/account/{accountId}` | Listar agendamentos |
| POST | `/api/v1/scheduled-pix/{id}/execute` | Executar agendamento |
| POST | `/api/v1/scheduled-pix/{id}/cancel` | Cancelar |
| POST | `/api/v1/scheduled-pix/{id}/pause` | Pausar recorrência |
| POST | `/api/v1/scheduled-pix/{id}/resume` | Retomar recorrência |

**Chaves PIX:**

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/pix-keys` | Registrar chave PIX |
| GET | `/api/v1/pix-keys/account/{accountId}` | Listar chaves da conta |
| DELETE | `/api/v1/pix-keys/{id}` | Remover chave |
| GET | `/api/v1/pix-keys/lookup/{key}` | Consultar chave (DICT simulado) |

**Boleto:**

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/boleto/generate` | Gerar boleto |
| POST | `/api/v1/boleto/pay/{id}` | Pagar boleto |
| POST | `/api/v1/boleto/pay-barcode` | Pagar por código de barras |
| GET | `/api/v1/boleto/account/{accountId}` | Listar boletos |
| GET | `/api/v1/boleto/{id}` | Detalhes do boleto |

**Contatos Favoritos:**

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/contacts/{accountId}` | Listar contatos |
| POST | `/api/v1/contacts/{accountId}` | Adicionar contato |
| POST | `/api/v1/contacts/{accountId}/{contactId}/favorite` | Favoritar |
| DELETE | `/api/v1/contacts/{accountId}/{contactId}` | Remover |

**Admin Command Center:**

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/admin/dashboard` | Dashboard administrativo (métricas) |
| GET | `/api/v1/admin/accounts/pending` | Contas pendentes de aprovação |
| POST | `/api/v1/admin/accounts/{id}/review` | Revisar conta |

---

## Integrações Cloud

### Backblaze B2 (Cloud Storage S3-Compatible)

Armazenamento de comprovantes PIX em nuvem via API S3-compatible:

- **Interface abstraída:** `ICloudStorage` permite trocar para AWS S3 ou MinIO sem alterar código
- **Pipeline:** ReceiptWorker gera PDF (QuestPDF) → Upload para B2 bucket → Presigned URL para download seguro
- **Retry com backoff:** 3 tentativas com delay exponencial em caso de falha
- **Métricas:** `krt.b2.uploads.completed` e `krt.b2.uploads.failed` exportados via OpenTelemetry
- **Bucket:** `krt-bank-receipts` na região `us-east-005`

### Grafana Cloud (Observabilidade Full-Stack)

Telemetria completa exportada via **OpenTelemetry Protocol (OTLP)** diretamente para Grafana Cloud (sem Prometheus/Grafana local):

- **Traces** → Grafana Tempo — instrumentação automática de ASP.NET Core, HttpClient, EF Core
- **Metrics** → Grafana Mimir — 11 métricas customizadas (`krt.pix.*`, `krt.fraud.*`, `krt.kafka.*`, `krt.rabbitmq.*`, `krt.b2.*`)
- **Logs** → Grafana Loki — structured logs via Serilog + OpenTelemetry exporter
- **2 Dashboards customizados:** Visão Geral PIX (8 painéis) + Infraestrutura (7 painéis)
- **Stack:** `klisteneswar3.grafana.net` — Alloy com OTLP endpoint

### GitHub Actions (CI/CD)

Pipeline automatizado com 5 jobs: restore, build, test, Docker image build e deploy.

---

## Desenvolvimento Local

Rode a infraestrutura no Docker e as APIs + Angular localmente com hot-reload:

```bash
# 1. Subir infraestrutura
docker-compose up -d postgres redis rabbitmq kafka zookeeper keycloak seq

# 2. APIs (cada uma em um terminal)
cd src/Services/KRT.Onboarding/KRT.Onboarding.Api && dotnet watch run
cd src/Services/KRT.Payments/KRT.Payments.Api && dotnet watch run
cd src/Services/KRT.Gateway/KRT.Gateway && dotnet run

# 3. Angular
cd src/Web/KRT.Web && npm install && ng serve
```

---

## Testes

```bash
# Todos os testes
dotnet test

# Apenas unitários
dotnet test tests/KRT.UnitTests

# Apenas integração
dotnet test tests/KRT.IntegrationTests

# E2E (requer app rodando)
cd src/Web/KRT.Web && npx cypress run
```

**117+ testes automatizados** cobrindo domínio, application, repositórios e fluxos E2E.

---

## Funcionalidades

### Onboarding (Contas e Autenticação)
- Abertura de conta digital com validação de CPF/CNPJ
- Registro unificado: Keycloak (IdP) + PostgreSQL em operação atômica
- Autenticação JWT via Keycloak (OpenID Connect)
- Login com CPF + senha → access token + refresh token
- Role-Based Access Control (User, Admin)
- Endpoints internos Debit/Credit para Saga Pattern (service-to-service)
- Busca por CPF, consulta de saldo, extrato

### Banking Core (Payments)
- Transferência PIX com análise anti-fraude assíncrona (Risk Score 0-100)
- Registro de chaves PIX (CPF, Email, Telefone, Aleatória) — DICT simulado
- QR Code PIX (geração e leitura)
- PIX agendado e recorrente (com pause/resume)
- Limites de transação configuráveis
- Comprovantes PDF profissionais (QuestPDF) com upload para Backblaze B2
- Extrato com filtros, busca e exportação (CSV/PDF)
- Boletos (geração, pagamento, pagamento por código de barras)
- Contatos favoritos PIX
- Metas financeiras + simulador de investimentos

### Admin Command Center
- Dashboard com gráficos Chart.js (line, donut, bar com gradientes dark theme)
- Painel administrativo com métricas operacionais
- Role-based access control (AdminGuard)
- Contas pendentes e revisão

### UI/UX Premium
- Design inspirado em Nubank/BTG/Inter
- Login/registro com backgrounds animados
- Tela de sucesso PIX com confetti e animações SVG
- Skeleton loaders com shimmer effect
- Dark mode completo em todos os componentes
- Sidebar navigation responsiva

### Integrações Cloud
- **Backblaze B2** — Upload de comprovantes PIX via S3-compatible API com presigned URLs
- **Grafana Cloud** — Observabilidade full-stack (Traces, Metrics, Logs) via OpenTelemetry OTLP
- **Keycloak** — Identity Provider com OpenID Connect e JWT
- **GitHub Actions** — Pipeline CI/CD com 5 jobs automatizados

### Infraestrutura e Resiliência
- Motor anti-fraude com scoring (0-100) e regras configuráveis
- Saga Pattern com compensação automática (Débito → Crédito → Rollback)
- Outbox Pattern para consistência transacional (zero perda de eventos)
- Kafka + RabbitMQ (dual messaging especializado)
- Dead Letter Queue para mensagens que falharam 3x
- 11 métricas customizadas OpenTelemetry instrumentadas em todos os consumers/workers
- 2 dashboards Grafana customizados (15 painéis: PIX overview + Infraestrutura)

---

## Números da Arquitetura

| Métrica | Valor |
|---------|-------|
| Containers Docker | 11 |
| Microserviços | 3 (Onboarding, Payments, Gateway) |
| Testes automatizados | 117+ |
| Kafka consumers | 4 (Fraud Approved/Rejected, PIX Completed, Audit Log) |
| RabbitMQ workers | 2 (Notification, Receipt) |
| Tópicos Kafka | 9 (domain events) |
| Filas RabbitMQ | 5 + DLQ |
| Métricas OpenTelemetry | 11 customizadas |
| Dashboards Grafana | 2 (15 painéis total) |
| Projetos na solution | 15 (.NET) |
| Componentes Angular | 22+ (16 pages + 6 shared) |

---

## Tecnologias e Versões

| Pacote/Ferramenta | Versão |
|-------------------|--------|
| .NET SDK | 8.0 |
| Angular | 17 |
| Entity Framework Core | 8.0 |
| MediatR | 12.x |
| FluentValidation | 11.x |
| Confluent.Kafka | 2.x |
| RabbitMQ.Client | 6.x |
| QuestPDF | 2024.3.0 |
| AWSSDK.S3 | 3.x (Backblaze B2) |
| OpenTelemetry | 1.7.x |
| YARP | 2.x |
| Keycloak | 23.x |
| PostgreSQL | 16 |
| Redis | 7 |
| Docker Compose | V2 |

---

## Autor

**Klístenes Lima** — Senior .NET Engineer (7+ anos)

Experiência em sistemas críticos de saúde e governo (Secretaria de Saúde de Pernambuco), com passagem por Qintess, G4F, Afixcode e Neotriad.

8 pós-graduações em tecnologia: Engenharia de Software, Desenvolvimento .NET, Ciência de Dados, Gestão de Produtos, entre outras.

---

## Licença

Este projeto é acadêmico e foi desenvolvido para fins de estudo e demonstração de arquitetura de software. QuestPDF utiliza licença Community (gratuita para receita < $1M/ano).
