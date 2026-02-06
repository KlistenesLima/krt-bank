
# KRT Bank — Enterprise Distributed Banking Platform 🚀

O **KRT Bank** é uma plataforma bancária digital distribuída, projetada como um **Architecture Showcase** para nível **Staff / Principal Engineer**.

O foco é demonstrar engenharia de sistemas financeiros reais, resolvendo problemas complexos como:

- Identidade Centralizada (OAuth2 / OpenID Connect)
- Rastreabilidade Distribuída (Correlation IDs end-to-end)
- Consistência Eventual (Outbox Pattern)
- Alta Disponibilidade e Resiliência

> Este projeto não é um CRUD. É um ecossistema bancário modular, seguro por design e observável por padrão.

---

## 🎯 Objetivos Estratégicos

- Demonstrar arquitetura enterprise realista (não apenas teórica)
- Implementar **Identity-First Security** com Keycloak
- Garantir **Observabilidade Total** com logs estruturados e tracing centralizado
- Implementar padrões de Sistemas Distribuídos (Saga, CQRS, Event-Driven)

---

## 🏗️ Arquitetura (Visão Executiva)

A arquitetura segue **Hexagonal / Clean Architecture**, isolando domínio de infraestrutura.

```text
[ Cliente / Web ] <---(JWT)---> [ Identity Provider (Keycloak) ]
       |
       v
[ API Gateway / Ingress ]
       |
       +-------------------------+
       |                         |
  [ Onboarding ]            [ Payments ] <---(Events)---> [ Kafka ]
       |                         |
  [ PostgreSQL ]            [ PostgreSQL ]
       |                         |
       +-----------+-------------+
                   |
           [ Observability (Seq) ]
      (Logs centralizados + Correlation ID)
```

### Princípios Arquiteturais

- **Security by Design** — Autenticação stateless via JWT (OIDC)
- **Domain-Driven Design (DDD)** — Domínio rico e invariantes protegidas
- **Event-Driven Architecture** — Desacoplamento via Kafka
- **Transactional Outbox Pattern** — Entrega garantida (At-Least-Once)
- **Distributed Tracing** — Correlação ponta a ponta via `X-Correlation-Id`

---

## 🔐 Segurança & Identidade (Keycloak)

A segurança é centralizada em um Identity Provider corporativo.

- **Identity Server:** Keycloak 24.0.1 (Docker)
- **Protocolos:** OAuth2 + OpenID Connect (OIDC)

### Fluxo

1. Usuário autentica no Keycloak → recebe JWT
2. Requests para APIs exigem `Authorization: Bearer <token>`
3. APIs validam assinatura (RS256), expiração e claims

> Zero Trust: nenhuma API confia em nada sem validar token.

---

## 📈 Observabilidade (Seq)

Observabilidade nativa por padrão.

- **Correlation ID:** Gerado na entrada e propagado via HTTP + Kafka
- **Centralização:** Todos os serviços enviam logs ao Seq (`http://localhost:5341`)

### Benefício

Permite rastrear uma transação completa:

- Request HTTP
- Commit no banco
- Publicação Kafka
- Consumo Kafka
- Execução de regra de domínio

---

## 🔌 Microsserviços

| Serviço | Responsabilidade | Stack |
|--------|------------------|-------|
| **KRT.Identity (Keycloak)** | Usuários, roles, tokens e SSO | Java/Quarkus |
| **KRT.Onboarding** | Criação de contas, KYC, validação cadastral | .NET 8, EF Core, PostgreSQL |
| **KRT.Payments** | Pix, transferências, ledger bancário | .NET 8, MediatR, PostgreSQL |
| **KRT.Infra** | Mensageria e Observabilidade | Kafka, Zookeeper, Seq |

---

## 🚀 Como Rodar Localmente

Ambiente 100% conteinerizado.

### Pré-requisitos

- Docker + Docker Compose
- PowerShell
- .NET 8 SDK

---

### 1️⃣ Subir Infraestrutura Completa

```powershell
docker-compose up -d
```

Sobe: Postgres, Redis, Kafka, Zookeeper, Seq e Keycloak.

---

### 2️⃣ Configurar Identity Provider (Automático)

```powershell
./setup-keycloak.ps1
```

Resultado:

- Realm: `krt-bank`
- Client: `krt-api`
- Usuário: `tester`

---

### 3️⃣ Executar Microsserviços

```bash
# Terminal 1 - Onboarding
cd src/Services/KRT.Onboarding/KRT.Onboarding.Api
dotnet run

# Terminal 2 - Payments
cd src/Services/KRT.Payments/KRT.Payments.Api
dotnet run
```

---

### 4️⃣ Teste End-to-End (E2E)

```powershell
./test-e2e-flow.ps1
```

O script:

1. Autentica no Keycloak
2. Cria conta no Onboarding
3. Executa Pix no Payments
4. Exibe Correlation ID

Visualize no Seq:
👉 http://localhost:5341

---

## 🛠️ Stack Tecnológica

### Backend & Infra

- .NET 8 (C#)
- Keycloak
- Seq
- Apache Kafka
- PostgreSQL (Database per Service)
- Redis
- Docker Compose

---

## 🧩 Design Patterns

- Clean Architecture
- CQRS
- Outbox Pattern
- Notification Pattern (Validações)
- Result Pattern (Tratamento de erros)

---

## 🧠 Destaques de Engenharia

✔ **Identity Agnostic** — Serviços não conhecem usuários, apenas tokens válidos  
✔ **Traceability First** — Logs estruturados e correlacionados  
✔ **Infrastructure as Code** — Ambiente inteiro sobe com um comando  
✔ **Fail-Fast Domain** — Validações antes de qualquer I/O

---

## 🛣️ Roadmap

- [x] Arquitetura Base (DDD / Clean Architecture)
- [x] Mensageria (Kafka + Outbox)
- [x] Identity Server (Keycloak + OIDC)
- [x] Observabilidade Centralizada (Seq)
- [ ] Resiliência (Polly - Retry / Circuit Breaker)
- [ ] API Gateway (YARP)
- [ ] Frontend Angular Integrado

---

© 2026 — **KRT Bank**  
Engineered for scale. Secured by design. Built for reality.
