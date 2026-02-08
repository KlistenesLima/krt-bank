# KRT Bank - Digital Banking Platform

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Angular 17](https://img.shields.io/badge/Angular-17+-DD0031?logo=angular)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.x-FF6600?logo=rabbitmq)
![Kafka](https://img.shields.io/badge/Kafka-7.5-231F20?logo=apachekafka)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)
![Keycloak](https://img.shields.io/badge/Keycloak-23-4D4D4D)

Plataforma bancaria digital completa construida com arquitetura de microsservicos, seguindo principios de **DDD**, **CQRS**, **Event-Driven Architecture**, **Saga Pattern** e **Outbox Pattern**. O sistema abrange desde o onboarding KYC ate observabilidade em producao com Grafana e Prometheus.

**30 modulos** | **57+ endpoints REST** | **129 arquivos C#** | **70 arquivos TypeScript** | **16 page components** | **12 containers Docker**

---

## Arquitetura

```
                              +-------------------------------------+
                              |          Angular 17+ SPA            |
                              |   (Dashboard, Pix, Admin, Chat)     |
                              +----------------+--------------------+
                                               | HTTP / WebSocket
                              +----------------v--------------------+
                              |     API Gateway (YARP Proxy)         |
                              |    :5000 - Routing + Rate Limiting   |
                              +---+----------------------+----------+
                                  |                      |
                   +--------------v--------+  +----------v-------------+
                   |   Payments API :5002   |  |  Onboarding API :5001  |
                   |                        |  |                        |
                   |  - Pix (Saga Pattern)  |  |  - Registro de Conta   |
                   |  - Boletos             |  |  - KYC (Doc + Selfie)  |
                   |  - Cartoes Virtuais    |  |  - Validacao Facial    |
                   |  - Dashboard/Extrato   |  |  - JWT Auth             |
                   |  - Metas Financeiras   |  +--------+---------------+
                   |  - Simulador Emprest.  |           |
                   |  - Chatbot / Market.   |           |
                   |  - Admin / Seguros     |           |
                   |  - Notificacoes        |           |
                   |  - Metricas/Health     |           |
                   +--+------+------+------+           |
                      |      |      |                   |
            +---------v+  +--v----+ +v--------+  +-----v------+
            |PostgreSQL |  |Redis  | |RabbitMQ |  | Keycloak   |
            |  :5432    |  |:6379  | |:5672    |  | :8080      |
            +-----------+  +-------+ +---------+  +------------+

            +-----------+  +-----------+
            | Zookeeper |  |   Kafka   |
            |  :2181    |--|  :29092   |
            +-----------+  +-----------+

            +-----------+  +------------+  +--------------+
            |Prometheus |  |  Grafana   |  | AlertManager |
            |  :9090    |--|  :3000     |  |   :9093      |
            +-----------+  +------------+  +--------------+
```

---

## Stack Tecnologica

| Camada | Tecnologias |
|--------|-------------|
| **Backend** | .NET 8, ASP.NET Core, Entity Framework Core, MediatR, FluentValidation |
| **Frontend** | Angular 17+ (Standalone Components), Chart.js, SCSS, Responsive Design |
| **Autenticacao** | JWT Bearer + Keycloak 23 (OpenID Connect / OAuth 2.0) |
| **Mensageria** | RabbitMQ 3.x (Saga Pattern) + Apache Kafka (Event Streaming, Outbox) |
| **Cache** | Redis 7 (Session, Rate Limiting) |
| **Real-time** | SignalR WebSocket (Saldo ao vivo, Push Notifications) |
| **Banco de Dados** | PostgreSQL 16 (Code-First Migrations, EF Core + Npgsql) |
| **API Gateway** | YARP Reverse Proxy (Roteamento, Rate Limiting: 100 req/min) |
| **PDF** | QuestPDF (Comprovantes, Extratos) |
| **QR Code** | QRCoder (Pix QR Code BR Standard) |
| **Logging** | Serilog + Seq (Structured Logging, Enrichers) — SEQ UI em http://localhost:5341 |
| **Testes** | xUnit + Moq + FluentAssertions, Jasmine/Karma, Cypress E2E |
| **CI/CD** | GitHub Actions (5 jobs: build, test, e2e, docker, deploy) |
| **Observabilidade** | Prometheus + Grafana + AlertManager + Node Exporter |
| **Containers** | Docker + Docker Compose (12 servicos) |

---

## Features Implementadas (30 Modulos)

### Fase 1 - Fundacao (Partes 1-6)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|--------|-----------|-------------------|
| 1-4 | Onboarding + DDD | Cadastro de conta, entidades de dominio ricas, value objects | Aggregate Root, Repository Pattern, Domain Events |
| 5-6 | Autenticacao JWT | Login, registro, refresh tokens, autorizacao por roles | Bearer Token, Claims-based Auth, Keycloak Integration |

### Fase 2 - Core Banking (Partes 7-14)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|--------|-----------|-------------------|
| 7-8 | Pix com Saga Pattern | Transferencia Pix completa com orquestracao distribuida | Saga Orchestrator, Compensating Transactions, RabbitMQ |
| 9-10 | Motor de Fraude | Analise de risco em tempo real com regras configuraveis | Rule Engine, Risk Scoring, Auto-block, Kafka Consumers |
| 11 | Testes E2E + Keycloak | Testes de integracao ponta a ponta com auth real | WebApplicationFactory, OpenID Connect |
| 12 | SignalR WebSocket | Notificacoes push, saldo ao vivo, alertas de transacao | Hub Pattern, Real-time Groups, Connection Management |
| 13 | QR Code + PDF + Limites | Gerar/ler QR Code Pix, comprovantes PDF, limites configuraveis | QRCoder BR Standard, QuestPDF Templates, PixLimit Entity |
| 14 | Cartoes Virtuais + Dark Mode | Cartao Visa/Mastercard, CVV dinamico, bloquear/desbloquear | Card Number Generation (Luhn), Rotating CVV, Theme Toggle |

### Fase 3 - Experiencia do Usuario (Partes 15-22)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|--------|-----------|-------------------|
| 15 | Dashboard Interativo | Graficos de saldo, gastos por categoria, resumo mensal | Chart.js (Line, Doughnut, Bar), Auto-categorizacao |
| 16 | Extrato Completo | Filtros avancados, paginacao, export CSV/PDF | Server-side Pagination, Query Filters, File Generation |
| 17 | Pix Agendado/Recorrente | Agendamento unico e recorrente (diario/semanal/mensal) | ScheduledPix Entity, Recurrence Engine, Pause/Resume |
| 18 | Central de Notificacoes | Inbox com lidas/nao-lidas, categorias, filtros | Notification Center Pattern, Unread Badge, Batch Read |
| 19 | Contatos Favoritos Pix | Agenda de contatos com favoritos e busca | PixContact Entity, Search, Favorite Toggle |
| 20 | Boletos | Gerar, pagar, cancelar, codigo de barras automatico | Boleto Entity, Barcode Generation (47 digits), Status Machine |
| 21 | Perfil e Configuracoes | Dados pessoais, preferencias, seguranca, log de atividade | 4 Settings Tabs, 2FA Toggle, Activity Audit Log |
| 22 | Sidebar Navigation | Menu lateral colapsavel com grupos e badges | Collapsible Sidebar, Active Route, Notification Badge |

### Fase 4 - Recursos Avancados (Partes 25-28)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|--------|-----------|-------------------|
| 25 | Metas Financeiras | Criar metas com progresso, depositos, resgates | FinancialGoal Entity, Progress Tracking, Monthly Required |
| 25 | Simulador de Emprestimo | Tabela Price vs SAC com parcelas detalhadas | Price/SAC Algorithms, Amortization Schedule, Rate Comparison |
| 26 | Onboarding KYC Completo | Upload documento (RG/CNH), selfie, validacao facial | Multi-step Wizard, Face Match Simulation, Liveness Score |
| 26 | Seguros | 4 planos (Pix, Celular, Vida, Cartao) com contratacao | Insurance Plans, Policy Management, Claims System |
| 27 | Painel Administrativo | Dashboard gerencial, aprovar contas, bloquear fraudes | Admin Metrics, Fraud Alert System, Account Review |
| 28 | Chatbot IA | Assistente virtual com NLP basico e sugestoes contextuais | Intent Recognition, Contextual Suggestions, Chat UI |
| 28 | Marketplace | Cashback, cupons, sistema de pontos, resgate | Points System, Offer Catalog, Redemption, History |

### Fase 5 - Qualidade e DevOps (Partes 23-24, 29-30)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|--------|-----------|-------------------|
| 23 | Health Check e Docs | Endpoint de saude, lista de endpoints, status de servicos | Health Check Pattern, Service Status, Endpoint Catalog |
| 24 | Docker Compose | Orquestracao completa com 12 containers | Multi-stage Dockerfile, Volume Persistence, Network |
| 29 | Testes Frontend | Unit tests Jasmine + E2E Cypress | 5 Jasmine Specs, 4 Cypress E2E Specs |
| 30 | CI/CD Pipeline + Observabilidade | GitHub Actions (5 jobs) + Prometheus + Grafana | 8 Dashboard Panels, 4 Alert Rules, Metrics Middleware |

---

## API Reference (57+ Endpoints)

### Dashboard
```
GET    /api/v1/dashboard/summary/{accountId}
GET    /api/v1/dashboard/balance-history/{accountId}
GET    /api/v1/dashboard/spending-categories/{accountId}
GET    /api/v1/dashboard/monthly-summary/{accountId}
```

### Extrato
```
GET    /api/v1/statement/{accountId}?page&size&type&startDate&endDate&search&sortBy&sortOrder
GET    /api/v1/statement/{accountId}/export/csv
GET    /api/v1/statement/{accountId}/export/pdf
```

### Pix
```
POST   /api/v1/pix/transfer
POST   /api/v1/pix/qrcode/generate
GET    /api/v1/pix/receipt/{transactionId}
GET    /api/v1/pix/limits/{accountId}
PUT    /api/v1/pix/limits/{accountId}
```

### Pix Agendado
```
GET    /api/v1/pix/scheduled/account/{accountId}
POST   /api/v1/pix/scheduled
POST   /api/v1/pix/scheduled/{id}/execute
POST   /api/v1/pix/scheduled/{id}/cancel
POST   /api/v1/pix/scheduled/{id}/pause
POST   /api/v1/pix/scheduled/{id}/resume
PUT    /api/v1/pix/scheduled/{id}/amount
GET    /api/v1/pix/scheduled/{id}
```

### Contatos Pix
```
GET    /api/v1/contacts/{accountId}?favoritesOnly&search
POST   /api/v1/contacts/{accountId}
PUT    /api/v1/contacts/{accountId}/{contactId}
POST   /api/v1/contacts/{accountId}/{contactId}/favorite
DELETE /api/v1/contacts/{accountId}/{contactId}
```

### Boletos
```
GET    /api/v1/boletos/account/{accountId}?status
POST   /api/v1/boletos/generate
POST   /api/v1/boletos/pay/{boletoId}
POST   /api/v1/boletos/pay-barcode
POST   /api/v1/boletos/cancel/{boletoId}
GET    /api/v1/boletos/{boletoId}
```

### Cartoes Virtuais
```
GET    /api/v1/cards/account/{accountId}
POST   /api/v1/cards
GET    /api/v1/cards/{cardId}
POST   /api/v1/cards/{id}/block
POST   /api/v1/cards/{id}/unblock
DELETE /api/v1/cards/{id}
POST   /api/v1/cards/{id}/rotate-cvv
PUT    /api/v1/cards/{id}/settings
```

### Metas Financeiras
```
GET    /api/v1/goals/{accountId}?status
POST   /api/v1/goals/{accountId}
POST   /api/v1/goals/{accountId}/{goalId}/deposit
POST   /api/v1/goals/{accountId}/{goalId}/withdraw
POST   /api/v1/goals/{accountId}/{goalId}/cancel
```

### Simulador de Emprestimo
```
POST   /api/v1/loans/simulate
GET    /api/v1/loans/rates
```

### Notificacoes
```
GET    /api/v1/notifications/{accountId}?unreadOnly&category
GET    /api/v1/notifications/{accountId}/unread-count
POST   /api/v1/notifications/{accountId}/{id}/read
POST   /api/v1/notifications/{accountId}/read-all
DELETE /api/v1/notifications/{accountId}/{id}
POST   /api/v1/notifications/{accountId}
```

### Perfil e Configuracoes
```
GET    /api/v1/profile/{accountId}
PUT    /api/v1/profile/{accountId}
PUT    /api/v1/profile/{accountId}/preferences
PUT    /api/v1/profile/{accountId}/security
GET    /api/v1/profile/{accountId}/activity
```

### KYC / Onboarding
```
GET    /api/v1/kyc/{accountId}
POST   /api/v1/kyc/{accountId}/document
POST   /api/v1/kyc/{accountId}/selfie
POST   /api/v1/kyc/{accountId}/confirm
POST   /api/v1/kyc/{accountId}/approve
```

### Seguros
```
GET    /api/v1/insurance/plans
GET    /api/v1/insurance/{accountId}/policies
POST   /api/v1/insurance/{accountId}/subscribe
POST   /api/v1/insurance/{accountId}/cancel/{policyId}
POST   /api/v1/insurance/{accountId}/claim
```

### Chatbot
```
POST   /api/v1/chatbot/message
GET    /api/v1/chatbot/suggestions
```

### Marketplace
```
GET    /api/v1/marketplace/offers
GET    /api/v1/marketplace/{accountId}/points
POST   /api/v1/marketplace/{accountId}/redeem
GET    /api/v1/marketplace/{accountId}/history
```

### Admin
```
GET    /api/v1/admin/dashboard
GET    /api/v1/admin/accounts/pending
POST   /api/v1/admin/accounts/{accountId}/review
GET    /api/v1/admin/fraud/alerts
POST   /api/v1/admin/fraud/alerts/{alertId}/action
GET    /api/v1/admin/metrics?days
```

### Metricas e Health
```
GET    /api/v1/health
GET    /api/v1/health/detailed
GET    /api/v1/health/endpoints
GET    /api/v1/metrics/prometheus
GET    /api/v1/metrics/json
```

---

## Testes

### Backend (xUnit)

```
tests/
├── KRT.IntegrationTests/
│   └── Repositories/
│       ├── AccountRepositoryTests.cs
│       └── PixTransactionRepositoryTests.cs
└── KRT.UnitTests/
    ├── Application/
    │   └── CreateAccountCommandHandlerTests.cs
    └── Domain/
        ├── BuildingBlocks/
        │   └── ValueObjectAndResultTests.cs
        ├── Onboarding/
        │   └── AccountTests.cs
        └── Payments/
            ├── BoletoTests.cs
            ├── PixContactTests.cs
            ├── PixLimitTests.cs
            ├── PixTransactionTests.cs
            ├── ScheduledPixTests.cs
            └── VirtualCardTests.cs
```

### Frontend (Jasmine/Karma)

```
├── dashboard-charts.component.spec.ts
├── statement.component.spec.ts
├── contacts.component.spec.ts
├── goals.component.spec.ts
└── chatbot.component.spec.ts
```

### E2E (Cypress)

```
cypress/e2e/
├── dashboard.cy.ts
├── statement.cy.ts
├── chatbot.cy.ts
└── navigation.cy.ts
```

### Executar Testes

```bash
# Backend
dotnet test --verbosity normal

# Frontend Unit
cd src/Web/KRT.Web && npx ng test --watch=false --browsers=ChromeHeadless

# Cypress E2E
cd src/Web/KRT.Web && npx cypress open
# ou headless:
npx cypress run
```

---

## Como Executar

### Pre-requisitos

- .NET 8 SDK
- Node.js 20+
- Docker e Docker Compose
- Angular CLI (`npm install -g @angular/cli`)

### Execucao Local

```bash
# 1. Infraestrutura
docker-compose up -d postgres rabbitmq redis keycloak kafka zookeeper seq

# 2. Backend (3 terminais)
dotnet run --project src/Services/KRT.Payments/KRT.Payments.Api
dotnet run --project src/Services/KRT.Onboarding/KRT.Onboarding.Api
dotnet run --project src/Services/KRT.Gateway/KRT.Gateway

# 3. Frontend
cd src/Web/KRT.Web
npm install
ng serve
```

### Docker Compose (Stack completa)

```bash
# Aplicacao + infraestrutura
docker-compose up -d

# Com observabilidade (Prometheus + Grafana)
docker-compose -f docker-compose.yml -f docker-compose.observability.yml up -d
```

---

## URLs de Acesso

| Servico | URL | Credenciais |
|---------|-----|-------------|
| Frontend Angular | http://localhost:4200 | — |
| API Gateway (YARP) | http://localhost:5000 | — |
| Payments API (Swagger) | http://localhost:5002/swagger | — |
| Onboarding API (Swagger) | http://localhost:5001/swagger | — |
| PostgreSQL | localhost:5433 | krt / KrtBank2026 |
| Redis | localhost:6380 | — |
| Keycloak Admin | http://localhost:8080/admin | admin / admin |
| RabbitMQ Management | http://localhost:15680 | krt / krt123 |
| SEQ (Logs) | http://localhost:5341 | — |
| Kafka (externo) | localhost:29092 | — |
| Zookeeper | localhost:32181 | — |
| Grafana | http://localhost:3000 | admin / krtbank2026 |
| Prometheus | http://localhost:9090 | — |
| AlertManager | http://localhost:9093 | — |

---

## CI/CD Pipeline

O pipeline GitHub Actions executa automaticamente em push para `main` e `develop`:

```
+-------------+    +-------------+    +-------------+
|   Backend   |    |  Frontend   |    |    E2E      |
|  Build+Test |--->|  Build+Test |--->|   Cypress   |
|  (xUnit)    |    |  (Jasmine)  |    |             |
+------+------+    +------+------+    +------+------+
       |                  |                   |
       +----------+-------+-------------------+
                  |
           +------v------+
           |   Docker    |
           |    Build    |
           +------+------+
                  |
           +------v------+
           |   Deploy    |
           |   Staging   |
           +-------------+
```

**Jobs:** backend-build, frontend-build, e2e-tests, docker-build (main only), deploy-staging (main only)

---

## Observabilidade

### Prometheus
- Scrape de 3 servicos a cada 15s
- 4 regras de alerta: HighErrorRate, HighLatency, ServiceDown, HighMemoryUsage
- Retencao de 30 dias

### Grafana Dashboard
- Requests/segundo por servico
- Taxa de erro (2xx / 4xx / 5xx)
- Latencia P95 por endpoint
- Uso de memoria por servico

### Metrics Middleware
Toda requisicao e rastreada via `MetricsMiddleware`, expondo metricas em formato Prometheus (`/api/v1/metrics/prometheus`) e JSON (`/api/v1/metrics/json`).

---

## Estrutura do Projeto

```
krt-bank/
├── .github/workflows/
│   └── ci.yml                              # Pipeline CI/CD (5 jobs)
├── .docker/
│   └── postgres-data-clean/
├── infra/
│   ├── grafana/
│   │   ├── dashboards/                     # Dashboard JSON
│   │   └── provisioning/                   # Datasources + dashboards
│   ├── keycloak/
│   │   └── krt-bank-realm.json             # Realm config (roles, clients)
│   └── prometheus/
│       ├── prometheus.yml                  # Scrape config
│       └── alerts.yml                      # 4 alert rules
├── src/
│   ├── BuildingBlocks/
│   │   ├── KRT.BuildingBlocks.Domain/      # Entity, ValueObject, Result
│   │   ├── KRT.BuildingBlocks.EventBus/
│   │   │   └── Kafka/                      # KafkaEventBus + KafkaConsumerBase
│   │   ├── KRT.BuildingBlocks.Infrastructure/
│   │   │   ├── Behaviors/                  # MediatR Pipeline Behaviors
│   │   │   ├── Data/                       # Generic Repository + UnitOfWork
│   │   │   ├── Idempotency/                # Idempotency Keys
│   │   │   └── Outbox/                     # Outbox Pattern (transactional messaging)
│   │   └── KRT.BuildingBlocks.MessageBus/
│   │       └── Notifications/
│   ├── Services/
│   │   ├── KRT.Gateway/KRT.Gateway/        # YARP Reverse Proxy + Rate Limiting
│   │   ├── KRT.Onboarding/
│   │   │   ├── KRT.Onboarding.Api/         # AccountsController, AuthController
│   │   │   ├── KRT.Onboarding.Application/ # MediatR Commands/Queries
│   │   │   ├── KRT.Onboarding.Domain/      # Account Entity (DDD)
│   │   │   ├── KRT.Onboarding.Infra.Cache/ # Redis Cache
│   │   │   ├── KRT.Onboarding.Infra.Data/  # EF Core + PostgreSQL
│   │   │   ├── KRT.Onboarding.Infra.IoC/   # Dependency Injection
│   │   │   └── KRT.Onboarding.Infra.MessageQueue/
│   │   └── KRT.Payments/
│   │       ├── KRT.Payments.Api/            # 19 Controllers
│   │       ├── KRT.Payments.Application/    # MediatR Handlers
│   │       ├── KRT.Payments.Domain/         # 12+ Entities
│   │       ├── KRT.Payments.Infra.Data/     # EF Core + Npgsql
│   │       ├── KRT.Payments.Infra.Http/     # HttpClient + Polly
│   │       └── KRT.Payments.Infra.IoC/
│   └── Web/
│       └── KRT.Web/                         # Angular 17+ SPA
│           ├── cypress/e2e/                  # 4 Cypress E2E specs
│           └── src/app/
│               ├── pages/ (16 page components)
│               └── shared/components/ (6 shared components)
├── tests/
│   ├── KRT.IntegrationTests/                # Repository integration tests
│   └── KRT.UnitTests/                       # Domain + Application unit tests
├── docker-compose.yml                        # 12 servicos
├── docker-compose.observability.yml          # Prometheus + Grafana
└── README.md
```

---

## Padroes Arquiteturais

| Padrao | Onde e Aplicado |
|--------|----------------|
| **Domain-Driven Design (DDD)** | Entidades ricas com comportamento (Account, FinancialGoal, VirtualCard, ScheduledPix, Boleto, PixContact) |
| **CQRS** | Separacao de comandos e queries via MediatR (Pipeline Behaviors) |
| **Saga Pattern** | Transferencia Pix com orquestracao e compensacao (RabbitMQ) |
| **Event-Driven Architecture** | Kafka (Event Streaming) + RabbitMQ (Message Queue) |
| **Outbox Pattern** | Garantia de entrega de eventos via OutboxProcessor<TContext> |
| **Idempotency** | Chaves de idempotencia para operacoes financeiras |
| **Repository + Unit of Work** | Abstracao de acesso a dados com transacoes |
| **API Gateway** | YARP como ponto unico de entrada com rate limiting |
| **Health Check Pattern** | Endpoints de saude para cada servico |
| **Observer Pattern** | SignalR para notificacoes real-time |
| **Strategy Pattern** | Motor de fraude com regras plugaveis |
| **Factory Pattern** | Criacao de entidades via metodos estaticos Create() |

---

## Notas Tecnicas

- **PostgreSQL 16** e utilizado como banco principal via EF Core + Npgsql com auto-migration no startup.
- **Kafka** e utilizado para Event Streaming via `KafkaEventBus` e `KafkaConsumerBase` (Confluent.Kafka).
- **RabbitMQ** e utilizado para a Saga do Pix e comunicacao entre servicos.
- **YARP** (Yet Another Reverse Proxy) substitui o Ocelot como API Gateway, com rate limiting integrado (100 req/min por IP).
- **Serilog + Seq** para structured logging em todos os servicos. SEQ roda em http://localhost:5341 e recebe logs de ambas as APIs.
- **Seed data** e gerado automaticamente para facilitar demonstracoes.
- **Dark Mode** suportado globalmente via CSS variables.
- **Standalone Components** (Angular 17+) sao usados em todo o frontend, sem NgModules.
- **QuestPDF** opera sob licenca Community para geracao de comprovantes e extratos.

---

## Autores

**KRT Bank** — Projeto Academico 2026

