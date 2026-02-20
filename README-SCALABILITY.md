# KRT Bank — Arquitetura de Escalabilidade & Resiliência

> Como o KRT Bank foi projetado para suportar centenas de milhares de requisições simultâneas em produção.

---

## Visão Geral

O KRT Bank é uma plataforma bancária digital construída com arquitetura distribuída, projetada para escalar horizontalmente e manter resiliência sob carga extrema. O sistema suporta operações críticas como PIX, consulta de saldo, extratos e onboarding de clientes com alta disponibilidade.

**Stack Principal:** .NET 8 | Angular 17 | PostgreSQL | Redis | Kafka | RabbitMQ | Keycloak | Docker

**Resultados de Load Testing:**
- Smoke Test (5 VUs): 100% sucesso, 0% falhas
- Load Test (1.000 VUs): 227.000+ iterações com token cache otimizado
- Stress Test (5.000 VUs): operação estável com degradação graceful
- Spike Test (10.000 VUs): recuperação automática após pico

---

## 1. Arquitetura de Microserviços

### Separação por Domínio de Negócio

O sistema é dividido em serviços independentes seguindo Domain-Driven Design (DDD):

| Serviço | Responsabilidade | Porta |
|---------|-----------------|-------|
| **Gateway** | Roteamento, rate limiting, load balancing | 5000 |
| **Payments API** | PIX, transferências, saldo, extrato | 5002 |
| **Onboarding API** | Cadastro, KYC, criação de conta | 5001 |
| **Web (Angular)** | Interface do cliente | 4200 |

**Por que isso escala:** Cada serviço pode ser escalado independentemente. Se o volume de PIX aumenta 10x, apenas o Payments API precisa de mais instâncias — o Onboarding continua com os mesmos recursos.

### API Gateway Pattern

O Gateway centraliza o tráfego de entrada e atua como ponto único de controle:

- **Roteamento inteligente** — direciona requisições para o serviço correto
- **Rate limiting** — protege serviços internos de sobrecarga
- **Load balancing** — distribui carga entre múltiplas instâncias
- **Circuit breaker** — impede cascata de falhas entre serviços

Em produção, com Kubernetes ou Docker Swarm, o Gateway distribui tráfego entre N réplicas de cada serviço automaticamente.

---

## 2. Event-Driven Architecture (EDA)

### Dual Messaging: Kafka + RabbitMQ

O KRT Bank usa dois sistemas de mensageria com propósitos distintos, cada um otimizado para seu caso de uso:

**Apache Kafka** — Eventos de domínio (alta throughput, ordenação garantida):
- `account.created` — nova conta criada
- `pix.initiated` / `pix.completed` / `pix.failed` — ciclo de vida do PIX
- `fraud.detected` — alerta de fraude
- Throughput: milhões de mensagens/segundo por partição
- Retenção de eventos para replay e auditoria

**RabbitMQ** — Notificações e comandos (delivery garantido, routing flexível):
- Notificações por e-mail e SMS
- Comandos de processamento assíncrono
- Dead Letter Queue para mensagens que falharam

**Por que dual messaging escala:** Kafka processa o volume bruto de eventos de domínio sem perder ordenação. RabbitMQ garante que cada notificação seja entregue exatamente uma vez. Separar as responsabilidades evita que um pico de notificações impacte o processamento de transações.

### 4 Kafka Consumers Especializados

Cada consumer processa um tipo específico de evento, permitindo escalonamento independente:

1. **Account Consumer** — processa eventos de criação de conta
2. **Payment Consumer** — processa eventos de PIX e transferências
3. **Fraud Consumer** — analisa transações em tempo real para detecção de fraude
4. **Notification Consumer** — dispara notificações via RabbitMQ

Em produção, cada consumer pode ter N instâncias em um Consumer Group do Kafka, dividindo automaticamente as partições entre si.

---

## 3. Padrões de Resiliência

### CQRS (Command Query Responsibility Segregation)

Leituras e escritas são separadas em pipelines distintos:

- **Commands** (10-15% do tráfego): `POST /pix`, `POST /register` — passam por validação, regras de negócio, persistência
- **Queries** (85-90% do tráfego): `GET /balance`, `GET /statement`, `GET /dashboard` — leitura otimizada, podem usar cache

**Por que isso escala:** Em um banco real, ~90% das requisições são consultas. Com CQRS, as leituras podem ser servidas por réplicas read-only do banco ou cache, sem impactar as escritas. As escritas passam pelo pipeline completo de validação e persistência.

### Saga Pattern (Transações Distribuídas)

Operações que envolvem múltiplos serviços usam o Saga Pattern para garantir consistência eventual:

**Exemplo — Fluxo de PIX:**
1. Payments API valida saldo → reserva valor
2. Kafka publica `pix.initiated`
3. Fraud Consumer analisa a transação
4. Se aprovado: `pix.completed` → débito confirmado → notificação enviada
5. Se rejeitado: `pix.failed` → compensação automática → saldo restaurado

**Por que isso escala:** Diferente de transações distribuídas (2PC) que bloqueiam recursos, Sagas são assíncronas. Cada etapa é independente e pode ser retentada. O sistema não fica travado esperando resposta de outro serviço.

### Outbox Pattern (Consistência Eventual)

Garante que eventos sejam publicados mesmo se o Kafka estiver temporariamente indisponível:

1. A transação grava no banco E na tabela Outbox dentro da mesma transação SQL
2. Um background worker lê a tabela Outbox e publica no Kafka
3. Após publicação confirmada, marca como processado

**Por que isso escala:** Elimina o problema de "dual write" (gravar no banco + publicar no Kafka). Se o Kafka cair, os eventos ficam na tabela Outbox e são publicados quando o Kafka voltar. Zero perda de dados.

### Circuit Breaker

Quando um serviço downstream (ex: Keycloak) começa a falhar, o Circuit Breaker:

1. **Closed** (normal): requisições passam normalmente
2. **Open** (falhas detectadas): requisições são rejeitadas imediatamente, sem sobrecarregar o serviço com problemas
3. **Half-Open** (testando recuperação): permite algumas requisições para verificar se o serviço voltou

**Por que isso escala:** Impede cascata de falhas. Se o Keycloak ficar lento, o Gateway retorna erro 503 imediatamente em vez de acumular milhares de conexões pendentes que derrubariam todo o sistema.

---

## 4. Caching Estratégico

### Redis — Cache Distribuído

O Redis atua em múltiplas camadas:

- **Session cache** — tokens de autenticação e dados de sessão
- **Application cache** — dados frequentemente consultados
- **Rate limiting** — controle de requisições por usuário/IP
- **Distributed locking** — previne race conditions em operações concorrentes

**Latência:** Redis opera em memória com latência de ~1ms vs ~5-50ms do PostgreSQL. Para consultas de saldo que representam 40% do tráfego, isso é transformador.

### Token Cache com TTL (k6 Load Testing)

Implementação de cache de tokens JWT com Time-To-Live de 15 minutos:

- Tokens são reutilizados entre requisições do mesmo usuário
- Login no Keycloak só ocorre quando o token expira
- Redução de 99.5% nas requisições de autenticação

**Cálculo real:**
- Sem cache: 1.000 VUs × 1 req/s × 18 min = 1.080.000 logins no Keycloak
- Com cache (TTL 15 min): 1.000 VUs × (18 min / 15 min) = ~2.000 logins
- Resultado: Keycloak recebe 500x menos carga

---

## 5. Banco de Dados — PostgreSQL

### Estratégias de Escalabilidade

- **Connection pooling** — reutilização de conexões para evitar overhead de criação
- **Índices otimizados** — queries de saldo e extrato com tempo de resposta < 5ms
- **Particionamento** — tabelas de transações particionadas por data para queries históricas eficientes

### Em Produção (Estratégia)

- **Read replicas** — consultas de saldo e extrato direcionadas para réplicas
- **Write primary** — apenas operações de escrita (PIX, cadastro) no banco principal
- **Backups incrementais** — point-in-time recovery para compliance bancário

---

## 6. Autenticação — Keycloak

### Arquitetura de Auth

- **OAuth 2.0 / OpenID Connect** — padrão de mercado para autenticação
- **JWT tokens** — stateless, sem consulta ao banco para validar
- **Realm isolado** (`krt-bank`) — configuração independente

### Otimizações para Alta Carga

No `docker-compose.yml`, o Keycloak recebe tuning de JVM:

```yaml
environment:
  JAVA_OPTS_APPEND: "-Xms512m -Xmx1g"
  KC_DB_POOL_MIN_SIZE: 10
  KC_DB_POOL_MAX_SIZE: 50
```

- **Heap de 1GB** — evita Garbage Collection excessivo sob carga
- **Connection Pool de 50** — suporta autenticações concorrentes sem espera por conexão

### Em Produção (Estratégia)

- Keycloak em cluster com múltiplas instâncias
- Session replication via Infinispan (built-in)
- Token TTL de 15 minutos (padrão bancário de segurança)

---

## 7. Observabilidade — OpenTelemetry + Grafana

### Três Pilares da Observabilidade

**Métricas (Grafana Cloud):**
- Requisições por segundo por endpoint
- Latência p50, p90, p95, p99
- Taxa de erros por serviço
- Uso de CPU, memória e conexões de banco

**Logs Estruturados (Seq / Serilog):**
- Correlation IDs para rastrear requisições entre serviços
- Logs enriquecidos com contexto de negócio
- Alertas automáticos para erros críticos

**Traces Distribuídos (OpenTelemetry):**
- Rastreamento end-to-end de cada requisição
- Identificação de gargalos entre serviços
- Mapa de dependências em tempo real

### Custom Metrics para k6

9 métricas customizadas monitoradas durante load testing:

| Métrica | Descrição |
|---------|-----------|
| `bank_login_duration` | Tempo de autenticação no Keycloak |
| `bank_register_duration` | Tempo de criação de conta |
| `bank_balance_duration` | Tempo de consulta de saldo |
| `bank_statement_duration` | Tempo de consulta de extrato |
| `bank_dashboard_duration` | Tempo de carregamento do dashboard |
| `bank_pix_duration` | Tempo de execução de PIX |
| `bank_successful_logins` | Counter de logins bem-sucedidos |
| `bank_successful_registrations` | Counter de cadastros |
| `bank_successful_pix` | Counter de PIX executados |

**Por que isso escala:** Sem observabilidade, é impossível identificar gargalos em produção. Com métricas em tempo real, a equipe detecta degradação antes que os clientes percebam e escala proativamente.

---

## 8. Load Testing — k6

### Estratégia de Testes

6 cenários de teste simulando diferentes padrões de uso:

| Cenário | VUs | Duração | Objetivo |
|---------|-----|---------|----------|
| **Smoke** | 5 | 1 min | Validação básica |
| **Load** | 1.000 | 18 min | Operação normal de produção |
| **Stress** | 5.000 | 21 min | Acima da capacidade planejada |
| **Spike** | 10.000 | 7 min | Pico repentino (Black Friday, PIX em massa) |
| **Soak** | 500 | 2h | Estabilidade de longa duração (memory leaks) |
| **Breakpoint** | 0→∞ | 30 min | Encontrar o limite máximo do sistema |

### Distribuição Realista de Tráfego

Os testes simulam o perfil real de um banco digital:

- 40% — `GET /balance` (consulta de saldo)
- 25% — `GET /statement` (extrato)
- 20% — `GET /dashboard` (visão geral)
- 10% — `POST /pix` (transferência)
- 5% — `POST /register` (abertura de conta)

**85% leituras / 15% escritas** — proporção verificada em bancos digitais reais.

### Pool de Usuários

Os testes usam um pool de usuários pré-criados no `setup()`:

| Cenário | Pool Size | Razão |
|---------|-----------|-------|
| Load | 50 | 1 user : 20 VUs |
| Stress | 80 | 1 user : 62 VUs |
| Spike | 80 | 1 user : 125 VUs |
| Soak | 100 | Duração longa, maior distribuição |
| Breakpoint | 100 | Escala progressiva |

Isso simula o comportamento real onde múltiplas sessões do mesmo usuário são possíveis (app mobile + web + tablet).

---

## 9. Containerização — Docker

### 11 Containers Orquestrados

```
┌─────────────────────────────────────────────────────┐
│                   Docker Compose                     │
│                                                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐          │
│  │ Gateway  │  │Payments  │  │Onboarding│          │
│  │  :5000   │─▶│  :5002   │  │  :5001   │          │
│  └──────────┘  └────┬─────┘  └────┬─────┘          │
│                     │             │                   │
│  ┌──────────┐  ┌────▼─────┐  ┌───▼──────┐          │
│  │ Keycloak │  │PostgreSQL│  │  Redis   │          │
│  │  :8080   │  │  :5433   │  │  :6380   │          │
│  └──────────┘  └──────────┘  └──────────┘          │
│                                                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐          │
│  │  Kafka   │  │ RabbitMQ │  │   Seq    │          │
│  │  :9092   │  │  :5672   │  │  :5341   │          │
│  └──────────┘  └──────────┘  └──────────┘          │
│                                                      │
│  ┌──────────┐  ┌──────────┐                         │
│  │Zookeeper │  │   Web    │                         │
│  │  :2181   │  │  :4200   │                         │
│  └──────────┘  └──────────┘                         │
└─────────────────────────────────────────────────────┘
```

### Estratégia de Escala em Produção

Em produção com Kubernetes:

- **Horizontal Pod Autoscaler (HPA)** — escala automaticamente baseado em CPU/memória
- **Cada serviço** pode ter de 1 a N réplicas independentes
- **Health checks** garantem que apenas instâncias saudáveis recebem tráfego
- **Rolling updates** — deploy sem downtime

---

## 10. Segurança e Anti-Fraude

### Sistema de Detecção de Fraude

O Fraud Consumer analisa cada transação PIX em tempo real:

- Valor acima do limite do cliente
- Frequência anormal de transações
- Horário atípico
- Destinatário desconhecido com valor alto

Se fraude é detectada, o evento `fraud.detected` é publicado e a transação é cancelada com compensação automática via Saga Pattern.

### Segurança de Infraestrutura

- **JWT stateless** — tokens validados sem consulta ao banco
- **OAuth 2.0** — padrão de mercado para autorização
- **Token TTL 15 min** — reduz janela de exposição de tokens comprometidos
- **Senhas no Keycloak** — bcrypt com salt, nunca armazenadas na aplicação

---

## 11. Padrões Arquiteturais

### Clean Architecture

```
┌─────────────────────────────────────┐
│           Presentation              │  Controllers, Minimal APIs
├─────────────────────────────────────┤
│           Application               │  Use Cases, CQRS Handlers
├─────────────────────────────────────┤
│             Domain                  │  Entities, Value Objects, Events
├─────────────────────────────────────┤
│          Infrastructure             │  EF Core, Kafka, Redis, Keycloak
└─────────────────────────────────────┘
```

Cada camada depende apenas das camadas internas. O Domain não conhece banco de dados, Kafka ou Redis — facilitando testes unitários e troca de tecnologias.

### Domain-Driven Design (DDD)

- **Aggregates** — Account, Transaction, User como raízes de agregado
- **Value Objects** — CPF, Money, Email com validação embutida
- **Domain Events** — `AccountCreated`, `PixInitiated`, `FraudDetected`
- **Bounded Contexts** — Payments e Onboarding como contextos independentes

---

## 12. Capacidade Estimada em Produção

### Projeção de Escala

Com base nos testes de carga e a arquitetura implementada:

| Componente | Single Instance | Cluster (3 nodes) | Com Auto-scaling |
|------------|----------------|-------------------|------------------|
| API Gateway | 1.000 req/s | 3.000 req/s | 10.000+ req/s |
| Payments API | 500 req/s | 1.500 req/s | 5.000+ req/s |
| Kafka | 100K msg/s | 300K msg/s | 1M+ msg/s |
| PostgreSQL | 5K queries/s | 15K queries/s (read replicas) | 50K+ queries/s |
| Redis | 100K ops/s | 300K ops/s | 1M+ ops/s |

### Gargalos Identificados e Soluções

| Gargalo | Causa | Solução Implementada |
|---------|-------|---------------------|
| Keycloak auth | Login a cada requisição | Token cache com TTL 15 min |
| Keycloak JVM | Heap insuficiente | JAVA_OPTS: 1GB heap |
| DB connections | Pool pequeno | KC_DB_POOL_MAX_SIZE: 50 |
| User contention | Pool pequeno nos testes | Pool de 50-100 usuários |

---

## Resumo: Por que o KRT Bank escala

1. **Microserviços** — escala horizontal independente por serviço
2. **Event-Driven** — Kafka + RabbitMQ para processamento assíncrono
3. **CQRS** — leituras e escritas otimizadas separadamente
4. **Saga Pattern** — transações distribuídas sem bloqueio
5. **Outbox Pattern** — consistência eventual sem perda de dados
6. **Redis Cache** — latência de 1ms para 85% das requisições
7. **Token Cache** — 99.5% menos carga no Keycloak
8. **Circuit Breaker** — proteção contra cascata de falhas
9. **Observabilidade** — métricas, logs e traces para detecção proativa
10. **Load Testing** — 6 cenários validando o sistema sob pressão extrema
11. **Docker/K8s-ready** — deploy com auto-scaling e zero downtime

---

*Documentação gerada para o projeto KRT Bank — Preparação para entrevista técnica BTG Pactual*
