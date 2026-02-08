# 🏦 KRT Bank — Digital Banking Platform

Plataforma bancária digital completa, construída com **Microservices**, **.NET 8**, **Angular 17** e **11 containers Docker**.

> **Stack completa rodando com um único comando:** `docker-compose up -d --build`

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Stack Tecnológica](#-stack-tecnológica)
- [Pré-requisitos](#-pré-requisitos)
- [Quick Start (Docker)](#-quick-start-docker)
- [Desenvolvimento Local](#-desenvolvimento-local)
- [URLs e Credenciais](#-urls-e-credenciais)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [APIs e Endpoints](#-apis-e-endpoints)
- [Testes](#-testes)
- [Observabilidade](#-observabilidade)
- [Segurança e Autenticação](#-segurança-e-autenticação)
- [Containers Docker](#-containers-docker)
- [Troubleshooting](#-troubleshooting)

---

## 🔍 Visão Geral

O KRT Bank é um sistema bancário digital que oferece:

- **Onboarding** — Criação de contas, KYC, autenticação JWT via Keycloak
- **Pagamentos** — PIX (instantâneo, agendado, QR Code), boletos, recargas
- **Anti-fraude** — Engine de análise em tempo real com scoring e regras configuráveis
- **Cartões** — Cartões virtuais com limite configurável
- **Investimentos** — Simulação de investimentos e metas financeiras
- **Seguros** — Contratação e gestão de apólices
- **Notificações** — Email, SMS e push via RabbitMQ
- **Dashboard** — Visão consolidada com gráficos Chart.js e extrato
- **Chatbot** — Assistente virtual integrado com FAB flutuante
- **Contatos** — Gerenciamento de favoritos para transferências rápidas
- **Admin** — Painel administrativo com métricas, alertas de fraude e revisão de contas

---

## 🏗 Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                    Angular 17 (SPA)                         │
│              Material Design + Chart.js + RxJS              │
│                   http://localhost:4200                      │
└──────────────────────────┬──────────────────────────────────┘
                           │
                ┌──────────▼──────────┐
                │  Gateway (YARP)     │
                │  Rate Limiting      │
                │  http://localhost:5000│
                └────┬────────────┬───┘
                     │            │
          ┌──────────▼──┐  ┌─────▼───────────┐
          │ Onboarding  │  │  Payments API   │
          │  API :5001  │  │    :5002        │
          │ ─────────── │  │ ─────────────── │
          │ Contas      │  │ PIX + Boletos   │
          │ Auth (JWT)  │  │ Cartões + Metas │
          │ KYC         │  │ Seguros + Chat  │
          │ Keycloak    │  │ Anti-fraude     │
          └──────┬──────┘  └──┬──────┬────┬──┘
                 │            │      │    │
     ┌───────────┼────────────┼──────┼────┼────────────┐
     │           │            │      │    │             │
  ┌──▼───┐  ┌───▼──┐  ┌──────▼┐  ┌─▼──┐ │  ┌────────┐│
  │Postgre│  │Redis │  │Rabbit │  │Kafka│ │  │Keycloak││
  │ SQL   │  │Cache │  │  MQ   │  │    │ │  │  IAM   ││
  │ :5433 │  │:6380 │  │:15680 │  │:29092│  │ :8080  ││
  └───────┘  └──────┘  └───────┘  └────┘ │  └────────┘│
                                   ┌──────▼──┐         │
                                   │  SEQ    │         │
                                   │ (Logs)  │         │
                                   │ :8081   │         │
                                   └─────────┘         │
     └─────────────────────────────────────────────────┘
                  Docker Compose Network
```

---

## 🛠 Stack Tecnológica

| Camada | Tecnologias |
|--------|-------------|
| **Frontend** | Angular 17, Angular Material 17, Chart.js, RxJS, TypeScript |
| **API Gateway** | ASP.NET 8 + YARP (Reverse Proxy), Rate Limiting, Health Checks |
| **Backend** | ASP.NET 8 Web API, MediatR (CQRS), Rich Domain Entities, Polly (Resilience) |
| **Anti-fraude** | Engine customizada com scoring por regras (frequência, destino, valor, horário) |
| **Persistência** | PostgreSQL 16 + Entity Framework Core 8, Redis 7 (cache distribuído) |
| **Mensageria** | RabbitMQ 3 (notificações, workers), Apache Kafka (eventos de domínio) |
| **Autenticação** | Keycloak 23 (OpenID Connect), JWT Bearer Tokens |
| **Real-time** | SignalR (atualização de saldo em tempo real) |
| **Logging** | Serilog + SEQ (Structured Logging com UI) |
| **Padrões** | CQRS, Outbox Pattern, Saga (compensação), Circuit Breaker, Domain Events |
| **Testes** | xUnit (91 testes .NET), Karma/Jasmine (17 testes Angular), E2E scripts (9 testes) |
| **Containerização** | Docker, Docker Compose (11 containers), Multi-stage builds, Nginx |

---

## 📦 Pré-requisitos

- **Docker Desktop** ≥ 4.0 (com Docker Compose V2)
- **RAM disponível:** ≥ 4 GB para os containers

Para desenvolvimento local (opcional):
- .NET SDK 8.0
- Node.js ≥ 18
- Angular CLI 17 (`npm install -g @angular/cli@17`)

---

## 🚀 Quick Start (Docker)

**Subir tudo com um comando:**

```bash
git clone <repo-url> krt-bank
cd krt-bank
docker-compose up -d --build
```

Aguarde ~2-3 minutos (primeiro build). Depois acesse:

| Serviço | URL |
|---------|-----|
| **Aplicação** | http://localhost:4200 |
| **Swagger Onboarding** | http://localhost:5001/swagger |
| **Swagger Payments** | http://localhost:5002/swagger |

**Configuração inicial do Keycloak** (necessário apenas na primeira vez):
```powershell
# Obter token admin
$token = (Invoke-RestMethod -Uri "http://localhost:8080/realms/master/protocol/openid-connect/token" `
  -Method POST -ContentType "application/x-www-form-urlencoded" `
  -Body "grant_type=password&client_id=admin-cli&username=admin&password=admin").access_token

# Criar realm
Invoke-RestMethod -Uri "http://localhost:8080/admin/realms" -Method POST `
  -Headers @{Authorization="Bearer $token"} -ContentType "application/json" `
  -Body '{"realm":"krt-bank","enabled":true}'

# Criar client
$client = '{"clientId":"krt-bank-app","enabled":true,"publicClient":true,"directAccessGrantsEnabled":true,"redirectUris":["http://localhost:4200/*","http://localhost:5000/*"],"webOrigins":["http://localhost:4200","http://localhost:5000"],"protocol":"openid-connect"}'
Invoke-RestMethod -Uri "http://localhost:8080/admin/realms/krt-bank/clients" -Method POST `
  -Headers @{Authorization="Bearer $token"} -ContentType "application/json" -Body $client
```

> **Nota:** Os dados do Keycloak são persistentes via volume Docker. Após a configuração inicial, sobrevivem a `docker-compose down` + `up`. Só são perdidos com `docker-compose down -v`.

**Parar tudo:**
```bash
docker-compose down       # Preserva dados
docker-compose down -v    # Remove dados (volumes)
```

---

## 💻 Desenvolvimento Local

Para desenvolvimento com hot-reload, rode a **infraestrutura no Docker** e as **APIs + Angular localmente**:

### 1. Subir infraestrutura
```bash
docker-compose up -d postgres redis rabbitmq kafka zookeeper keycloak seq
```

### 2. APIs com hot-reload (cada uma em um terminal)
```bash
# Terminal 1 — Onboarding API
cd src/Services/KRT.Onboarding/KRT.Onboarding.Api
dotnet watch run

# Terminal 2 — Payments API
cd src/Services/KRT.Payments/KRT.Payments.Api
dotnet watch run

# Terminal 3 — Gateway
cd src/Services/KRT.Gateway/KRT.Gateway
dotnet run
```

### 3. Angular (outro terminal)
```bash
cd src/Web/KRT.Web
npm install
ng serve
```

> **Nota:** Os `appsettings.json` já apontam para `localhost` nas portas corretas dos containers (5433 para PostgreSQL, 6380 para Redis, etc).

---

## 🔗 URLs e Credenciais

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **Frontend Angular** | http://localhost:4200 | — |
| **API Gateway (YARP)** | http://localhost:5000 | — |
| **Payments API (Swagger)** | http://localhost:5002/swagger | — |
| **Onboarding API (Swagger)** | http://localhost:5001/swagger | — |
| **SEQ (Logs UI)** | http://localhost:8081 | — |
| **SEQ (Ingestão)** | http://localhost:5341 | — |
| **Keycloak Admin** | http://localhost:8080/admin | `admin` / `admin` |
| **RabbitMQ Management** | http://localhost:15680 | `krt` / `REDACTED_RABBITMQ_PASSWORD` |
| **PostgreSQL** | localhost:5433 | `krt` / `REDACTED_DB_PASSWORD` (db: `krtbank`) |
| **Redis** | localhost:6380 | — |
| **Kafka** | localhost:29092 | — |

---

## 📁 Estrutura do Projeto

```
krt-bank/
├── docker-compose.yml                    # Stack completa (11 containers)
├── docker-compose.observability.yml      # Prometheus + Grafana (opcional)
├── run-all-tests.ps1                     # Script de execução de todos os testes
│
├── src/
│   ├── BuildingBlocks/                   # Bibliotecas compartilhadas
│   │   ├── KRT.BuildingBlocks.Domain/        # Result pattern, Value Objects, DomainEvent
│   │   ├── KRT.BuildingBlocks.EventBus/      # Kafka abstractions
│   │   ├── KRT.BuildingBlocks.Infrastructure/ # EF base, Outbox pattern
│   │   └── KRT.BuildingBlocks.MessageBus/    # RabbitMQ (NotificationWorker)
│   │
│   ├── Services/
│   │   ├── KRT.Gateway/                  # YARP reverse proxy + health checks
│   │   │   └── KRT.Gateway/
│   │   │       ├── appsettings.json          # Routes (localhost)
│   │   │       └── appsettings.Docker.json   # Routes (container names)
│   │   │
│   │   ├── KRT.Onboarding/              # Contas, Auth, KYC
│   │   │   ├── KRT.Onboarding.Api/          # Controllers (Auth, Accounts)
│   │   │   ├── KRT.Onboarding.Application/  # Commands (CQRS), Keycloak Service
│   │   │   ├── KRT.Onboarding.Domain/       # Entities, Value Objects
│   │   │   ├── KRT.Onboarding.Infra.Data/   # EF Core, Repositories
│   │   │   ├── KRT.Onboarding.Infra.Cache/  # Redis cache
│   │   │   ├── KRT.Onboarding.Infra.IoC/    # Dependency Injection
│   │   │   └── KRT.Onboarding.Infra.MessageQueue/ # RabbitMQ publisher
│   │   │
│   │   └── KRT.Payments/                # PIX, Boletos, Cartões, Seguros, Metas
│   │       ├── KRT.Payments.Api/            # 10 Controllers
│   │       ├── KRT.Payments.Application/    # CQRS, FraudAnalysisEngine, Workers
│   │       ├── KRT.Payments.Domain/         # Entities, Interfaces
│   │       ├── KRT.Payments.Infra.Data/     # EF Core, 15 tabelas
│   │       ├── KRT.Payments.Infra.Http/     # HttpClient (inter-service)
│   │       └── KRT.Payments.Infra.IoC/      # DI, Polly, Circuit Breaker
│   │
│   └── Web/
│       └── KRT.Web/                      # Angular 17 SPA
│           ├── src/app/
│           │   ├── core/                     # Services, Guards, Interceptors
│           │   ├── modules/                  # Feature modules
│           │   │   ├── dashboard/            # Dashboard com saldo e gráficos
│           │   │   ├── onboarding/           # Registro e login
│           │   │   ├── payments/             # PIX, Boleto, Recarga, Chaves PIX
│           │   │   └── statement/            # Extrato e comprovantes
│           │   ├── pages/                    # Chatbot, Charts, Profile, Cards
│           │   └── shared/                   # Components (chat-dialog, sidebar, toast)
│           ├── Dockerfile                    # Multi-stage (Node build → Nginx serve)
│           └── nginx.conf                    # SPA routing + gzip + cache
│
├── tests/
│   ├── KRT.Payments.UnitTests/           # 83 testes unitários (xUnit)
│   └── KRT.Payments.IntegrationTests/    # 8 testes de integração (xUnit)
│
├── infra/                                # Prometheus, Grafana configs
└── scripts/                              # Keycloak setup, E2E scripts
```

---

## 📡 APIs e Endpoints

### Onboarding API (`:5001`)

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/auth/register` | Criar conta (Keycloak + PostgreSQL) |
| POST | `/api/v1/auth/login` | Login (retorna JWT + dados da conta) |
| GET | `/api/v1/accounts/{id}` | Dados da conta |
| GET | `/api/v1/accounts/by-document/{cpf}` | Buscar conta por CPF |
| GET | `/api/v1/accounts/{id}/balance` | Saldo disponível |
| POST | `/api/v1/accounts/{id}/debit` | Debitar conta |
| POST | `/api/v1/accounts/{id}/credit` | Creditar conta |

### Payments API (`:5002`)

#### PIX
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/pix` | Enviar PIX (análise anti-fraude assíncrona) |
| GET | `/api/v1/pix/{id}` | Status da transação + fraud score |
| GET | `/api/v1/pix/account/{accountId}` | Histórico PIX (paginado) |
| POST | `/api/v1/pix/qrcode/generate` | Gerar QR Code PIX |
| POST | `/api/v1/pix/qrcode/image` | Imagem do QR Code |
| GET | `/api/v1/pix/receipt/{id}` | Comprovante |
| POST | `/api/v1/pix/receipt` | Gerar PDF do comprovante |
| GET | `/api/v1/pix/limits/{accountId}` | Limites PIX |
| PUT | `/api/v1/pix/limits/{accountId}` | Atualizar limites |

#### PIX Agendado
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/scheduled-pix` | Agendar PIX |
| GET | `/api/v1/scheduled-pix/account/{accountId}` | Listar agendamentos |
| POST | `/api/v1/scheduled-pix/{id}/execute` | Executar agendamento |
| POST | `/api/v1/scheduled-pix/{id}/cancel` | Cancelar |
| POST | `/api/v1/scheduled-pix/{id}/pause` | Pausar |
| POST | `/api/v1/scheduled-pix/{id}/resume` | Retomar |

#### Boleto
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/boleto/generate` | Gerar boleto |
| POST | `/api/v1/boleto/pay/{id}` | Pagar boleto |
| POST | `/api/v1/boleto/pay-barcode` | Pagar por código de barras |
| GET | `/api/v1/boleto/account/{accountId}` | Listar boletos |
| GET | `/api/v1/boleto/{id}` | Detalhes do boleto |

#### Contatos
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/contacts/{accountId}` | Listar contatos |
| POST | `/api/v1/contacts/{accountId}` | Adicionar contato |
| POST | `/api/v1/contacts/{accountId}/{contactId}/favorite` | Favoritar |
| DELETE | `/api/v1/contacts/{accountId}/{contactId}` | Remover contato |

#### Chatbot
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/chatbot/message` | Enviar mensagem |
| GET | `/api/v1/chatbot/suggestions` | Sugestões rápidas |

#### Admin
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/admin/dashboard` | Dashboard administrativo |
| GET | `/api/v1/admin/accounts/pending` | Contas pendentes |
| POST | `/api/v1/admin/accounts/{id}/review` | Revisar conta |
| GET | `/api/v1/admin/fraud/alerts` | Alertas de fraude |
| POST | `/api/v1/admin/fraud/alerts/{id}/action` | Ação sobre alerta |
| GET | `/api/v1/admin/metrics` | Métricas do sistema |

#### Dashboard
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/dashboard/summary/{accountId}` | Resumo consolidado |
| GET | `/api/v1/dashboard/balance-history/{accountId}` | Histórico de saldo |

> 📖 Documentação interativa completa no Swagger: http://localhost:5002/swagger

---

## 🧪 Testes

**117 testes no total:**

| Suite | Framework | Quantidade | Comando |
|-------|-----------|------------|---------|
| .NET Unitários | xUnit | 83 | `dotnet test tests/KRT.Payments.UnitTests` |
| .NET Integração | xUnit | 8 | `dotnet test tests/KRT.Payments.IntegrationTests` |
| Angular | Karma + Jasmine | 17 | `cd src/Web/KRT.Web && npx ng test --watch=false --browsers=ChromeHeadless` |
| E2E | PowerShell Script | 9 | `.\test-e2e-flow.ps1` (requer Docker rodando) |

**Executar todos os testes:**
```powershell
# Script integrado
.\run-all-tests.ps1

# Ou individualmente:
dotnet test KRT.sln                                                        # 91 .NET
cd src/Web/KRT.Web && npx ng test --watch=false --browsers=ChromeHeadless  # 17 Angular
.\test-e2e-flow.ps1                                                        # 9 E2E
```

---

## 📊 Observabilidade

### SEQ (Logs Estruturados) — Incluído na stack

Todos os serviços enviam logs estruturados via Serilog para o SEQ:
- **UI:** http://localhost:8081
- **Ingestão:** http://localhost:5341

Filtros úteis:
```
Application = 'KRT.Payments.Api'
SourceContext like 'FraudAnalysis%'
@Level = 'Error'
```

### Prometheus + Grafana (Opcional)

```bash
docker-compose -f docker-compose.observability.yml up -d
```

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| Grafana | http://localhost:3000 | `admin` / `REDACTED_GRAFANA_PASSWORD` |
| Prometheus | http://localhost:9090 | — |
| AlertManager | http://localhost:9093 | — |

---

## 🔐 Segurança e Autenticação

### Keycloak (Identity Provider)
- **Realm:** `krt-bank`
- **Client:** `krt-bank-app` (public, direct access grants)
- **Fluxo:** Registration → Keycloak user + PostgreSQL account (atômico)
- **Login:** Keycloak authentication → JWT access + refresh tokens
- **Volume persistente:** `keycloak-data` (dados sobrevivem reinícios)

### JWT Bearer Tokens
- Todas as APIs protegidas com `[Authorize]`
- Token propagado via `AuthInterceptor` no Angular
- Refresh token automático

### Anti-fraude (Payments)
Engine de scoring com regras configuráveis:
- `HIGH_FREQUENCY` — Múltiplas transações na última hora
- `REPEATED_DESTINATION` — Mesmo destino repetido
- `HIGH_VALUE` — Valor acima do threshold
- `OFF_HOURS` — Transações em horários incomuns

Thresholds:
- Score < 80 → **Aprovado**
- Score 80–150 → **Em Revisão**
- Score > 150 → **Rejeitado**

Fluxo PIX: `Pending → Fraud Analysis → Debit → Credit → Completed` (com Saga para compensação em caso de falha)

---

## 🐳 Containers Docker

| # | Container | Imagem | Porta(s) | Healthcheck | Volume |
|---|-----------|--------|----------|-------------|--------|
| 1 | krt-postgres | postgres:16-alpine | 5433 | `pg_isready` | `postgres-data` |
| 2 | krt-redis | redis:7-alpine | 6380 | `redis-cli ping` | — |
| 3 | krt-rabbitmq | rabbitmq:3-management | 5672, 15680 | `rabbitmq-diagnostics` | — |
| 4 | krt-kafka | cp-kafka:7.5.0 | 9092, 29092 | — | — |
| 5 | krt-zookeeper | cp-zookeeper:7.5.0 | 32181 | — | — |
| 6 | krt-keycloak | keycloak:23.0 | 8080 | — | `keycloak-data` |
| 7 | krt-seq | datalust/seq:2024.1 | 5341, 8081 | — | `seq-data` |
| 8 | krt-onboarding | .NET 8 (build local) | 5001 | `/health` | — |
| 9 | krt-payments | .NET 8 (build local) | 5002 | `/health` | — |
| 10 | krt-gateway | .NET 8 + YARP | 5000 | `/health` | — |
| 11 | krt-web | Node 20 → Nginx | 4200 | `/nginx-health` | — |

### Comunicação inter-serviço
- **Payments → Onboarding:** HTTP via `Services__OnboardingUrl` (debit/credit)
- **APIs → PostgreSQL:** Connection string via environment variables
- **APIs → Redis:** Cache distribuído para sessions e rate limiting
- **APIs → RabbitMQ:** Notificações assíncronas
- **APIs → Kafka:** Eventos de domínio (PIX completed, fraud detected)
- **APIs → SEQ:** Logs estruturados via Serilog

---

## 🔧 Troubleshooting

### Container crashando (`Restarting`)
```bash
docker logs <container-name> --tail 30
```

### Keycloak perdeu os dados
Se usou `docker-compose down -v`, o volume foi removido. Recrie seguindo a seção Quick Start.

### APIs não conectam ao PostgreSQL
Os containers usam portas internas padrão (PostgreSQL `5432`, Redis `6379`). As portas externas (`5433`, `6380`) são só para acesso local. Verifique se o `docker-compose.yml` aponta para nomes dos containers (`postgres`, `redis`), não `localhost`.

### PIX ficando em "UnderReview" ou "Rejected"
A engine de anti-fraude pode rejeitar transações frequentes. Ajuste os thresholds em:
```
src/Services/KRT.Payments/KRT.Payments.Application/Services/FraudAnalysisEngine.cs
```

### Gateway retorna 502
O Gateway YARP precisa que as APIs estejam respondendo. Em Docker, usa `appsettings.Docker.json` com nomes dos containers (`payments-api:80`, `onboarding-api:80`).

### Angular — erro NG0701 (Missing locale data)
Não use `:'pt-BR'` nos currency pipes do Angular. Use apenas `currency:'BRL':'symbol':'1.2-2'`.

### SEQ crashando
```bash
docker volume rm krt-bank_seq-data
docker-compose up -d seq
```

### Nginx: `unknown directive "ï»¿server"`
O `nginx.conf` tem BOM (Byte Order Mark). Reescreva sem BOM:
```powershell
[System.IO.File]::WriteAllText("path\nginx.conf", $content, (New-Object System.Text.UTF8Encoding $false))
```

---

## 📊 Banco de Dados

**15 tabelas no PostgreSQL (`krtbank`):**

| Contexto | Tabelas |
|----------|---------|
| **Onboarding** | Accounts, OutboxMessages |
| **Payments** | PixTransactions, PixLimits, PixContacts, ScheduledPixTransactions |
| **Financeiro** | Boletos, StatementEntries, VirtualCards |
| **Produtos** | FinancialGoals, InsurancePolicies |
| **Usuário** | UserProfiles, UserPointsTable, KycProfiles, Notifications |

---

## 📄 Licença

Este projeto é de uso acadêmico / portfólio.

---

> **KRT Bank** — Plataforma completa de banking digital desenvolvida com .NET 8, Angular 17, e 11 containers Docker.
