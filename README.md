# KRT Bank — Enterprise Distributed Banking Platform 🚀

![.NET 8](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![Kafka](https://img.shields.io/badge/Apache%20Kafka-231F20?style=for-the-badge&logo=apachekafka&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)

O **KRT Bank** é uma plataforma bancária digital distribuída, projetada como **Architecture Showcase** para nível **Staff / Principal Engineer**.  
O foco é demonstrar **engenharia de sistemas financeiros reais**, com:
- Alta disponibilidade
- Consistência eventual
- Escalabilidade horizontal
- Observabilidade
- Resiliência transacional

Este projeto não é um CRUD. É um **core bancário modular**, orientado a eventos e preparado para produção.

---

## 🎯 Objetivos Estratégicos

- Demonstrar **arquitetura enterprise realista**
- Implementar **DDD + Clean Architecture + Event-Driven**
- Simular desafios de bancos digitais modernos
- Servir como **portfólio técnico de alto nível**

---

## 🏗️ Arquitetura (Visão Executiva)

```text
[ Angular Web ]
       |
[ API Gateway ]
       |
-----------------------------
|           |               |
Onboarding  Payments     Notifications
   |            |
 PostgreSQL   PostgreSQL
   |            |
   ---- Outbox Pattern ----
               |
             Kafka
               |
        Event Consumers
               |
        Redis / Projections
```

### Princípios Arquiteturais

- **Clean Architecture**
- **Domain-Driven Design (DDD)**
- **CQRS**
- **Event-Driven Architecture**
- **Hexagonal Architecture**
- **Transactional Outbox Pattern**
- **Idempotência e Retentativas**
- **Fail-Fast + Retry Policies**
- **Observabilidade desde o primeiro commit**

---

## 🧩 Building Blocks (Shared Kernel)

Camada reutilizável entre microsserviços.

### `KRT.BuildingBlocks.Domain`
- Aggregate Roots
- Entidades base
- Value Objects (CPF, Money, Email)
- Domain Events
- Guards de invariantes

### `KRT.BuildingBlocks.Infrastructure`
- EF Core abstraído
- UnitOfWork
- Repositórios genéricos
- Outbox Pattern
- Retry Policies
- Interceptadores de auditoria

### `KRT.BuildingBlocks.EventBus`
- Abstração de mensageria
- Implementação Kafka
- Serialização resiliente
- Dead Letter Queue (DLQ)

---

## 🔌 Microsserviços

| Serviço | Responsabilidade | Stack |
|--------|------------------|-------|
| **KRT.Onboarding** | Criação de contas, autenticação, KYC, ciclo de vida do cliente | .NET 8, EF Core, Redis |
| **KRT.Payments** | Pix, boletos, transferências internas, ledger | .NET 8, MediatR, PostgreSQL |
| **KRT.Notifications** *(roadmap)* | Push, email, eventos outbound | Kafka Consumers |
| **KRT.Fraud** *(roadmap)* | Análise antifraude e scoring | Kafka Streams / ML |

---

## 🛠️ Stack Tecnológica

### Backend
- **.NET 8**
- **EF Core + PostgreSQL**
- **Redis**
- **Apache Kafka**
- **MediatR**
- **FluentValidation**
- **Serilog**
- **OpenTelemetry**
- **HealthChecks**

### Frontend
- **Angular 16+**
- **Angular Material (Material 3)**
- **JWT Interceptors**
- **Guards**
- **Lazy Loading**
- **Skeleton Loading**
- **PWA Ready**

### Infraestrutura
- **Docker / Docker Compose**
- **Zookeeper + Kafka**
- **PostgreSQL**
- **Redis**
- **Traefik / Nginx (roadmap)**

---

## 🔄 Fluxos de Negócio

### 🏦 Criação de Conta (Onboarding)

1. Frontend envia `CreateAccountCommand`
2. Handler valida invariantes de domínio
3. Persistência transacional em PostgreSQL
4. Evento `AccountCreatedEvent` gravado na Outbox
5. Worker publica no Kafka
6. Consumidores atualizam projeções e caches

✔️ **Resultado:** consistência eventual sem perda de evento

---

### 💸 Transferência Pix (Payments)

1. Cliente inicia Pix
2. Serviço valida saldo, chave e limites
3. Ledger é atualizado atomicamente
4. Evento `PaymentExecutedEvent` é publicado
5. Serviços downstream reagem

✔️ **Resultado:** sistema desacoplado, resiliente e auditável

---

## 🔐 Segurança

- JWT Authentication
- Claims por domínio
- Policy-based authorization
- Criptografia de dados sensíveis
- Secrets via variáveis de ambiente
- Proteção contra replay attacks
- Idempotência por request

---

## 📈 Observabilidade

- Structured Logging (Serilog)
- CorrelationId em todas as requests
- Tracing distribuído (OpenTelemetry)
- HealthChecks por serviço
- Métricas prontas para Prometheus

---

## 🚀 Como Rodar Localmente

### Pré-requisitos

- Docker
- Docker Compose
- .NET 8 SDK
- Node.js 18+

---

### 1️⃣ Subir Infraestrutura

```bash
docker-compose up -d
```

Sobe automaticamente:
- PostgreSQL
- Redis
- Kafka
- Zookeeper

---

### 2️⃣ Backend

```bash
# Onboarding
cd src/Services/KRT.Onboarding/KRT.Onboarding.Api
dotnet run

# Payments
cd src/Services/KRT.Payments/KRT.Payments.Api
dotnet run
```

---

### 3️⃣ Frontend

```bash
cd src/Web/KRT.Web
npm install
npm start
```

Acesse: `http://localhost:4200`

---

## 🧠 Destaques Técnicos Reais

✔ Outbox Pattern com reprocessamento seguro  
✔ Event-driven real, não fake  
✔ CQRS segregado corretamente  
✔ Value Objects ricos (CPF, Money, Email)  
✔ Boundary clara entre domínio e infraestrutura  
✔ Serviços prontos para escalar horizontalmente  
✔ Código preparado para auditoria bancária  
✔ Design para falhas, não para happy-path  

---

## 🛣️ Roadmap

- [ ] API Gateway com rate-limit
- [ ] Saga Orchestrator
- [ ] Processamento assíncrono antifraude
- [ ] Observabilidade com Grafana
- [ ] Circuit Breaker distribuído
- [ ] Feature flags
- [ ] Canary deploy

---

## 👨‍💻 Autor

Projeto desenvolvido como **laboratório de arquitetura bancária moderna**, com foco em:
- Sistemas distribuídos críticos
- Arquitetura corporativa
- Engenharia de plataforma
- Design resiliente

LinkedIn: _(adicione aqui)_  
GitHub: _(adicione aqui)_

---

© 2026 — KRT Bank  
**Engineered for scale. Designed for failure. Built for reality.**
