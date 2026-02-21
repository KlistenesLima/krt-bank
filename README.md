<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 8"/>
  <img src="https://img.shields.io/badge/Angular-17-DD0031?style=for-the-badge&logo=angular" alt="Angular 17"/>
  <img src="https://img.shields.io/badge/Kafka-3.x-231F20?style=for-the-badge&logo=apachekafka" alt="Kafka"/>
  <img src="https://img.shields.io/badge/RabbitMQ-3.x-FF6600?style=for-the-badge&logo=rabbitmq" alt="RabbitMQ"/>
  <img src="https://img.shields.io/badge/Docker-11_Containers-2496ED?style=for-the-badge&logo=docker" alt="Docker"/>
  <img src="https://img.shields.io/badge/Grafana_Cloud-Observability-F46800?style=for-the-badge&logo=grafana" alt="Grafana"/>
  <img src="https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql" alt="PostgreSQL"/>
</p>

# KRT Bank — Digital Banking Platform

**Plataforma bancaria digital completa** construida com arquitetura de microsservicos, implementando **DDD**, **CQRS**, **Event-Driven Architecture**, **Saga Pattern** e **Outbox Pattern**. O sistema abrange desde o onboarding KYC ate observabilidade full-stack com Grafana Cloud.

---

## Arquitetura

```
                            ┌──────────────────┐
                            │   Angular 17 SPA │
                            │   (Dark Mode UI) │
                            └────────┬─────────┘
                                     │ HTTPS
                            ┌────────▼─────────┐
                            │   YARP Gateway   │
                            │   (API Gateway)  │
                            └───┬──────────┬───┘
                    ┌───────────┘          └───────────┐
           ┌────────▼────────┐           ┌─────────────▼─────────┐
           │  Onboarding API │           │     Payments API      │
           │  (Registro/KYC) │           │  (PIX/Cartoes/Boleto) │
           └────────┬────────┘           └──────────┬────────────┘
                    │                               │
                    ▼                               ▼
           ┌──────────────┐     ┌──────────────────────────────────┐
           │  PostgreSQL   │     │         Event Bus (Kafka)        │
           │  (15 tabelas) │     │  fraud.approved | fraud.rejected │
           └──────────────┘     │  pix.completed  | audit.log      │
                                └───────────────┬──────────────────┘
                                                │
                    ┌───────────────────────────┼──────────────────────┐
                    ▼                           ▼                      ▼
        ┌───────────────────┐    ┌──────────────────────┐   ┌─────────────────┐
        │ FraudApproved     │    │ FraudRejected        │   │ PixTransfer     │
        │ Consumer          │    │ Consumer             │   │ Completed       │
        │ (metrics+latency) │    │ (metrics+alerts)     │   │ Consumer        │
        └───────────────────┘    └──────────────────────┘   └────────┬────────┘
                                                                      │
                                                            ┌─────────▼─────────┐
                                                            │  RabbitMQ (Tasks)  │
                                                            │  email | sms | push│
                                                            │  receipts.generate │
                                                            └─────────┬─────────┘
                                                                      │
                                                            ┌─────────▼─────────┐
                                                            │  Receipt Worker   │
                                                            │  QuestPDF + B2    │
                                                            └───────────────────┘
                                                                      │
                                          ┌───────────────────────────┼──────────┐
                                          ▼                           ▼          ▼
                                  ┌──────────────┐          ┌─────────────┐  ┌───────┐
                                  │ Backblaze B2 │          │ Grafana     │  │ Redis │
                                  │ (Receipts)   │          │ Cloud (OTel)│  │ Cache │
                                  └──────────────┘          └─────────────┘  └───────┘
```

---

## Stack Tecnologica

| Camada | Tecnologias |
|--------|-------------|
| **Backend** | .NET 8, ASP.NET Core, Entity Framework Core, MediatR, FluentValidation |
| **Frontend** | Angular 17 (Standalone), Tailwind CSS, Chart.js, Dark Mode |
| **Mensageria** | Apache Kafka (domain events) + RabbitMQ (task queues) — Dual Messaging |
| **Banco de Dados** | PostgreSQL 16 (principal), Redis (cache/sessions), MongoDB (logs) |
| **Auth** | Keycloak 23 (OpenID Connect), JWT RS256 |
| **Cloud Storage** | Backblaze B2 (S3-compatible) — comprovantes PIX |
| **Observabilidade** | OpenTelemetry → Grafana Cloud (Tempo + Mimir + Loki), 26+ metricas custom |
| **PDF** | QuestPDF — comprovantes profissionais |
| **CI/CD** | GitHub Actions (5 jobs: restore, build, test, Docker, deploy) |
| **Containers** | Docker Compose — 11 containers |
| **Gateway** | YARP Reverse Proxy |
| **Secrets** | HashiCorp Vault |

---

## Patterns e Principios Arquiteturais

| Pattern | Onde e Aplicado |
|---------|-----------------|
| **DDD (Domain-Driven Design)** | Aggregates, Value Objects, Domain Events em Payments e Onboarding |
| **CQRS** | Commands separados de Queries via MediatR em todos os servicos |
| **Saga Pattern** | Orquestracao PIX: Criacao → Anti-Fraude → Debito → Credito → Comprovante |
| **Outbox Pattern** | Eventual consistency com tabela OutboxMessages + Polling Publisher |
| **Event-Driven Architecture** | Kafka para domain events, RabbitMQ para task distribution |
| **Clean Architecture** | Camadas Domain → Application → Infrastructure → API |
| **Repository Pattern** | Abstracoes sobre EF Core com Unit of Work |
| **Anti-Corruption Layer** | Isolamento entre Bounded Contexts via contratos |
| **Circuit Breaker** | Polly para chamadas externas (B2, APIs terceiros) |
| **Retry com Backoff** | 3 tentativas com delay exponencial em uploads B2 e notificacoes |

---

## Microsservicos

### Onboarding API
Registro de contas, autenticacao, KYC (documento + selfie), perfil do usuario.

### Payments API
PIX (transferencias, QR Code, agendamentos, limites), cartoes virtuais, boletos, extrato, metas financeiras, simulador de emprestimos, seguros.

### Gateway (YARP)
Roteamento, rate limiting, agregacao de requests, health checks.

---

## Fluxo PIX — Saga Completa

```
Cliente → [1. POST /pix/transfer] → Payments API
    → [2. Valida saldo, limites, horario]
    → [3. Publica PixTransferCreatedEvent no Kafka]
    → [4. FraudAnalysisEngine avalia risco (Score 0-100)]
        ├── Score < 70 → [5a. Kafka: fraud.approved]
        │   → FraudApprovedConsumer
        │   → Debita origem, credita destino
        │   → Kafka: pix.transfer.completed
        │   → PixTransferCompletedConsumer
        │       → RabbitMQ: email + SMS + push (remetente e destinatario)
        │       → RabbitMQ: receipts.generate
        │       → ReceiptWorker: QuestPDF → Upload B2 → Presigned URL
        │
        └── Score >= 70 → [5b. Kafka: fraud.rejected]
            → FraudRejectedConsumer
            → Marca transacao como FAILED
            → RabbitMQ: 3 notificacoes urgentes (prioridade 9)
            → Kafka: audit.log
```

---

## Observabilidade — OpenTelemetry + Grafana Cloud

Telemetria completa exportada via **OTLP** diretamente para Grafana Cloud (`klisteneswar3.grafana.net`):

| Sinal | Destino | Detalhes |
|-------|---------|----------|
| **Traces** | Grafana Tempo | Instrumentacao automatica ASP.NET Core, HttpClient, EF Core |
| **Metrics** | Grafana Mimir | 26+ metricas customizadas (veja tabela abaixo) |
| **Logs** | Grafana Loki | Structured logs via Serilog + OpenTelemetry exporter |

### Metricas Customizadas (26+)

| Meter | Metrica | Tipo | Descricao |
|-------|---------|------|-----------|
| **KRT.Bank.Pix** | `krt.pix.transactions.completed` | Counter | Transacoes PIX concluidas |
| | `krt.pix.transactions.failed` | Counter | Transacoes PIX falhadas |
| | `krt.pix.transaction.duration` | Histogram | Latencia de processamento PIX (ms) |
| **KRT.Bank.Fraud** | `krt.fraud.detected` | Counter | Fraudes detectadas |
| | `krt.fraud.analysis.completed` | Counter | Analises de fraude concluidas |
| **KRT.Bank.Kafka** | `krt.kafka.messages.consumed` | Counter | Mensagens Kafka consumidas por topic |
| | `krt.kafka.consumer.errors` | Counter | Erros de processamento nos consumers |
| | `krt.kafka.consumer.latency` | Histogram | Latencia de processamento Kafka (ms) |
| **KRT.Bank.RabbitMQ** | `krt.rabbitmq.messages.published` | Counter | Mensagens publicadas no RabbitMQ |
| **KRT.Bank.Storage** | `krt.b2.uploads.completed` | Counter | Uploads B2 com sucesso |
| | `krt.b2.uploads.failed` | Counter | Uploads B2 com falha |

### Kafka Consumers Instrumentados

| Consumer | Topic | Metricas |
|----------|-------|----------|
| **FraudApprovedConsumer** | `fraud.approved` | messages.consumed, latency, pix.completed, fraud.analysis |
| **FraudRejectedConsumer** | `fraud.rejected` | messages.consumed, latency, pix.failed, fraud.detected |
| **PixTransferCompletedConsumer** | `pix.transfer.completed` | messages.consumed, latency, pix.completed, rabbitmq.published |
| **AuditLogConsumer** | `audit.log` | messages.consumed, latency, consumer.errors |

### Dashboards Grafana Cloud

**Dashboard 1 — KRT Bank: Visao Geral PIX** (9 paineis)
- PIX Completed / Failed / Fraud Detected (stat panels)
- Success Rate (gauge)
- PIX Transactions Over Time (timeseries)
- PIX Transaction Latency p50/p95/p99 (timeseries)
- Fraud Analysis Results (donut chart)
- B2 Cloud Storage Uploads (bar chart)
- RabbitMQ Notifications Published (timeseries)

**Dashboard 2 — KRT Bank: Infraestrutura & Kafka Consumers** (8 paineis)
- Kafka Messages Consumed by Topic (stat)
- Kafka Consumer Errors (stat)
- Kafka Consumer Error Rate % (gauge)
- Kafka Throughput by Topic (timeseries)
- Kafka Consumer Latency p50/p95/p99 (timeseries)
- Kafka Consumer Latency by Topic (bar gauge)
- Kafka Consumer Errors Over Time (bar chart)
- System Overview — All Metrics (table)

---

## Dual Messaging: Kafka + RabbitMQ

| Aspecto | Kafka | RabbitMQ |
|---------|-------|----------|
| **Papel** | Event Log (fatos imutaveis) | Task Queue (tarefas executaveis) |
| **Uso no KRT** | PIX created, fraud result, audit | Email, SMS, push, receipt generation |
| **Garantia** | Log imutavel, replay, auditoria BACEN | Exactly-once delivery, DLQ, retry |
| **Consumer Groups** | Compliance team, analytics | Workers de notificacao, receipt |

---

## Integracoes Cloud

### Backblaze B2 (S3-Compatible Storage)
- Interface abstraida: `ICloudStorage` (troca para AWS S3/MinIO sem alterar codigo)
- Pipeline: QuestPDF → Upload B2 → Presigned URL (download seguro)
- Retry com backoff exponencial (3 tentativas)
- Bucket: `krt-bank-receipts` (regiao `us-east-005`)

### Grafana Cloud (Observabilidade)
- Stack: `klisteneswar3.grafana.net`
- OTLP endpoint via Alloy collector
- 2 dashboards customizados com 17 paineis totais
- Metricas, traces e logs centralizados

### GitHub Actions (CI/CD)
Pipeline automatizado com 5 jobs:
```
restore → build → test (xUnit + Angular) → Docker build → deploy
```

---

## Docker — 11 Containers

```bash
docker-compose up -d
```

| Container | Porta | Descricao |
|-----------|-------|-----------|
| `krt-gateway` | 5000 | YARP API Gateway |
| `krt-onboarding-api` | 5001 | Onboarding/Auth API |
| `krt-payments-api` | 5002 | Payments/PIX API |
| `krt-web` | 4200 | Angular SPA |
| `krt-postgres` | 5432 | PostgreSQL 16 |
| `krt-redis` | 6379 | Redis (cache) |
| `krt-mongo` | 27017 | MongoDB (logs) |
| `krt-kafka` | 9092 | Apache Kafka |
| `krt-zookeeper` | 2181 | Zookeeper |
| `krt-rabbitmq` | 5672/15672 | RabbitMQ + Management UI |
| `krt-keycloak` | 8180 | Keycloak (Identity) |

---

## Endpoints da API (55+)

### Onboarding
| Metodo | Rota | Descricao |
|--------|------|-----------|
| POST | `/api/v1/accounts/register` | Criar conta |
| POST | `/api/v1/accounts/login` | Autenticacao |
| GET | `/api/v1/accounts/{id}` | Dados da conta |
| POST | `/api/v1/kyc/document` | Upload documento KYC |
| POST | `/api/v1/kyc/selfie` | Upload selfie KYC |
| POST | `/api/v1/kyc/confirm` | Confirmar KYC |

### PIX
| Metodo | Rota | Descricao |
|--------|------|-----------|
| POST | `/api/v1/pix/transfer` | Transferencia PIX |
| POST | `/api/v1/pix/transfer/schedule` | Agendar PIX |
| GET | `/api/v1/pix/transfer/{id}/receipt` | Download comprovante (PDF) |
| GET | `/api/v1/pix/transfer/{id}/status` | Status da transacao |
| GET | `/api/v1/pix/contacts` | Listar contatos PIX |
| POST | `/api/v1/pix/contacts` | Adicionar contato |
| GET | `/api/v1/pix/limits` | Consultar limites |
| PUT | `/api/v1/pix/limits` | Atualizar limites |
| POST | `/api/v1/pix/qrcode/generate` | Gerar QR Code PIX |
| POST | `/api/v1/pix/qrcode/read` | Ler QR Code PIX |

### Financeiro
| Metodo | Rota | Descricao |
|--------|------|-----------|
| GET | `/api/v1/statement` | Extrato com paginacao |
| GET | `/api/v1/statement/export` | Exportar extrato |
| GET | `/api/v1/virtual-cards` | Listar cartoes virtuais |
| POST | `/api/v1/virtual-cards` | Criar cartao virtual |
| GET | `/api/v1/boletos` | Listar boletos |
| POST | `/api/v1/boletos/generate` | Gerar boleto |
| GET | `/api/v1/goals` | Metas financeiras |
| POST | `/api/v1/goals` | Criar meta |
| POST | `/api/v1/loan/simulate` | Simulador emprestimo (Price + SAC) |

### Admin
| Metodo | Rota | Descricao |
|--------|------|-----------|
| GET | `/api/v1/admin/dashboard` | Dashboard administrativo |
| GET | `/api/v1/admin/accounts/pending` | Contas pendentes |
| POST | `/api/v1/admin/accounts/{id}/review` | Revisar conta |

---

## Testes

| Tipo | Framework | Quantidade | Escopo |
|------|-----------|------------|--------|
| **Unit** | xUnit + Moq + FluentAssertions | 100+ | Domain, Application, Services |
| **Integration** | xUnit + WebApplicationFactory | 20+ | API endpoints, DB |
| **Frontend Unit** | Jasmine + Karma | 30+ | Components, Services, Pipes |
| **E2E** | Cypress | 10+ | Fluxos completos (login, PIX, dashboard) |

```bash
# Backend
dotnet test --configuration Release --collect:"XPlat Code Coverage"

# Frontend
cd src/Web/KRT.Web
npx ng test --watch=false --browsers=ChromeHeadless --code-coverage
npx cypress run
```

---

## Estrutura do Projeto

```
krt-bank/
├── .github/workflows/          # GitHub Actions CI/CD
│   └── ci-cd.yml
├── src/
│   ├── BuildingBlocks/
│   │   ├── KRT.BuildingBlocks.Domain/           # Base entities, Value Objects
│   │   ├── KRT.BuildingBlocks.Infrastructure/   # OTel, KrtMetrics, Persistence
│   │   ├── KRT.BuildingBlocks.EventBus/         # Kafka abstractions
│   │   └── KRT.BuildingBlocks.MessageBus/       # RabbitMQ abstractions
│   ├── Services/
│   │   ├── KRT.Gateway/                         # YARP API Gateway
│   │   ├── KRT.Onboarding/
│   │   │   ├── KRT.Onboarding.Domain/
│   │   │   ├── KRT.Onboarding.Application/
│   │   │   ├── KRT.Onboarding.Infrastructure/
│   │   │   └── KRT.Onboarding.Api/
│   │   └── KRT.Payments/
│   │       ├── KRT.Payments.Domain/
│   │       ├── KRT.Payments.Application/
│   │       │   └── Consumers/                   # 4 Kafka Consumers (instrumented)
│   │       ├── KRT.Payments.Infrastructure/
│   │       └── KRT.Payments.Api/
│   └── Web/
│       └── KRT.Web/                             # Angular 17 SPA
├── tests/
│   ├── KRT.Onboarding.Tests/
│   ├── KRT.Payments.Tests/
│   └── KRT.IntegrationTests/
├── deploy/
│   └── docker/                                  # Docker configs
├── docker-compose.yml                           # 11 containers
└── README.md
```

---

| Container | Imagem | Porta | Função |
|-----------|--------|-------|--------|
| `krt-web` | Angular 17 (custom build) | **4200** | Frontend SPA — interface bancária |
| `krt-gateway` | .NET 8 (YARP) | **5000** | API Gateway — roteamento, rate limiting |
| `krt-onboarding-api` | .NET 8 | **5001** | Microsserviço de Onboarding (Contas, Auth, KYC) |
| `krt-payments-api` | .NET 8 | **5002** | Microsserviço de Pagamentos (PIX, Boleto, Cartão) |
| `krt-postgres` | postgres:16-alpine | **5433** | Banco de dados principal (15 tabelas) |
| `krt-redis` | redis:7-alpine | **6380** | Cache de contas e sessões |
| `krt-kafka` | confluentinc/cp-kafka:7.5.0 | **9092** | Event log — domain events imutáveis |
| `krt-zookeeper` | confluentinc/cp-zookeeper:7.5.0 | **32181** | Coordenação do Kafka |
| `krt-rabbitmq` | rabbitmq:3-management | **5672 / 15680** | Task queue — email, SMS, PDF |
| `krt-keycloak` | keycloak:23.0 | **8080** | Identity Provider (OAuth2 / OIDC) |
| `krt-seq` | datalust/seq:2024.1 | **8081** | Dashboard de logs estruturados |

```bash
# 1. Clone
git clone https://github.com/KlistenesLima/krt-bank.git
cd krt-bank

# 2. Subir infraestrutura
docker-compose up -d

# 3. Aplicar migrations
dotnet ef database update -p src/Services/KRT.Onboarding/KRT.Onboarding.Infrastructure -s src/Services/KRT.Onboarding/KRT.Onboarding.Api
dotnet ef database update -p src/Services/KRT.Payments/KRT.Payments.Infrastructure -s src/Services/KRT.Payments/KRT.Payments.Api

# 4. Rodar APIs
dotnet run --project src/Services/KRT.Gateway/KRT.Gateway
dotnet run --project src/Services/KRT.Onboarding/KRT.Onboarding.Api
dotnet run --project src/Services/KRT.Payments/KRT.Payments.Api

# 5. Frontend
cd src/Web/KRT.Web && npm install && npx ng serve
```

| Servico | URL |
|---------|-----|
| Angular SPA | http://localhost:4200 |
| Gateway | http://localhost:5000 |
| Onboarding API | http://localhost:5001/swagger |
| Payments API | http://localhost:5002/swagger |
| RabbitMQ UI | http://localhost:15672 (guest/guest) |
| Keycloak | http://localhost:8180 |
| Grafana Cloud | https://klisteneswar3.grafana.net |

---

## Notas Tecnicas

- **Kafka vs RabbitMQ**: Kafka para audit trail regulatorio (BACEN exige rastreabilidade), RabbitMQ para task execution com delivery guarantees.
- **Outbox Pattern**: Garante consistencia eventual entre escrita no DB e publicacao de eventos.
- **QuestPDF**: Gera comprovantes PDF profissionais com layout bancario (header, dados, footer com CNPJ).
- **OpenTelemetry**: Instrumentacao centralizada em `KrtMetrics.cs` com 5 Meters e 11+ contadores.
- **Fraud Engine**: Score 0-100 baseado em valor, frequencia, horario e historico da conta.

---

## Autor

**Klistenes Lima** — Senior .NET Engineer

[![LinkedIn](https://img.shields.io/badge/LinkedIn-blue?style=flat&logo=linkedin)](https://linkedin.com/in/klisteneslima)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=flat&logo=github)](https://github.com/KlistenesLima)

---

<p align="center">
  <sub>KRT Bank — Plataforma completa de banking digital | .NET 8 + Angular 17 + Kafka + RabbitMQ + Docker + Grafana Cloud</sub>
</p>