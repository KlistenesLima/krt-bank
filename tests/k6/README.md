# KRT Bank - Performance Testing Suite (k6)

Suite completa de testes de performance usando [Grafana k6](https://k6.io/).

## Instalacao do k6

```powershell
# Windows (via Chocolatey)
choco install k6

# Ou via Docker (sem instalacao)
docker pull grafana/k6
```


## O que sao VUs (Virtual Users)?

Cada **VU (Virtual User)** simula um usuario real acessando o sistema simultaneamente. Quando o load test sobe para 1.000 VUs, significa **1.000 pessoas usando o KRT Bank ao mesmo tempo** — cada uma fazendo registro, login, PIX e consultando extrato.

| Cenario | VUs | Equivalencia no Mundo Real |
|---------|-----|---------------------------|
| Smoke | 5 | Validacao basica |
| Load | 1.000 | Banco digital em horario de pico |
| Stress | 5.000 | Banco grande em dia de pagamento |
| Spike | 10.000 | Black Friday / PIX viral |
| Soak | 500 | Operacao continua por 2 horas |
| Breakpoint | 20.000 | Teste de limite maximo do sistema |
## Cenarios Disponiveis

| Cenario | VUs | Duracao | Objetivo |
|---------|-----|---------|----------|
| **Smoke** | 5 | 1 min | Validacao basica — sistema responde? |
| **Load** | 1.000 | 18 min | Carga normal de producao bancaria |
| **Stress** | 5.000 | 25 min | Acima da capacidade planejada |
| **Spike** | 10.000 | 7 min | Pico subito (Black Friday/PIX viral) |
| **Soak** | 500 | 2 horas | Resistencia prolongada (memory leaks) |
| **Breakpoint** | 20.000 | 30 min | Encontrar ponto de ruptura do sistema |

## Execucao

```powershell
# Smoke test (rapido, validacao)
.\tests\k6\run-tests.ps1 -Scenario smoke

# Load test (1.000 usuarios simultaneos)
.\tests\k6\run-tests.ps1 -Scenario load

# Stress test (5.000 usuarios)
.\tests\k6\run-tests.ps1 -Scenario stress

# Spike test (10.000 usuarios)
.\tests\k6\run-tests.ps1 -Scenario spike

# Todos (exceto soak e breakpoint)
.\tests\k6\run-tests.ps1 -Scenario all

# Via Docker (sem instalar k6)
.\tests\k6\run-tests.ps1 -Scenario load -UseDocker
```

## High-Load Docker Setup

Para rodar com infraestrutura otimizada:

```bash
docker-compose -f docker-compose.yml -f docker-compose.highload.yml up -d
```

Otimizacoes aplicadas:
- PostgreSQL: 500 connections, 512MB shared_buffers, parallel workers
- Redis: 256MB maxmemory, LRU eviction
- Kafka: 12 partitions, IO/network threads otimizados
- APIs: ThreadPool minimo 100-200, connection pool 200
- Gateway: ThreadPool minimo 200

## Metricas Customizadas

| Metrica | Descricao |
|---------|-----------|
| `krt_pix_success_rate` | Taxa de sucesso das transacoes PIX |
| `krt_pix_duration` | Latencia end-to-end do PIX (ms) |
| `krt_accounts_created` | Total de contas criadas no teste |
| `krt_pix_transactions` | Total de transacoes PIX executadas |
| `krt_fraud_detected` | Total de fraudes detectadas |
| `krt_login_success_rate` | Taxa de sucesso de login |

## SLAs Definidos

| Metrica | Load (1K VUs) | Stress (5K VUs) | Spike (10K VUs) |
|---------|---------------|------------------|------------------|
| p50 latency | < 300ms | < 500ms | < 1000ms |
| p95 latency | < 800ms | < 2000ms | < 5000ms |
| Error rate | < 1% | < 5% | < 10% |
| PIX success | > 95% | > 90% | > 85% |
| Throughput | > 100 req/s | > 50 req/s | N/A |
