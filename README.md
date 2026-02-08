<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 8"/>
  <img src="https://img.shields.io/badge/Angular-17+-DD0031?style=for-the-badge&logo=angular" alt="Angular"/>
  <img src="https://img.shields.io/badge/RabbitMQ-3.x-FF6600?style=for-the-badge&logo=rabbitmq" alt="RabbitMQ"/>
  <img src="https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker" alt="Docker"/>
  <img src="https://img.shields.io/badge/Keycloak-23-4D4D4D?style=for-the-badge&logo=keycloak" alt="Keycloak"/>
  <img src="https://img.shields.io/badge/License-Academic-green?style=for-the-badge" alt="License"/>
</p>

# KRT Bank - Digital Banking Platform

**Plataforma bancaria digital completa** construida com arquitetura de microsservicos, seguindo principios de **DDD**, **CQRS**, **Event-Driven Architecture** e **Saga Pattern**. O sistema abrange desde o onboarding KYC ate observabilidade em producao com Grafana e Prometheus.

> **30 modulos** | **55+ endpoints REST** | **100+ testes automatizados** | **25+ componentes Angular** | **12 containers Docker**

---

## Arquitetura

```
                              +-------------------------------------+
                              |          Angular 17+ SPA            |
                              |   (Dashboard, Pix, Admin, Chat)     |
                              +----------------+--------------------+
                                               | HTTP/WebSocket
                              +----------------v--------------------+
                              |        API Gateway (Ocelot)          |
                              |         :5000 - Roteamento           |
                              +---+----------------------+----------+
                                  |                      |
                   +--------------v--------+  +----------v-------------+
                   |   Payments API :5002   |  |  Onboarding API :5001  |
                   |                        |  |                        |
                   |  - Pix (Saga Pattern)  |  |  - Registro de Conta   |
                   |  - Boletos             |  |  - KYC (Doc + Selfie)  |
                   |  - Cartoes Virtuais    |  |  - Validacao Facial    |
                   |  - Dashboard/Extrato   |  |  - Aprovacao           |
                   |  - Metas Financeiras   |  +--------+---------------+
                   |  - Simulador Emprest.  |           |
                   |  - Chatbot IA          |           |
                   |  - Marketplace         |           |
                   |  - Admin Panel         |           |
                   |  - Seguros             |           |
                   |  - Notificacoes        |           |
                   |  - Metricas/Health     |           |
                   +--+------+------+------+           |
                      |      |      |                   |
            +---------v+  +--v----+ +v--------+  +-----v------+
            |SQL Server |  |Redis  | |RabbitMQ |  | Keycloak   |
            |  :1433    |  |:6379  | |:5672    |  | :8080      |
            +-----------+  +-------+ +---------+  +------------+

            +-----------+  +------------+  +--------------+
            |Prometheus |  |  Grafana   |  | AlertManager |
            |  :9090    |--|  :3000     |  |   :9093      |
            +-----------+  +------------+  +--------------+
```

---

## Stack Tecnologica

| Camada | Tecnologias |
|---|---|
| **Backend** | .NET 8, ASP.NET Core, Entity Framework Core, MediatR, FluentValidation |
| **Frontend** | Angular 17+ (Standalone Components), Chart.js, SCSS, Responsive Design |
| **Autenticacao** | JWT Bearer + Keycloak 23 (OpenID Connect / OAuth 2.0) |
| **Mensageria** | RabbitMQ 3.x (Saga Pattern, Event-Driven) |
| **Cache** | Redis 7 (Session, Rate Limiting) |
| **Real-time** | SignalR WebSocket (Saldo ao vivo, Push Notifications) |
| **Banco de Dados** | SQL Server 2022 (Code-First Migrations) |
| **Gateway** | Ocelot (Roteamento, Load Balancing, Rate Limiting) |
| **PDF** | QuestPDF (Comprovantes, Extratos) |
| **QR Code** | QRCoder (Pix QR Code BR Standard) |
| **Testes** | xUnit + Moq + FluentAssertions, Jasmine/Karma, Cypress E2E |
| **CI/CD** | GitHub Actions (5 jobs: build, test, e2e, docker, deploy) |
| **Observabilidade** | Prometheus + Grafana + AlertManager + Node Exporter |
| **Containers** | Docker + Docker Compose (12 servicos) |

---

## Features Implementadas (30 Modulos)

### Fase 1 - Fundacao (Partes 1-6)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|---|---|---|
| 1-4 | **Onboarding + DDD** | Cadastro de conta, entidades de dominio ricas, value objects | Aggregate Root, Repository Pattern, Domain Events |
| 5-6 | **Autenticacao JWT** | Login, registro, refresh tokens, autorizacao por roles | Bearer Token, Claims-based Auth, Keycloak Integration |

### Fase 2 - Core Banking (Partes 7-14)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|---|---|---|
| 7-8 | **Pix com Saga Pattern** | Transferencia Pix completa com orquestracao distribuida | Saga Orchestrator, Compensating Transactions, RabbitMQ |
| 9-10 | **Motor de Fraude** | Analise de risco em tempo real com regras configuraveis | Rule Engine, Risk Scoring, Auto-block, Event Consumers |
| 11 | **Testes E2E + Keycloak** | Testes de integracao ponta a ponta com auth real | TestContainers, WebApplicationFactory, OpenID Connect |
| 12 | **SignalR WebSocket** | Notificacoes push, saldo ao vivo, alertas de transacao | Hub Pattern, Real-time Groups, Connection Management |
| 13 | **QR Code + PDF + Limites** | Gerar/ler QR Code Pix, comprovantes PDF, limites configuraveis | QRCoder BR Standard, QuestPDF Templates, PixLimit Entity |
| 14 | **Cartoes Virtuais + Dark Mode** | Cartao Visa/Mastercard, CVV dinamico, bloquear/desbloquear | Card Number Generation (Luhn), Rotating CVV, Theme Toggle |

### Fase 3 - Experiencia do Usuario (Partes 15-22)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|---|---|---|
| 15 | **Dashboard Interativo** | Graficos de saldo, gastos por categoria, resumo mensal | Chart.js (Line, Doughnut, Bar), Auto-categorizacao |
| 16 | **Extrato Completo** | Filtros avancados, paginacao, export CSV/PDF | Server-side Pagination, Query Filters, File Generation |
| 17 | **Pix Agendado/Recorrente** | Agendamento unico e recorrente (diario/semanal/mensal) | ScheduledPix Entity, Recurrence Engine, Pause/Resume |
| 18 | **Central de Notificacoes** | Inbox com lidas/nao-lidas, categorias, filtros | Notification Center Pattern, Unread Badge, Batch Read |
| 19 | **Contatos Favoritos Pix** | Agenda de contatos com favoritos e busca | PixContact Entity, Search, Favorite Toggle, Transfer Count |
| 20 | **Boletos** | Gerar, pagar, cancelar, codigo de barras automatico | Boleto Entity, Barcode Generation (47 digits), Status Machine |
| 21 | **Perfil e Configuracoes** | Dados pessoais, preferencias, seguranca, log de atividade | 4 Settings Tabs, 2FA Toggle, Activity Audit Log |
| 22 | **Sidebar Navigation** | Menu lateral colapsavel com grupos e badges | Collapsible Sidebar, Active Route, Notification Badge |

### Fase 4 - Recursos Avancados (Partes 25-28)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|---|---|---|
| 25 | **Metas Financeiras** | Criar metas com progresso, depositos, resgates | FinancialGoal Entity, Progress Tracking, Monthly Required |
| 25 | **Simulador de Emprestimo** | Tabela Price vs SAC com parcelas detalhadas | Price/SAC Algorithms, Amortization Schedule, Rate Comparison |
| 26 | **Onboarding KYC Completo** | Upload documento (RG/CNH), selfie, validacao facial | Multi-step Wizard, Face Match Simulation, Liveness Score |
| 26 | **Seguros** | 4 planos (Pix, Celular, Vida, Cartao) com contratacao | Insurance Plans, Policy Management, Claims System |
| 27 | **Painel Administrativo** | Dashboard gerencial, aprovar contas, bloquear fraudes | Admin Metrics, Fraud Alert System, Account Review Workflow |
| 28 | **Chatbot IA** | Assistente virtual com NLP basico e sugestoes contextuais | Intent Recognition, Contextual Suggestions, Chat UI |
| 28 | **Marketplace** | Cashback, cupons, sistema de pontos, resgate | Points System, Offer Catalog, Redemption, History |

### Fase 5 - Qualidade e DevOps (Partes 23-24, 29-30)

| # | Modulo | Descricao | Destaques Tecnicos |
|---|---|---|---|
| 23 | **Health Check e Docs** | Endpoint de saude, lista de endpoints, status de servicos | Health Check Pattern, Service Status, Endpoint Catalog |
| 24 | **Docker Compose** | Orquestracao completa com 8 containers | Multi-stage Dockerfile, Volume Persistence, Network |
| 29 | **Testes Frontend** | Unit tests Jasmine + E2E Cypress | 5 Jasmine Specs (20+ tests), 4 Cypress E2E Specs |
| 30 | **CI/CD Pipeline** | GitHub Actions com 5 jobs automatizados | Build, Test, E2E, Docker Build, Deploy Staging |
| 30 | **Observabilidade** | Prometheus + Grafana + AlertManager | 8 Dashboard Panels, 4 Alert Rules, Metrics Middleware |

---

## API Reference (55+ Endpoints)

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
POST   /api/v1/cards/{id}/block
POST   /api/v1/cards/{id}/unblock
POST   /api/v1/cards/{id}/rotate-cvv
```

### Metas Financeiras
```
GET    /api/v1/goals/{accountId}?status
POST   /api/v1/goals/{accountId}
POST   /api/v1/goals/{accountId}/{goalId}/deposit
POST   /api/v1/goals/{accountId}/{goalId}/withdraw
POST   /api/v1/goals/{accountId}/{goalId}/cancel
```

### Simulador Emprestimo
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
100+ testes unitarios e de integracao
+-- Domain Tests
|   +-- PixTransferTests          - Validacao de transferencia, limites
|   +-- SagaOrchestratorTests     - Fluxo Saga completo, compensacao
|   +-- FraudAnalysisTests        - Regras de fraude, scoring
|   +-- ScheduledPixTests         - Agendamento, recorrencia, pausa
|   +-- VirtualCardTests          - Geracao, CVV, bloqueio
|   +-- PixLimitTests             - Limites diurno/noturno
|   +-- PixContactTests           - CRUD contatos, favoritos
|   +-- BoletoTests               - Geracao, pagamento, cancelamento
|   +-- FinancialGoalTests        - Metas, deposito, conclusao
+-- Integration Tests
    +-- E2E Keycloak Tests        - Fluxo completo com autenticacao
```

### Frontend (Jasmine/Karma)
```
20+ testes unitarios Angular
+-- DashboardChartsComponent      - Load, error handling, cards
+-- StatementComponent            - Filters, pagination, sorting, export
+-- ContactsComponent             - Load, favorites filter
+-- GoalsComponent                - Load, summary
+-- ChatbotComponent              - Welcome, send, empty validation
```

### E2E (Cypress)
```
4 specs com cenarios completos
+-- dashboard.cy.ts               - Cards, charts, loading
+-- statement.cy.ts               - Table, filters, export buttons
+-- chatbot.cy.ts                 - Send/receive, suggestions
+-- navigation.cy.ts              - All routes accessible
```

### Executar Testes
```bash
# Backend
dotnet test --verbosity normal

# Frontend Unit
cd src/Web/KRT.Web && ng test --watch=false --browsers=ChromeHeadless

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
- Angular CLI (npm install -g @angular/cli)

### Execucao Local

```bash
# 1. Infraestrutura
docker-compose up -d sqlserver rabbitmq redis keycloak

# 2. Backend (3 terminais)
dotnet run --project src/Services/KRT.Payments/KRT.Payments.Api
dotnet run --project src/Services/KRT.Onboarding/KRT.Onboarding.Api
dotnet run --project src/Services/KRT.Gateway/KRT.Gateway

# 3. Frontend
cd src/Web/KRT.Web
npm install
ng serve
```

### Docker Compose (Tudo)

```bash
# Stack completa (aplicacao + infraestrutura)
docker-compose up -d

# Com observabilidade (Prometheus + Grafana)
docker-compose -f docker-compose.yml -f docker-compose.observability.yml up -d
```

### URLs de Acesso

| Servico | URL | Credenciais |
|---|---|---|
| **Frontend Angular** | http://localhost:4200 | - |
| **API Gateway** | http://localhost:5000 | - |
| **Payments API** | http://localhost:5002 | - |
| **Onboarding API** | http://localhost:5001 | - |
| **Keycloak Admin** | http://localhost:8080 | admin / admin |
| **RabbitMQ Management** | http://localhost:15672 | krt / krt123 |
| **Grafana** | http://localhost:3000 | admin / krtbank2026 |
| **Prometheus** | http://localhost:9090 | - |
| **AlertManager** | http://localhost:9093 | - |

---

## CI/CD Pipeline

O pipeline GitHub Actions executa automaticamente em push para main e develop:

```
+-------------+    +-------------+    +-------------+
|   Backend   |    |  Frontend   |    |    E2E      |
|  Build+Test |    |  Build+Test |    |   Cypress   |
|  (xUnit)    |    |  (Jasmine)  |    |             |
+------+------+    +------+------+    +------+------+
       |                  |                   |
       +----------+-------+                   |
                  |                           |
           +------v------+                    |
           |   Docker    |<-------------------+
           |    Build    |
           +------+------+
                  |
           +------v------+
           |   Deploy    |
           |   Staging   |
           +-------------+
```

**Jobs:**
1. **backend-build** - Restore, Build, xUnit Tests, Code Coverage (Codecov)
2. **frontend-build** - npm ci, Lint, Jasmine/Karma, ng build production
3. **e2e-tests** - Cypress com ng serve (depende dos builds)
4. **docker-build** - Multi-stage build dos 3 servicos (somente main)
5. **deploy-staging** - Deploy para ambiente staging (somente main)

---

## Observabilidade

### Prometheus
- Scrape de 3 servicos a cada 15s
- 4 regras de alerta: HighErrorRate, HighLatency, ServiceDown, HighMemoryUsage
- Retencao de 30 dias

### Grafana Dashboard (8 paineis)
- Requests/segundo por servico
- Taxa de erro (2xx / 4xx / 5xx)
- Latencia P95 por endpoint
- Uso de memoria por servico
- Servicos ativos (up/down)
- HTTP Status Codes ao longo do tempo

### Metrics Middleware
Toda requisicao e automaticamente rastreada com latencia e status code via MetricsMiddleware, expondo metricas em formato Prometheus (/api/v1/metrics/prometheus) e JSON (/api/v1/metrics/json).

---

## Estrutura do Projeto

```
krt-bank/
+-- .github/
|   +-- workflows/
|       +-- ci.yml                          # Pipeline CI/CD
+-- infra/
|   +-- prometheus/
|   |   +-- prometheus.yml                  # Config Prometheus
|   |   +-- alerts.yml                      # Regras de alerta
|   +-- grafana/
|       +-- provisioning/                   # Datasources + dashboards
|       +-- dashboards/
|           +-- krt-bank-overview.json      # Dashboard principal
+-- src/
|   +-- Services/
|   |   +-- KRT.Gateway/                    # API Gateway (Ocelot)
|   |   +-- KRT.Payments/
|   |   |   +-- KRT.Payments.Domain/        # Entidades, Value Objects, Enums
|   |   |   +-- KRT.Payments.Api/           # Controllers, Middleware, Hubs
|   |   +-- KRT.Onboarding/
|   |       +-- KRT.Onboarding.Domain/
|   |       +-- KRT.Onboarding.Api/
|   +-- Web/
|       +-- KRT.Web/                        # Angular SPA
|           +-- src/app/
|           |   +-- components/             # Sidebar, shared components
|           |   +-- pages/                  # 15+ page components
|           +-- cypress/                    # E2E test specs
|           +-- cypress.config.ts
+-- tests/
|   +-- KRT.UnitTests/                      # 100+ xUnit tests
|   +-- KRT.IntegrationTests/              # E2E integration tests
+-- docker-compose.yml                      # Stack principal
+-- docker-compose.observability.yml        # Prometheus + Grafana
+-- README.md
```

---

## Padroes Arquiteturais

| Padrao | Onde e Aplicado |
|---|---|
| **Domain-Driven Design (DDD)** | Entidades ricas com comportamento (FinancialGoal, Boleto, VirtualCard, PixContact) |
| **CQRS** | Separacao de comandos e queries via MediatR |
| **Saga Pattern** | Transferencia Pix com orquestracao e compensacao |
| **Event-Driven** | RabbitMQ para comunicacao assincrona entre servicos |
| **Repository Pattern** | Abstracao de acesso a dados |
| **API Gateway** | Ocelot como ponto unico de entrada |
| **Health Check Pattern** | Endpoints de saude para cada servico |
| **Observer Pattern** | SignalR para notificacoes real-time |
| **Strategy Pattern** | Motor de fraude com regras plugaveis |
| **Factory Pattern** | Criacao de entidades via metodos estaticos Create() |

---

## Notas Tecnicas

- **In-memory storage** e utilizado nos controllers para fins de demonstracao. Em producao, substituir por EF Core + SQL Server.
- **Seed data** e gerado automaticamente para facilitar demonstracoes (contatos, notificacoes, transacoes).
- **Dark Mode** e suportado globalmente via CSS variables em todos os componentes.
- **Standalone Components** (Angular 17+) sao usados em todo o frontend, sem NgModules.
- **QuestPDF** opera sob licenca Community para geracao de comprovantes e extratos.

---

## Autores

**KRT Bank** - Projeto Academico 2026

---

<p align="center">
  <sub>Desenvolvido com .NET 8 + Angular 17 + RabbitMQ + Docker + Grafana</sub>
</p>