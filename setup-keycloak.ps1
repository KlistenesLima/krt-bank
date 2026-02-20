# ============================================================
# KRT Bank — Setup Keycloak via Admin API
# Usar quando o auto-import do docker-compose nao funcionar
# ============================================================
param(
    [string]$KeycloakUrl = "http://localhost:8080",
    [string]$AdminUser = "admin",
    [string]$AdminPass = "admin"
)

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  KRT Bank — Keycloak Setup" -ForegroundColor White
Write-Host "  $KeycloakUrl" -ForegroundColor Gray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# 1. Espera o Keycloak subir
Write-Host "[1/5] Aguardando Keycloak..." -ForegroundColor Yellow
$maxRetries = 30
$retry = 0
while ($retry -lt $maxRetries) {
    try {
        $null = Invoke-RestMethod -Uri "$KeycloakUrl/realms/master" -TimeoutSec 3 -ErrorAction Stop
        Write-Host "  Keycloak pronto!" -ForegroundColor Green
        break
    } catch {
        $retry++
        if ($retry -ge $maxRetries) {
            Write-Host "  ERRO: Keycloak nao respondeu em $($maxRetries * 3)s" -ForegroundColor Red
            exit 1
        }
        Write-Host "  Tentativa $retry/$maxRetries..." -ForegroundColor Gray
        Start-Sleep -Seconds 3
    }
}

# 2. Obtém admin token
Write-Host "[2/5] Obtendo admin token..." -ForegroundColor Yellow
try {
    $tokenResponse = Invoke-RestMethod -Uri "$KeycloakUrl/realms/master/protocol/openid-connect/token" `
        -Method POST `
        -ContentType "application/x-www-form-urlencoded" `
        -Body "grant_type=password&client_id=admin-cli&username=$AdminUser&password=$AdminPass"
    $adminToken = $tokenResponse.access_token
    Write-Host "  OK (token obtido)" -ForegroundColor Green
} catch {
    Write-Host "  ERRO: Falha ao obter token admin: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $adminToken"
    "Content-Type" = "application/json"
}

# 3. Verifica se realm ja existe
Write-Host "[3/5] Verificando realm krt-bank..." -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms/krt-bank" -Headers $headers -ErrorAction Stop
    Write-Host "  Realm ja existe! Pulando criacao." -ForegroundColor Green
} catch {
    # 4. Importa realm via API
    Write-Host "[4/5] Importando realm..." -ForegroundColor Yellow
    $realmFile = Join-Path $PSScriptRoot "infra\keycloak\krt-bank-realm.json"
    if (-not (Test-Path $realmFile)) {
        Write-Host "  ERRO: $realmFile nao encontrado" -ForegroundColor Red
        exit 1
    }
    $realmJson = Get-Content $realmFile -Raw
    try {
        Invoke-RestMethod -Uri "$KeycloakUrl/admin/realms" `
            -Method POST -Headers $headers -Body $realmJson -ErrorAction Stop
        Write-Host "  Realm krt-bank criado!" -ForegroundColor Green
    } catch {
        Write-Host "  ERRO ao criar realm: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# 5. Validacao
Write-Host "[5/5] Validando..." -ForegroundColor Yellow
try {
    $testToken = Invoke-RestMethod -Uri "$KeycloakUrl/realms/krt-bank/protocol/openid-connect/token" `
        -Method POST `
        -ContentType "application/x-www-form-urlencoded" `
        -Body "grant_type=password&client_id=krt-bank-app&username=demo&password=demo123"
    Write-Host "  Token do usuario demo obtido com sucesso!" -ForegroundColor Green
    Write-Host "  Access token: $($testToken.access_token.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "  AVISO: Nao foi possivel obter token do demo. Verifique as credenciais." -ForegroundColor Yellow
    Write-Host "  Erro: $($_.Exception.Message)" -ForegroundColor DarkYellow
}

Write-Host ""
Write-Host "Setup concluido!" -ForegroundColor Green
Write-Host "  Admin Console: $KeycloakUrl/admin" -ForegroundColor Cyan
Write-Host "  Credenciais:   admin / admin" -ForegroundColor Cyan
Write-Host "  Realm:         krt-bank" -ForegroundColor Cyan
Write-Host "  Usuario demo:  demo / demo123" -ForegroundColor Cyan
Write-Host ""

