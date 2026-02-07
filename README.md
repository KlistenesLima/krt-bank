# 🏦 KRT Bank — Plataforma Bancária Distribuída

Sistema bancário distribuído com **Anti-Fraude Assíncrono**, **Saga Pattern**, **Event-Driven Architecture** e **Observabilidade completa**, desenvolvido em **.NET 8** com **Clean Architecture** e **DDD**.

---

## Arquitetura

```
┌─────────────────┐       ┌────────────────────────────┐
│   Angular SPA   │──────▶│       YARP Gateway          │
│   :4200         │       │       :5000                  │
│                 │       │  • Rate Limiting (100/min)   │
│  • Auth (OIDC)  │       │  • CorrelationId Injection   │
│  • API Client   │       │  • Aggregated Health Checks  │
└─────────────────┘       └──────────┬─────────┬─────────┘
                                     │         │
                          ┌──────────▼──┐  ┌───▼───────────┐
                          │ Onboarding  │  │  Payments     │
                          │ :5001       │  │  :5002        │
                          │             │  │               │
                          │ • Accounts  │  │ • Pix Saga    │
                          │ • Redis $   │◀─│ • Anti-Fraude │
                          │ • Kafka     │  │ • Polly       │
                          │ • Notif.    │  │ • Kafka       │
                          └──────┬──────┘  └───┬───────────┘
                                 │             │
              ┌──────────────────┼─────────────┼──────────────────┐
              │                  │             │                  │
        ┌─────▼─────┐    ┌──────▼──────┐  ┌───▼────┐     ┌──────▼──────┐
        │ PostgreSQL │    │    Kafka    │  │ Redis  │     │  RabbitMQ   │
        │  :5433     │    │  :29092     │  │ :6380  │     │  :5672      │
        │            │    │             │  │        │     │             │
        │ krt_onboard│    │ Eventos:    │  │ Cache  │     │ Notificações│
        │ krt_payment│    │ "o que      │  │ Aside  │     │ "o que      │
        │ keycloak   │    │  aconteceu" │  │ 5min   │     │  fazer"     │
        └────────────┘    └─────────────┘  └────────┘     └─────────────┘
                                                            │
                                                    ┌───────┴────────┐
                                                    │  Dead-Letter   │
                                                    │  Queue (DLQ)   │
                                                    │  3x retry      │
                                                    └────────────────┘
              ┌──────────┐    ┌──────────┐
              │   Seq    │    │ Keycloak │
              │  :5341   │    │  :8080   │
              │ Logs +   │    │ OIDC +   │
              │ Correlat. │    │ JWT      │
              └──────────┘    └──────────┘
```

---

## Stack Tecnológica

| Camada | Tecnologia | Detalhes |
|---|---|---|
| **Frontend** | Angular 17 + Material | SPA com AuthGuard, OIDC, API client |
| **API Gateway** | YARP Reverse Proxy | Rate Limiting, CorrelationId, Health Checks |
| **Backend** | .NET 8 | Clean Architecture + DDD |
| **CQRS** | MediatR | Commands, Handlers, Pipeline Behaviors |
| **Validação** | FluentValidation | Pipeline integrado ao MediatR |
| **Autenticação** | Keycloak 24 | JWT Bearer, OIDC, realm auto-import |
| **Banco de Dados** | PostgreSQL 15 | Database-per-service, auto-migration |
| **Cache** | Redis | Cache-aside (5min TTL), invalidação em write |
| **Eventos** | Apache Kafka | Outbox Pattern, IntegrationEvents, Topics |
| **Notificações** | RabbitMQ | Email/SMS/Push, Dead-Letter Queue, Prioridade |
| **Anti-Fraude** | Scoring Engine | 7 regras, async worker, 3 decisões |
| **Resiliência** | Polly | Retry 3x exponencial + Circuit Breaker |
| **Observabilidade** | Serilog → Seq | CorrelationId E2E, structured logging |
| **Containers** | Docker Compose | 9 serviços de infraestrutura |

---

## Patterns Implementados

### Clean Architecture + DDD
- **Domain Layer** — Entidades ricas com métodos de negócio (`Account.Debit()`, `Account.Block()`), Value Objects (`Cpf`, `Email`, `Money`), Domain Events, Aggregate Roots
- **Application Layer** — CQRS via MediatR, FluentValidation Pipeline Behavior, DTOs
- **Infrastructure Layer** — EF Core, Redis, Kafka, RabbitMQ, HTTP Clients
- **IoC Layer** — Registro de dependências isolado por contexto

### Saga Pattern (Pix)
```
POST /pix → PendingAnalysis
              ↓
        FraudAnalysisWorker
              ↓
        [Approved] → Debit Source → Credit Destination → Completed
                          ↓ (falha no crédito)
                     Compensação automática: Credit Source (rollback)
```

### Anti-Fraude Assíncrono
```
POST /pix → 202 Accepted (PendingAnalysis)
                    ↓ FraudAnalysisWorker (polling 2s)
              Scoring Engine (7 regras):
              ├── Valor alto (>R$5k/10k)        +30/+50 pts
              ├── Horário madrugada (00-06h)     +20 pts
              ├── Auto-transferência             +80 pts
              ├── Frequência alta (>3/hora)      +40 pts
              ├── Mesmo destino repetido         +35 pts
              ├── Valor redondo                  +10 pts
              └── Micro-transação pattern        +25 pts
                    ↓
              Score < 40  → ✅ Aprovado → Saga executa
              Score 40-70 → ⏳ Revisão manual (hold)
              Score > 70  → 🚫 Rejeitado → Alerta Email + SMS
```

### Dual Messaging (Kafka + RabbitMQ)
```
KAFKA (eventos — log imutável do que aconteceu):
  OutboxProcessor → KafkaEventBus → Topics
  • krt.accounts.created / .credited / .debited
  • krt.pix.transfer-initiated / .completed / .failed
  • krt.fraud.analysis-approved / .rejected / .review

RABBITMQ (comandos — o que precisa ser feito):
  DomainEventHandler / FraudWorker → IMessageBus → Queues
  • krt.notifications.email  (prioridade 0-9, TTL 5min)
  • krt.notifications.sms
  • krt.notifications.push
  • krt.dead-letters  (DLQ — falhas após 3 tentativas)
```

### Outbox Pattern
```
Handler → grava OutboxMessage na mesma transação do banco
OutboxProcessor → poll a cada 5s → KafkaEventBus → Topic
(garante at-least-once delivery, sem two-phase commit)
```

### Polly Resilience (Payments → Onboarding)
- **Retry**: 3 tentativas com backoff exponencial (1s, 2s, 4s), trata 5xx + 429
- **Circuit Breaker**: Abre após 5 falhas consecutivas, fica aberto 30s

### Redis Cache-Aside (Onboarding)
- `GET /accounts/{id}` → tenta Redis primeiro, fallback pro banco, cacheia 5min
- `POST /accounts/{id}/debit` e `/credit` → invalida cache automaticamente

### CorrelationId End-to-End
```
Gateway (gera/propaga X-Correlation-Id)
  → HttpRequest Header
    → Onboarding Middleware (lê header, salva no HttpContext)
      → Serilog LogContext (enriquece todos os logs)
        → CorrelationIdDelegatingHandler (propaga para HTTP calls)
          → Kafka Headers (correlation-id no evento)
```

---

## Estrutura de Projetos

```
src/
├── BuildingBlocks/
│   ├── KRT.BuildingBlocks.Domain          # Entity, AggregateRoot, ValueObject, DomainEvent
│   ├── KRT.BuildingBlocks.EventBus        # IEventBus, KafkaEventBus, IntegrationEvent
│   ├── KRT.BuildingBlocks.Infrastructure  # Repository<T>, OutboxProcessor, UoW
│   └── KRT.BuildingBlocks.MessageBus      # IMessageBus, RabbitMqBus, NotificationWorker
│
├── Services/
│   ├── KRT.Gateway/                       # YARP + Rate Limiting + Health Checks
│   │
│   ├── KRT.Onboarding/                    # Bounded Context: Contas
│   │   ├── Api                            # Controllers, Middlewares, Program.cs
│   │   ├── Application                    # Commands, Handlers, Validators, DTOs
│   │   ├── Domain                         # Account, Enums, Events, Interfaces
│   │   ├── Infra.Data                     # EF Core, Repositories, Migrations
│   │   ├── Infra.Cache                    # Redis (ICacheService, RedisCacheService)
│   │   ├── Infra.MessageQueue             # IntegrationEvents, DomainEventHandlers
│   │   └── Infra.IoC                      # DI Registration
│   │
│   └── KRT.Payments/                      # Bounded Context: Pagamentos
│       ├── Api                            # PixController (POST, GET status, GET extrato)
│       ├── Application                    # ProcessPixCommand, FraudEngine, FraudWorker
│       ├── Domain                         # PixTransaction (state machine), FraudAnalysis
│       ├── Infra.Data                     # EF Core, Repositories (GetByStatusAsync)
│       ├── Infra.Http                     # OnboardingServiceClient (Polly)
│       └── Infra.IoC                      # DI Registration
│
└── Web/
    └── KRT.Web/                           # Angular 17 SPA
        ├── auth/                          # AuthService (Keycloak OIDC), AuthGuard
        ├── services/                      # AccountService, PaymentService
        └── environments/                  # Gateway routing config

tests/
├── KRT.UnitTests/                         # 55+ testes (Domain, Application)
└── KRT.IntegrationTests/                  # Repositories, EF Core InMemory

infra/
├── docker-compose.yml                     # 9 containers
├── init-db.sql                            # Bootstrap databases
└── keycloak/krt-bank-realm.json           # Realm + users auto-import
```

---

## Quick Start

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (Angular)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Infraestrutura

```powershell
docker compose up -d
```

Aguarde ~30s para o Keycloak inicializar. O realm é importado automaticamente.

### 2. Backend

```powershell
# Terminal 1 — Gateway (:5000)
cd src\Services\KRT.Gateway\KRT.Gateway
dotnet run

# Terminal 2 — Onboarding (:5001)
cd src\Services\KRT.Onboarding\KRT.Onboarding.Api
dotnet run --urls http://localhost:5001

# Terminal 3 — Payments (:5002)
cd src\Services\KRT.Payments\KRT.Payments.Api
dotnet run --urls http://localhost:5002
```

### 3. Frontend

```powershell
cd src\Web\KRT.Web
npm install
ng serve
```

### 4. Testes E2E

```powershell
.\test-e2e-flow.ps1
```

### 5. Keycloak (fallback manual)

```powershell
.\setup-keycloak.ps1
```

Credenciais de teste: `demo / demo123` | Admin: `admin / admin`

---

## API Endpoints

### Onboarding Service (Contas)

| Método | Endpoint | Descrição |
|---|---|---|
| `POST` | `/api/v1/accounts` | Criar conta |
| `GET` | `/api/v1/accounts/{id}` | Consultar conta (Redis cached) |
| `POST` | `/api/v1/accounts/{id}/credit` | Creditar valor |
| `POST` | `/api/v1/accounts/{id}/debit` | Debitar valor |

### Payments Service (Pix + Anti-Fraude)

| Método | Endpoint | Descrição |
|---|---|---|
| `POST` | `/api/v1/pix` | Iniciar Pix (retorna **202 Accepted**) |
| `GET` | `/api/v1/pix/{id}` | Status + fraud score + detalhes |
| `GET` | `/api/v1/pix/account/{accountId}` | Extrato Pix |

### Gateway

| Método | Endpoint | Descrição |
|---|---|---|
| `GET` | `/health` | Health check agregado dos backends |
| `*` | `/api/v1/accounts/**` | Proxy → Onboarding |
| `*` | `/api/v1/pix/**` | Proxy → Payments |

---

## Portas

| Serviço | Porta | URL |
|---|---|---|
| Angular | 4200 | http://localhost:4200 |
| Gateway (YARP) | 5000 | http://localhost:5000 |
| Onboarding API | 5001 | http://localhost:5001/swagger |
| Payments API | 5002 | http://localhost:5002/swagger |
| Keycloak | 8080 | http://localhost:8080/admin |
| PostgreSQL | 5433 | `localhost:5433` |
| Redis | 6380 | `localhost:6380` |
| Kafka | 29092 | `localhost:29092` |
| RabbitMQ (AMQP) | 5672 | — |
| RabbitMQ (Mgmt) | 15680 | http://localhost:15680 |
| Seq | 5341 | http://localhost:5341 |

---

## Fluxo Completo: Pix com Anti-Fraude

```
1. Cliente envia POST /api/v1/pix via Angular
2. Gateway: Rate Limit check → injeta CorrelationId → proxy para Payments
3. PixController: cria ProcessPixCommand → MediatR
4. ProcessPixCommandHandler: cria PixTransaction (PendingAnalysis) → 202 Accepted
5. FraudAnalysisWorker (background, polling 2s):
   a. Busca transações PendingAnalysis no banco
   b. Executa 7 regras de scoring
   c. Score < 40 → Approve → StartSaga:
      - Debit Source (HTTP → Onboarding com Polly retry)
      - Credit Destination (HTTP → Onboarding)
      - Publish PixTransferCompletedEvent (Kafka via Outbox)
      - Publish PushNotification "Pix Enviado!" (RabbitMQ)
   d. Score 40-70 → HoldForReview → Push "Pix em Análise"
   e. Score > 70 → Reject:
      - Publish FraudAnalysisRejectedEvent (Kafka)
      - Email urgente (prioridade 9) + SMS alerta (RabbitMQ)
6. Cliente consulta GET /api/v1/pix/{id} → status + fraud score
7. NotificationWorker consome filas RabbitMQ e "envia" notificações
8. Se notificação falha 3x → Dead-Letter Queue para análise
9. Todos os logs com CorrelationId no Seq (http://localhost:5341)
```

---

## Segurança

- **Keycloak** como Identity Provider (OpenID Connect)
- **JWT Bearer** em todos os endpoints dos backends
- **Rate Limiting** no Gateway (Fixed Window, 100 req/min por IP, status 429)
- **CORS** configurado para o Angular (`localhost:4200`)
- **Idempotency Key** nas transações Pix (previne duplicação)

---

## Observabilidade

| Ferramenta | URL | Função |
|---|---|---|
| **Seq** | http://localhost:5341 | Logs estruturados, filtro por CorrelationId |
| **RabbitMQ Management** | http://localhost:15680 | Filas, mensagens, DLQ (krt / REDACTED_RABBITMQ_PASSWORD) |
| **Keycloak Admin** | http://localhost:8080/admin | Users, tokens, realm (admin / admin) |
| **Health Checks** | http://localhost:5000/health | Status agregado dos backends |

---

## Testes

```powershell
# Unit + Integration Tests (55+)
dotnet test

# E2E Flow (9 testes automatizados)
.\test-e2e-flow.ps1
```

| Suite | Testes | Cobertura |
|---|---|---|
| Unit Tests | 55+ | Domain (Account, PixTransaction), Validators |
| Integration Tests | 8+ | Repositories (EF InMemory) |
| E2E Tests | 9 | Health, Auth, CRUD, Cache, Saldo, Seq |

---

## Licença

Projeto acadêmico — KRT Bank © 2026.
