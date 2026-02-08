# 🏦 KRT Bank — Digital Banking Platform

Plataforma bancária digital completa, construída com **Microservices**, **.NET 8**, **Angular 17** e **11 containers Docker**.

> **Stack completa rodando com um único comando:** `docker-compose up -d --build`

---

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Stack Tecnológica](#stack-tecnológica)
- [Pré-requisitos](#pré-requisitos)
- [Quick Start (Docker)](#quick-start-docker)
- [Desenvolvimento Local](#desenvolvimento-local)
- [URLs e Credenciais](#urls-e-credenciais)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [APIs e Endpoints](#apis-e-endpoints)
- [Testes](#testes)
- [Observabilidade](#observabilidade)
- [Troubleshooting](#troubleshooting)

---

## Visão Geral

O KRT Bank é um sistema bancário digital que oferece:

- **Onboarding** — Criação de contas, KYC, autenticação JWT
- **Pagamentos** — PIX (instantâneo, agendado, QR Code), boletos, transferências
- **Cartões** — Cartões virtuais com limite configurável
- **Investimentos** — Simulação de investimentos e metas financeiras
- **Seguros** — Contratação e gestão de apólices
- **Notificações** — Email, SMS e push via RabbitMQ
- **Dashboard** — Visão consolidada com gráficos e extrato
- **Chat** — Chatbot integrado para atendimento

---

## Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                    Angular 17 (SPA)                         │
│                   http://localhost:4200                      │
└──────────────────────────┬──────────────────────────────────┘
                           │
                ┌──────────▼──────────┐
                │   Gateway (YARP)    │
                │  http://localhost:5000│
                └────┬────────────┬───┘
                     │            │
          ┌──────────▼──┐  ┌─────▼───────────┐
          │ Onboarding  │  │   Payments API   │
          │  API :5001  │  │     :5002        │
          └──────┬──────┘  └──┬──────┬────┬───┘
                 │            │      │    │
     ┌───────────┼────────────┼──────┼────┼────────────┐
     │           │            │      │    │             │
  ┌──▼───┐  ┌───▼──┐  ┌──────▼┐  ┌─▼──┐ │  ┌────────┐│
  │Postgre│  │Redis │  │Rabbit │  │Kafka│ │  │Keycloak││
  │ SQL   │  │      │  │  MQ   │  │    │ │  │        ││
  └───────┘  └──────┘  └───────┘  └────┘ │  └────────┘│
                                     ┌────▼───┐        │
                                     │  SEQ   │        │
                                     │ (Logs) │        │
                                     └────────┘        │
     └─────────────────────────────────────────────────┘
                  Docker Compose Network
```

---

## Stack Tecnológica

| Camada | Tecnologias |
|--------|-------------|
| **Frontend** | Angular 17, Angular Material 17, Chart.js, RxJS |
| **API Gateway** | ASP.NET 8 + YARP (Reverse Proxy), Rate Limiting |
| **Backend** | ASP.NET 8 Web API, MediatR (CQRS), Rich Domain Entities |
| **Persistência** | PostgreSQL 16, Entity Framework Core 8, Redis 7 (cache) |
| **Mensageria** | RabbitMQ 3 (notificações), Apache Kafka (eventos de domínio) |
| **Autenticação** | Keycloak 23 (OpenID Connect), JWT Bearer Tokens |
| **Real-time** | SignalR (atualizações de saldo) |
| **Logging** | Serilog + SEQ (Structured Logging com UI em http://localhost:8081) |
| **Testes** | xUnit (83 unitários + 8 integração), Cypress (E2E) |
| **Containerização** | Docker, Docker Compose (11 containers) |

---

## Pré-requisitos

- **Docker Desktop** ≥ 4.0 (com Docker Compose V2)
- **RAM disponível:** ≥ 4 GB para os containers

Para desenvolvimento local (opcional):
- .NET SDK 8.0
- Node.js ≥ 18
- Angular CLI 17 (`npm install -g @angular/cli@17`)

---

## Quick Start (Docker)

**Subir tudo com um comando:**

```bash
git clone <repo-url> krt-bank
cd krt-bank
docker-compose up -d --build
```

Aguarde ~2-3 minutos (primeiro build). Depois acesse:

- **App:** http://localhost:4200
- **Swagger Payments:** http://localhost:5002/swagger
- **Swagger Onboarding:** http://localhost:5001/swagger

**Parar tudo:**
```bash
docker-compose down
```

**Parar e limpar dados:**
```bash
docker-compose down -v
```

---

## Desenvolvimento Local

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

## URLs e Credenciais

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **Frontend Angular** | http://localhost:4200 | — |
| **API Gateway (YARP)** | http://localhost:5000 | — |
| **Payments API (Swagger)** | http://localhost:5002/swagger | — |
| **Onboarding API (Swagger)** | http://localhost:5001/swagger | — |
| **SEQ (Logs UI)** | http://localhost:8081 | — |
| **SEQ (API)** | http://localhost:5341 | — |
| **Keycloak Admin** | http://localhost:8080/admin | admin / admin |
| **RabbitMQ Management** | http://localhost:15680 | krt / REDACTED_RABBITMQ_PASSWORD |
| **PostgreSQL** | localhost:5433 | krt / REDACTED_DB_PASSWORD (db: krtbank) |
| **Redis** | localhost:6380 | — |
| **Kafka** | localhost:29092 | — |
| **Zookeeper** | localhost:32181 | — |

---

## Estrutura do Projeto

```
krt-bank/
├── docker-compose.yml                    # Stack completa (11 containers)
├── docker-compose.observability.yml      # Prometheus + Grafana (opcional)
│
├── src/
│   ├── BuildingBlocks/                   # Shared libraries
│   │   ├── KRT.BuildingBlocks.Domain/        # Result pattern, Value Objects
│   │   ├── KRT.BuildingBlocks.EventBus/      # Kafka abstractions
│   │   ├── KRT.BuildingBlocks.Infrastructure/ # EF base, Outbox pattern
│   │   └── KRT.BuildingBlocks.MessageBus/    # RabbitMQ (NotificationWorker)
│   │
│   ├── Services/
│   │   ├── KRT.Gateway/                  # YARP reverse proxy
│   │   │   └── KRT.Gateway/
│   │   │       ├── appsettings.json          # Routes + Clusters (localhost)
│   │   │       └── appsettings.Docker.json   # Routes + Clusters (container names)
│   │   │
│   │   ├── KRT.Onboarding/              # Account creation, Auth, KYC
│   │   │   ├── KRT.Onboarding.Api/
│   │   │   ├── KRT.Onboarding.Application/
│   │   │   ├── KRT.Onboarding.Domain/
│   │   │   ├── KRT.Onboarding.Infra.Data/
│   │   │   ├── KRT.Onboarding.Infra.Cache/
│   │   │   ├── KRT.Onboarding.Infra.IoC/
│   │   │   └── KRT.Onboarding.Infra.MessageQueue/
│   │   │
│   │   └── KRT.Payments/                # PIX, Boletos, Cards, Insurance, Goals
│   │       ├── KRT.Payments.Api/
│   │       ├── KRT.Payments.Application/
│   │       ├── KRT.Payments.Domain/
│   │       ├── KRT.Payments.Infra.Data/
│   │       ├── KRT.Payments.Infra.Http/
│   │       └── KRT.Payments.Infra.IoC/
│   │
│   └── Web/
│       └── KRT.Web/                      # Angular 17 SPA
│           ├── src/app/
│           │   ├── core/                     # Services, Guards, Interceptors
│           │   ├── modules/                  # Feature modules (dashboard, pix, etc)
│           │   └── shared/                   # Components reutilizáveis
│           ├── Dockerfile                    # Multi-stage (Node build → Nginx serve)
│           └── nginx.conf                    # SPA routing + gzip + cache
│
├── tests/
│   ├── KRT.Payments.UnitTests/           # 83 testes unitários
│   └── KRT.Payments.IntegrationTests/    # 8 testes de integração
│
├── infra/                                # Prometheus, Grafana configs
└── scripts/                              # Keycloak setup, E2E scripts
```

---

## APIs e Endpoints

### Onboarding API (`:5001`)

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/auth/register` | Criar conta |
| POST | `/api/v1/auth/login` | Login (retorna JWT) |
| POST | `/api/v1/auth/refresh` | Refresh token |
| GET | `/api/v1/accounts/{id}` | Dados da conta |
| GET | `/api/v1/accounts/{id}/balance` | Saldo |

### Payments API (`:5002`)

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/v1/pix/send` | Enviar PIX |
| GET | `/api/v1/pix/keys/{accountId}` | Chaves PIX |
| POST | `/api/v1/pix/keys` | Registrar chave PIX |
| POST | `/api/v1/boleto/pay` | Pagar boleto |
| POST | `/api/v1/boleto/generate` | Gerar boleto |
| GET | `/api/v1/cards/{accountId}` | Listar cartões virtuais |
| POST | `/api/v1/cards` | Criar cartão virtual |
| GET | `/api/v1/insurance/plans` | Planos de seguro |
| POST | `/api/v1/insurance/subscribe` | Contratar seguro |
| GET | `/api/v1/goals/{accountId}` | Metas financeiras |
| POST | `/api/v1/goals` | Criar meta |
| GET | `/api/v1/scheduled-pix/{accountId}` | PIX agendados |
| POST | `/api/v1/scheduled-pix` | Agendar PIX |
| GET | `/api/v1/dashboard/{accountId}` | Dashboard consolidado |
| GET | `/api/v1/payments/statement/{accountId}` | Extrato |

> Documentação completa no Swagger: http://localhost:5002/swagger

---

## Testes

```bash
# Testes unitários (83 testes)
cd tests/KRT.Payments.UnitTests
dotnet test

# Testes de integração (8 testes)
cd tests/KRT.Payments.IntegrationTests
dotnet test

# Todos os testes
dotnet test KRT.sln

# Testes E2E (Cypress — requer app rodando)
cd src/Web/KRT.Web
npx cypress run
```

---

## Observabilidade

### SEQ (Logs Estruturados) — Incluído na stack

Todos os serviços enviam logs estruturados via Serilog para o SEQ:
- **UI:** http://localhost:8081
- **API:** http://localhost:5341

Filtre logs por serviço: `Application = 'KRT.Payments.Api'`

### Prometheus + Grafana (Opcional)

```bash
docker-compose -f docker-compose.observability.yml up -d
```

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| Grafana | http://localhost:3000 | admin / REDACTED_GRAFANA_PASSWORD |
| Prometheus | http://localhost:9090 | — |
| AlertManager | http://localhost:9093 | — |

---

## Containers Docker

| # | Container | Imagem | Porta | Healthcheck |
|---|-----------|--------|-------|-------------|
| 1 | krt-postgres | postgres:16-alpine | 5433 | `pg_isready` |
| 2 | krt-redis | redis:7-alpine | 6380 | `redis-cli ping` |
| 3 | krt-rabbitmq | rabbitmq:3-management | 5672, 15680 | `rabbitmq-diagnostics ping` |
| 4 | krt-kafka | confluentinc/cp-kafka:7.5.0 | 9092, 29092 | — |
| 5 | krt-zookeeper | confluentinc/cp-zookeeper:7.5.0 | 32181 | — |
| 6 | krt-keycloak | keycloak:23.0 | 8080 | — |
| 7 | krt-seq | datalust/seq:2024.1 | 5341, 8081 | — |
| 8 | krt-onboarding | .NET 8 (build local) | 5001 | `/health` |
| 9 | krt-payments | .NET 8 (build local) | 5002 | `/health` |
| 10 | krt-gateway | .NET 8 + YARP | 5000 | `/health` |
| 11 | krt-web | Node 20 build → Nginx | 4200 | `/nginx-health` |

---

## Troubleshooting

### Container crashando (`Restarting`)
```bash
docker logs <container-name> --tail 30
```

### APIs não conectam ao PostgreSQL
Os containers usam portas internas padrão (PostgreSQL `5432`, Redis `6379`). As portas externas (`5433`, `6380`) são só para acesso local. Verifique se os `environment` no docker-compose apontam para os nomes dos containers (`postgres`, `redis`, `rabbitmq`), não para `localhost`.

### SEQ crashando
A versão `latest` do SEQ pode ter bugs. O projeto usa `datalust/seq:2024.1` (estável). Se necessário:
```bash
docker volume rm krt-bank_seq-data
docker-compose up -d seq
```

### Angular build falha no Docker
Verifique se o `.dockerignore` na raiz exclui `node_modules`:
```bash
echo "src/Web/KRT.Web/node_modules" >> .dockerignore
```

### Gateway retorna 502
O Gateway YARP precisa que as APIs estejam respondendo. Em ambiente Docker, usa `appsettings.Docker.json` com os nomes dos containers (`payments-api:80`, `onboarding-api:80`). Em desenvolvimento local, usa `appsettings.json` com `localhost:5001/5002`.

### Nginx: `unknown directive "﻿server"`
O `nginx.conf` tem BOM (Byte Order Mark). Reescreva sem BOM:
```powershell
[System.IO.File]::WriteAllText("path\nginx.conf", $content, (New-Object System.Text.UTF8Encoding $false))
```

---

## Licença

Este projeto é de uso acadêmico / portfólio.

---

> **KRT Bank** — Desenvolvido como projeto fullstack de banking digital.
