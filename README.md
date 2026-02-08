# KRT Bank - Digital Banking Platform

Sistema bancario digital completo desenvolvido em .NET 8 com Angular, seguindo arquitetura de microsservicos, DDD, CQRS e Event-Driven.

## Arquitetura

```
KRT Bank
├── Gateway (Ocelot)
├── Payments API (Pix, Boletos, Cartoes, Dashboard, Extrato, Notificacoes)
├── Onboarding API (Contas, Cadastro, KYC)
├── Angular Frontend (SPA com Chart.js)
└── Infrastructure (SQL Server, RabbitMQ, Redis, Keycloak)
```

## Tecnologias

- **Backend**: .NET 8, ASP.NET Core, Entity Framework Core, MediatR, FluentValidation
- **Frontend**: Angular 17+, Chart.js, SCSS
- **Mensageria**: RabbitMQ (Saga Pattern)
- **Cache**: Redis
- **Auth**: JWT + Keycloak (OpenID Connect)
- **Real-time**: SignalR WebSocket
- **PDF**: QuestPDF
- **QR Code**: QRCoder
- **Gateway**: Ocelot
- **Testes**: xUnit, Moq, FluentAssertions
- **Container**: Docker + Docker Compose

## Features (Partes 1-24)

| Parte | Feature | Detalhes |
|-------|---------|----------|
| 1-4 | Onboarding + DDD | Cadastro, conta, entidades de dominio |
| 5-6 | JWT Auth | Autenticacao, autorizacao, refresh tokens |
| 7-8 | Pix Saga | Transferencia Pix com saga pattern + RabbitMQ |
| 9-10 | Fraud Analysis | Motor de fraude com regras configuraveis |
| 11 | E2E Tests + Keycloak | Testes integracao, OpenID Connect |
| 12 | SignalR WebSocket | Notificacoes real-time |
| 13 | QR Code + PDF | QR Code Pix, comprovantes PDF, limites |
| 14 | Cartoes Virtuais | Cartao virtual Visa/Mastercard, CVV rotativo |
| 15 | Dashboard | Graficos Chart.js: saldo, categorias, mensal |
| 16 | Extrato Completo | Filtros, paginacao, export CSV/PDF |
| 17 | Pix Agendado | Agendamento unico e recorrente |
| 18 | Notificacoes | Central com lidas/nao-lidas, categorias |
| 19 | Contatos Pix | Favoritos, busca, tipos de chave |
| 20 | Boletos | Geracao, pagamento, codigo de barras |
| 21 | Perfil/Config | Dados pessoais, preferencias, seguranca |
| 22 | Sidebar Nav | Navegacao lateral completa |
| 23 | Health/Docs | Health check, lista endpoints, Swagger |
| 24 | Docker + README | Docker Compose, Dockerfiles, documentacao |

## Endpoints (42 rotas)

### Dashboard
- `GET /api/v1/dashboard/summary/{accountId}`
- `GET /api/v1/dashboard/balance-history/{accountId}`
- `GET /api/v1/dashboard/spending-categories/{accountId}`
- `GET /api/v1/dashboard/monthly-summary/{accountId}`

### Extrato
- `GET /api/v1/statement/{accountId}`
- `GET /api/v1/statement/{accountId}/export/csv`
- `GET /api/v1/statement/{accountId}/export/pdf`

### Pix
- `POST /api/v1/pix/transfer`
- `POST /api/v1/pix/qrcode/generate`
- `GET /api/v1/pix/receipt/{id}`
- `GET /api/v1/pix/limits/{accountId}`
- `PUT /api/v1/pix/limits/{accountId}`

### Pix Agendado
- `GET /api/v1/pix/scheduled/account/{accountId}`
- `POST /api/v1/pix/scheduled`
- `POST /api/v1/pix/scheduled/{id}/execute|cancel|pause|resume`
- `PUT /api/v1/pix/scheduled/{id}/amount`

### Contatos
- `GET /api/v1/contacts/{accountId}`
- `POST /api/v1/contacts/{accountId}`
- `POST /api/v1/contacts/{accountId}/{id}/favorite`
- `DELETE /api/v1/contacts/{accountId}/{id}`

### Boletos
- `GET /api/v1/boletos/account/{accountId}`
- `POST /api/v1/boletos/generate`
- `POST /api/v1/boletos/pay/{id}`
- `POST /api/v1/boletos/pay-barcode`

### Cartoes Virtuais
- `GET /api/v1/cards/account/{accountId}`
- `POST /api/v1/cards`
- `POST /api/v1/cards/{id}/block|unblock|rotate-cvv`

### Notificacoes
- `GET /api/v1/notifications/{accountId}`
- `GET /api/v1/notifications/{accountId}/unread-count`
- `POST /api/v1/notifications/{accountId}/read-all`

### Perfil
- `GET /api/v1/profile/{accountId}`
- `PUT /api/v1/profile/{accountId}`
- `PUT /api/v1/profile/{accountId}/preferences|security`

### Health
- `GET /api/v1/health`
- `GET /api/v1/health/detailed`
- `GET /api/v1/health/endpoints`

## Como Executar

### Local
```bash
dotnet build
dotnet run --project src/Services/KRT.Payments/KRT.Payments.Api
dotnet run --project src/Services/KRT.Onboarding/KRT.Onboarding.Api
dotnet run --project src/Services/KRT.Gateway/KRT.Gateway
cd src/Web/KRT.Web && ng serve
```

### Docker
```bash
docker-compose up -d
```

### Testes
```bash
dotnet test
```

## Testes
- **Unit Tests**: 80+ testes (Pix, Saga, Fraud, ScheduledPix, VirtualCard, Boleto, PixContact, PixLimit)
- **Integration Tests**: 8 testes E2E

## Licenca
Projeto academico — KRT Bank 2026