# Auditoria de Segurança — KRT Bank

**Data:** 20/02/2026
**Auditor:** Automated Security Scan + Manual Review
**Escopo:** Preparação para repositório público no GitHub

## Resumo Executivo

**Status: APROVADO**

Todos os secrets foram removidos dos arquivos rastreados e do histórico Git. O repositório está seguro para ser tornado público.

## Verificações Realizadas

### 1. Git History Scan
- [x] Busca de senhas no histórico completo (todas as branches)
- Resultado: **Secrets encontrados e LIMPOS com git-filter-repo**
- Secrets removidos do histórico:
  - `KrtBank2026` (PostgreSQL password) → `REDACTED_DB_PASSWORD`
  - `krt123` (RabbitMQ password) → `REDACTED_RABBITMQ_PASSWORD`
  - `krt-dev-key-2026` (API key) → `REDACTED_API_KEY`
  - `krt-admin-key-2026` (Admin API key) → `REDACTED_ADMIN_KEY`
  - `krtbank2026` (Grafana password) → `REDACTED_GRAFANA_PASSWORD`
  - `klisteneswar3.grafana.net` (Grafana endpoint) → `REDACTED_GRAFANA_ENDPOINT`
- Verificação final: **ZERO** ocorrências em todo o histórico

### 2. Secrets em Arquivos Atuais
- [x] appsettings*.json — **LIMPO** (usa `CHANGE_ME` e `${VAR}` placeholders)
- [x] docker-compose*.yml — **LIMPO** (usa `${VAR:?Set ...}` para todos os secrets)
- [x] Keycloak realm — **LIMPO** (usa `CHANGE_ME_*` placeholders)
- [x] Dockerfiles — **LIMPO** (sem ENV/ARG com senhas)
- [x] .env.example — **LIMPO** (apenas placeholders `CHANGE_ME_*`)
- [x] Código C# — **LIMPO** (sem passwords hardcoded)
- [x] Código TypeScript — **LIMPO** (sem API keys ou tokens hardcoded)

### 3. Vulnerabilidades de Código
- SQL Injection: **LIMPO** — Nenhum uso de `FromSqlRaw` com concatenação de strings
- XSS: **LIMPO** — Angular sanitiza por padrão; sem uso de `innerHTML` inseguro
- CORS misconfiguration: **OK** — `WithOrigins()` com URLs específicas em produção; `AllowAnyOrigin()` apenas em dev no Onboarding (aceitável)
- Insecure deserialization: **LIMPO** — Nenhum uso de `BinaryFormatter`, `SoapFormatter`, `TypeNameHandling.All/Auto`
- Weak cryptography: **LIMPO** — Nenhum uso de MD5 ou SHA1
- Sensitive data logging: **LIMPO** — Nenhum log de passwords/secrets/tokens

### 4. Configuração de Segurança
- Security headers: **IMPLEMENTADO** em todos os serviços
  - Gateway: X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy, Permissions-Policy
  - Payments API: Security headers adicionados nesta auditoria
  - Onboarding API: Security headers adicionados nesta auditoria
- Rate limiting: **IMPLEMENTADO** no Gateway (100 req/min global, 30 req/min admin, 50 req/min charges)
- Input validation: **IMPLEMENTADO** — Controllers usam validação via MediatR/FluentValidation
- HTTPS enforcement: **N/A** — Tráfego interno Docker é HTTP (aceitável); HTTPS será terminado no reverse proxy em produção
- Authentication: **IMPLEMENTADO** — JWT via Keycloak em todos os endpoints protegidos

### 5. .gitignore
- [x] .env e .env.* protegidos
- [x] Certificados (*.pem, *.key, *.pfx, *.p12, *.cert, *.crt) protegidos
- [x] Arquivos de IDE (.vs/, .vscode/, .idea/) protegidos
- [x] Build artifacts (bin/, obj/, dist/) protegidos
- [x] launchSettings.json protegido
- [x] appsettings.Development.json protegido

## Issues Encontrados e Resolvidos

| # | Severidade | Descrição | Ação Tomada |
|---|-----------|-----------|-------------|
| 1 | CRITICAL | Secrets reais (KrtBank2026, krt123, krt-dev-key-2026, krt-admin-key-2026, krtbank2026) no histórico Git | Histórico limpo com `git-filter-repo --replace-text` |
| 2 | MEDIUM | launchSettings.json (3 arquivos) rastreados pelo Git | Removidos do tracking com `git rm --cached` |
| 3 | MEDIUM | .gitignore não cobria .env.*, *.pem, *.key, launchSettings.json | .gitignore atualizado com cobertura completa |
| 4 | MEDIUM | KRT.Payments e KRT.Onboarding sem security headers | Middleware de security headers adicionado |

## Issues Conhecidos (Aceitos para Demo)

| # | Severidade | Descrição | Justificativa |
|---|-----------|-----------|---------------|
| 1 | LOW | CORS `AllowAnyOrigin` em dev no KRT.Onboarding | Apenas em `ASPNETCORE_ENVIRONMENT=Development`; produção usa origins específicas |
| 2 | LOW | `RequireHttpsMetadata = false` no JWT | Necessário para ambiente Docker (Keycloak interno sem TLS) |
| 3 | LOW | `sslRequired: "none"` no Keycloak realm | Configuração de desenvolvimento; em produção deve ser "external" |
| 4 | LOW | Endpoints de charges com `[AllowAnonymous]` | Necessário para integração KLL→KRT via API Key. Melhoria futura: OAuth2 Client Credentials |

## Recomendações para Produção

1. **Rotacionar TODAS as senhas** antes do deploy em produção (as senhas de dev foram expostas no histórico anterior)
2. **Habilitar HTTPS** em todos os serviços com certificados TLS válidos
3. **Configurar `sslRequired: "external"`** no Keycloak realm
4. **Substituir API Key** por OAuth2 Client Credentials para comunicação service-to-service
5. **Adicionar CSP (Content-Security-Policy)** header quando os frontends forem servidos pelo mesmo domínio
6. **Adicionar HSTS (Strict-Transport-Security)** quando HTTPS estiver habilitado
7. **Configurar WAF** (Web Application Firewall) na frente do Gateway em produção
8. **Implementar audit logging** persistente para ações administrativas
