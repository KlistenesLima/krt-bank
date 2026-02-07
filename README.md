# KRT Bank — Plataforma Bancária Distribuída

Sistema bancário completo desenvolvido com **Clean Architecture**, **DDD**, **CQRS**, e microsserviços em **.NET 8**.

## Arquitetura

```
┌─────────────┐     ┌──────────────────┐     ┌──────────────────┐
│   Angular    │────▶│   YARP Gateway   │────▶│   Onboarding     │
│   Frontend   │     │   :5000           │     │   Service :5001  │
│   :4200      │     │  • Rate Limiting  │     │  • Accounts      │
└─────────────┘     │  • CorrelationId  │     │  • Redis Cache   │
                    │  • Health Checks  │     │  • Kafka Outbox  │
                    └──────────────────┘     └──────────────────┘
                              │
                              └─────────────▶┌──────────────────┐
                                             │   Payments       │
                                             │   Service :5002  │
                                             │  • Pix Saga      │
                                             │  • Polly Retry   │
                                             │  • Circuit Break │
                                             │  • Kafka Outbox  │
                                             └──────────────────┘
```

## Stack Tecnológica

| Camada           | Tecnologia                                      |
|------------------|------------------------------------------------|
| Frontend         | Angular 17 + Angular Material                  |
| API Gateway      | YARP Reverse Proxy + Rate Limiting              |
| Backend          | .NET 8 (Clean Architecture + DDD)               |
| CQRS             | MediatR + FluentValidation Pipeline             |
| Autenticação     | Keycloak (JWT Bearer)                           |
| Banco de Dados   | PostgreSQL 15 (database per service)            |
| Cache            | Redis (StackExchange.Redis)                     |
| Mensageria       | Apache Kafka (Outbox Pattern)                   |
| Resiliência      | Polly (Retry + Circuit Breaker)                 |
| Observabilidade  | Serilog → Seq + CorrelationId E2E              |
| Containers       | Docker Compose                                  |

## Estrutura de Projetos

```
src/
├── BuildingBlocks/
│   ├── KRT.BuildingBlocks.Domain          # Entity, AggregateRoot, ValueObject, DomainEvent
│   ├── KRT.BuildingBlocks.EventBus        # IEventBus, KafkaEventBus, IntegrationEvent
│   └── KRT.BuildingBlocks.Infrastructure  # Repository<T>, OutboxProcessor, UoW
│
├── Services/
│   ├── KRT.Gateway/                       # YARP + Rate Limiting + Health Checks
│   ├── KRT.Onboarding/                    # Contexto de Contas
│   │   ├── Api                            # Controllers, Middlewares
│   │   ├── Application                    # Commands, Handlers, Validators
│   │   ├── Domain                         # Account, Enums, Interfaces
│   │   ├── Infra.Data                     # EF Core, Repositories
│   │   ├── Infra.Cache                    # Redis (ICacheService)
│   │   ├── Infra.MessageQueue             # Integration Events
│   │   └── Infra.IoC                      # DI Registration
│   └── KRT.Payments/                      # Contexto de Pagamentos
│       ├── Api                            # Controllers, Middlewares
│       ├── Application                    # ProcessPixCommand, Saga Handler
│       ├── Domain                         # PixTransaction, Payment
│       ├── Infra.Data                     # EF Core, Repositories
│       ├── Infra.Http                     # OnboardingServiceClient (Polly)
│       └── Infra.IoC                      # DI Registration
│
└── Web/
    └── KRT.Web/                           # Angular SPA
```

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (para o Angular)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Quick Start

### 1. Subir a Infraestrutura

```powershell
docker compose up -d
```

Containers: PostgreSQL, Redis, Kafka, Zookeeper, Seq, Keycloak.

### 2. Configurar Keycloak (se necessário)

O realm `krt-bank` é importado automaticamente via `--import-realm`.
Se precisar reconfigurar manualmente:

```powershell
.\setup-keycloak.ps1
```

Credenciais de teste:
- **Admin Console:** http://localhost:8080/admin → `admin / admin`
- **Usuário demo:** `demo / demo123`

### 3. Iniciar os Backends

```powershell
# Terminal 1 — Gateway
cd src\Services\KRT.Gateway\KRT.Gateway
dotnet run

# Terminal 2 — Onboarding
cd src\Services\KRT.Onboarding\KRT.Onboarding.Api
dotnet run --urls http://localhost:5001

# Terminal 3 — Payments
cd src\Services\KRT.Payments\KRT.Payments.Api
dotnet run --urls http://localhost:5002
```

### 4. Iniciar o Frontend

```powershell
cd src\Web\KRT.Web
npm install
ng serve
```

Acesse: http://localhost:4200

### 5. Testar E2E

```powershell
.\test-e2e-flow.ps1
```

## Portas

| Serviço       | Porta  | URL                              |
|---------------|--------|----------------------------------|
| Angular       | 4200   | http://localhost:4200             |
| Gateway       | 5000   | http://localhost:5000             |
| Onboarding    | 5001   | http://localhost:5001/swagger     |
| Payments      | 5002   | http://localhost:5002/swagger     |
| Keycloak      | 8080   | http://localhost:8080/admin       |
| PostgreSQL    | 5433   | localhost:5433                   |
| Redis         | 6380   | localhost:6380                   |
| Kafka         | 29092  | localhost:29092                  |
| Seq           | 5341   | http://localhost:5341             |

## Patterns Implementados

### Clean Architecture + DDD
- **Domain Layer**: Entidades ricas (Account com Debit/Credit/Block), Value Objects, Domain Events
- **Application Layer**: CQRS via MediatR, FluentValidation Pipeline Behavior
- **Infrastructure Layer**: EF Core, Redis, Kafka, HTTP Clients

### Saga Pattern (Pix)
```
ProcessPixCommand → Debit Source → Credit Destination → Publish Event
                       ↓ (falha)
                  Compensação: Credit Source (rollback)
```

### Outbox Pattern
```
Command Handler → OutboxMessage (same DB transaction)
OutboxProcessor → Poll → KafkaEventBus → Topics
```

### Polly Resilience (Payments → Onboarding)
- **Retry**: 3 tentativas, backoff exponencial (1s, 2s, 4s)
- **Circuit Breaker**: Abre após 5 falhas, 30s aberto

### Redis Cache (Onboarding)
- `GET /accounts/{id}` → Cache Redis (5 min TTL)
- Invalidação automática em Debit/Credit

### Observabilidade
- **Serilog → Seq** em todos os serviços
- **CorrelationId** propagado: Gateway → Backend → HttpClient → Kafka headers
- Console template: `[HH:mm:ss LVL] [{CorrelationId}] Message`

## Segurança

- **Keycloak** como Identity Provider (OIDC)
- **JWT Bearer** em todos os backends
- **Rate Limiting** no Gateway (100 req/min por IP)
- **CORS** configurado para o Angular

## Licença

Projeto acadêmico — KRT Bank 2026.
