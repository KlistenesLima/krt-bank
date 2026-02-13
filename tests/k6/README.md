# KRT Bank — Testes de Performance (k6)

## Distribuição de Tráfego Realista

Os testes simulam o padrão de tráfego real de um banco digital, onde **~90% das operações são consultas** e apenas **~10% são escritas**:

```
┌─────────────────────────────────────────────────────────┐
│              DISTRIBUIÇÃO DE TRÁFEGO                    │
├──────────────────────────┬──────────────────────────────┤
│  40%  Consulta Saldo     │  GET /accounts/{id}/balance  │
│  25%  Extrato            │  GET /statement/{id}         │
│  20%  Dashboard          │  GET /dashboard/summary/{id} │
│  10%  PIX Transfer       │  POST /pix                   │
│   5%  Registro/Login     │  POST /auth/register + login │
├──────────────────────────┴──────────────────────────────┤
│  85% LEITURA (read)  │  15% ESCRITA (write)             │
└─────────────────────────────────────────────────────────┘
```

### Por que essa distribuição?

Em sistemas bancários reais:
- Usuários consultam saldo **dezenas de vezes** antes de fazer uma transferência
- Extrato e dashboard são as telas mais acessadas do app
- Registros de novas contas representam uma fração mínima do tráfego diário
- A proporção **90/10 (read/write)** é padrão da indústria financeira

## Arquitetura dos Testes

```
tests/k6/
├── lib/
│   ├── config.js          # Configurações, geradores (CPF, email, phone)
│   └── helpers.js         # Pool de usuários, distribuição de tráfego, metrics
├── scenarios/
│   ├── smoke.js           # Validação funcional (5 VUs, 1 min)
│   ├── load.js            # Carga normal (até 1.000 VUs, 18 min)
│   ├── stress.js          # Stress test (até 5.000 VUs, 21 min)
│   ├── spike.js           # Pico súbito (100 → 10.000 VUs)
│   ├── soak.js            # Endurance (500 VUs, 2 horas)
│   └── breakpoint.js      # Encontrar limite (até 20.000 VUs)
└── README.md
```

## Pool de Usuários Pré-Criados

Todos os cenários (exceto smoke) utilizam um **pool de usuários pré-criados** na fase `setup()`:

1. **Setup**: Cria N usuários com register + login (sequencial, com delay)
2. **Execução**: VUs reutilizam usuários do pool com re-autenticação
3. **Distribuição**: Cada VU executa ações com probabilidade bancária real

Isso evita que o teste gaste 95% do tempo criando contas e simula o padrão real onde milhares de usuários existentes fazem consultas simultâneas.

## Como Executar

### Pré-requisitos
- [k6](https://k6.io/docs/getting-started/installation/) instalado
- Docker containers rodando (`docker-compose up -d`)

### Comandos

```bash
# 1. Smoke Test — validação rápida (sempre rodar primeiro)
k6 run --env BASE_URL=http://localhost:5000 .\tests\k6\scenarios\smoke.js

# 2. Load Test — carga normal (18 min)
k6 run --env BASE_URL=http://localhost:5000 .\tests\k6\scenarios\load.js

# 3. Stress Test — buscar limites (21 min)
k6 run --env BASE_URL=http://localhost:5000 .\tests\k6\scenarios\stress.js

# 4. Spike Test — pico súbito (~7 min)
k6 run --env BASE_URL=http://localhost:5000 .\tests\k6\scenarios\spike.js

# 5. Soak Test — endurance (2 horas)
k6 run --env BASE_URL=http://localhost:5000 .\tests\k6\scenarios\soak.js

# 6. Breakpoint Test — encontrar limite (30 min)
k6 run --env BASE_URL=http://localhost:5000 .\tests\k6\scenarios\breakpoint.js
```

### Configuração do Pool

```bash
# Pool padrão (20 usuários)
k6 run --env BASE_URL=http://localhost:5000 .\tests\k6\scenarios\load.js

# Pool customizado (50 usuários para mais concorrência)
k6 run --env BASE_URL=http://localhost:5000 --env POOL_SIZE=50 .\tests\k6\scenarios\load.js
```

## Cenários

| Cenário | VUs Max | Duração | Pool | Objetivo |
|---------|---------|---------|------|----------|
| **Smoke** | 5 | 1 min | — | Validar todos os endpoints funcionam |
| **Load** | 1.000 | 18 min | 20 | Comportamento sob carga normal |
| **Stress** | 5.000 | 21 min | 30 | Identificar ponto de degradação |
| **Spike** | 10.000 | 7 min | 30 | Resiliência a picos súbitos |
| **Soak** | 500 | 2h | 50 | Estabilidade e memory leaks |
| **Breakpoint** | 20.000 | 30 min | 40 | Capacidade máxima do sistema |

## Métricas Customizadas

| Métrica | Tipo | Descrição |
|---------|------|-----------|
| `krt_pix_success_rate` | Rate | Taxa de sucesso de transferências PIX |
| `krt_pix_duration` | Trend | Latência das transferências PIX |
| `krt_pix_transactions` | Counter | Total de transações PIX executadas |
| `krt_accounts_created` | Counter | Total de contas criadas |
| `krt_fraud_detected` | Counter | Fraudes detectadas pelo anti-fraude |
| `krt_login_success_rate` | Rate | Taxa de sucesso de autenticação |
| `krt_balance_check_rate` | Rate | Taxa de sucesso de consulta de saldo |
| `krt_statement_check_rate` | Rate | Taxa de sucesso de consulta de extrato |
| `krt_dashboard_check_rate` | Rate | Taxa de sucesso de acesso ao dashboard |

## Endpoints Testados

| Operação | Método | Endpoint | Tráfego |
|----------|--------|----------|---------|
| Consulta Saldo | `GET` | `/api/v1/accounts/{id}/balance` | 40% |
| Extrato | `GET` | `/api/v1/statement/{id}` | 25% |
| Dashboard | `GET` | `/api/v1/dashboard/summary/{id}` | 20% |
| PIX Transfer | `POST` | `/api/v1/pix` | 10% |
| Registro | `POST` | `/api/v1/auth/register` | 5% |
| Login | `POST` | `/api/v1/auth/login` | Sob demanda |
| Health Check | `GET` | `/api/v1/health` | Smoke only |
