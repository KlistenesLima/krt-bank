# Auditoria de Segurança — KRT Bank

**Data:** 20/02/2026
**Auditor:** Automated Security Scan + Manual Review
**Escopo:** Preparação para repositório público no GitHub

## Resumo Executivo

**Status: APROVADO COM RESSALVAS**

Todos os secrets foram removidos dos arquivos rastreados e do histórico Git. Vulnerabilidades de código corrigidas. O repositório está seguro para ser tornado público, com ressalvas documentadas para produção.

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
- [x] Código TypeScript — **LIMPO** (admin API key original redactada pelo filter-repo)
- [x] GitHub Actions workflows — **LIMPO** (usa `${{ secrets.* }}`)

### 3. Vulnerabilidades de Código
- SQL Injection: **LIMPO** — Nenhum uso de `FromSqlRaw` com concatenação de strings
- XSS: **LIMPO** — Angular sanitiza por padrão; sem uso de `innerHTML` inseguro
- CORS misconfiguration: **OK** — `WithOrigins()` com URLs específicas em produção
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
- Authentication: **IMPLEMENTADO** — JWT via Keycloak; API Key middleware com deny-by-default

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
| 1 | CRITICAL | Secrets reais (KrtBank2026, krt123, krt-dev-key-2026, krt-admin-key-2026, krtbank2026, grafana endpoint) no histórico Git | Histórico limpo com `git-filter-repo --replace-text` |
| 2 | HIGH | ApiKeyMiddleware permitia passagem silenciosa quando API key não configurada | Corrigido: retorna 503 (deny-by-default) quando key não está definida |
| 3 | MEDIUM | launchSettings.json (3 arquivos) rastreados pelo Git | Removidos do tracking com `git rm --cached` |
| 4 | MEDIUM | .gitignore não cobria .env.*, *.pem, *.key, launchSettings.json | .gitignore atualizado com cobertura completa |
| 5 | MEDIUM | KRT.Payments e KRT.Onboarding sem security headers | Middleware de security headers adicionado em ambos |

## Issues Conhecidos (Aceitos para Demo)

| # | Severidade | Descrição | Justificativa |
|---|-----------|-----------|---------------|
| 1 | MEDIUM | `[AllowAnonymous]` em ~90 endpoints financeiros no Payments | Design para demo/portfólio. Maioria protegida por API Key middleware. Em produção: adicionar `[Authorize]` e OAuth2 Client Credentials |
| 2 | MEDIUM | `RequireHttpsMetadata = false` hardcoded no JWT | Necessário para Docker (Keycloak interno sem TLS). Em produção: tornar configurável |
| 3 | MEDIUM | `ValidIssuer` hardcoded como `localhost:8080` | Funcional para dev/Docker. Em produção: configurar via appsettings |
| 4 | MEDIUM | JWT tokens armazenados em localStorage | Padrão comum em SPAs. Em produção: migrar para httpOnly cookies |
| 5 | MEDIUM | `EnableDetailedErrors = true` no SignalR sem guard de ambiente | Aceito para demo. Em produção: gate com `IsDevelopment()` |
| 6 | LOW | CORS `AllowAnyOrigin` em dev no KRT.Onboarding | Apenas em Development; produção usa origins específicas |
| 7 | LOW | `sslRequired: "none"` no Keycloak realm | Configuração de desenvolvimento |
| 8 | LOW | `EnsureCreated()` sem guard de ambiente | Aceito para demo. Em produção: usar migrations com CI/CD |
| 9 | LOW | Keycloak `directAccessGrantsEnabled: true` (ROPC flow) | Usado para login direto via CPF/senha na UI. Em produção: migrar para Authorization Code + PKCE |
| 10 | LOW | Keycloak admin defaults `admin/admin` em fallback no C# | Overridden por env vars no Docker; apenas fallback de desenvolvimento |
| 11 | LOW | nginx.conf sem security headers | Headers aplicados no Gateway (upstream). Em produção: adicionar no nginx também |

## Recomendações para Produção

1. **Rotacionar TODAS as senhas** antes do deploy (senhas de dev foram expostas no histórico anterior)
2. **Adicionar `[Authorize]`** em todos os endpoints financeiros e remover `[AllowAnonymous]` excessivo
3. **Habilitar HTTPS** em todos os serviços com certificados TLS válidos
4. **Configurar `sslRequired: "external"`** no Keycloak realm
5. **Tornar `RequireHttpsMetadata` e `ValidIssuer`** configuráveis via appsettings
6. **Substituir API Key** por OAuth2 Client Credentials para comunicação service-to-service
7. **Migrar JWT storage** de localStorage para httpOnly cookies
8. **Adicionar CSP (Content-Security-Policy)** e **HSTS** headers
9. **Gate `EnableDetailedErrors`** e `EnsureCreated()` com `IsDevelopment()`
10. **Adicionar security headers** no nginx.conf
11. **Configurar WAF** na frente do Gateway em produção
12. **Implementar audit logging** persistente para ações administrativas
