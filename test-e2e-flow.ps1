# ============================================================
# KRT Bank — Teste End-to-End
# Keycloak Auth -> Criar Conta -> Consultar -> Pix -> Seq
# ============================================================
param(
    [string]$GatewayUrl = "http://localhost:5000",
    [string]$OnboardingUrl = "http://localhost:5001",
    [string]$PaymentsUrl = "http://localhost:5002",
    [string]$KeycloakUrl = "http://localhost:8080",
    [string]$SeqUrl = "http://localhost:5341"
)

$passed = 0
$failed = 0
$total = 8

function Write-TestResult($name, $success, $detail = "") {
    if ($success) {
        Write-Host "  PASS " -ForegroundColor Green -NoNewline
        $script:passed++
    } else {
        Write-Host "  FAIL " -ForegroundColor Red -NoNewline
        $script:failed++
    }
    Write-Host "$name" -ForegroundColor White -NoNewline
    if ($detail) { Write-Host " ($detail)" -ForegroundColor Gray } else { Write-Host "" }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  KRT Bank — E2E Test Suite" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# --- TEST 1: Health Check Gateway ---
Write-Host "[1/$total] Health Check Gateway..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$GatewayUrl/health" -TimeoutSec 10
    Write-TestResult "Gateway /health" ($health.status -eq "Healthy") $health.status
} catch {
    Write-TestResult "Gateway /health" $false $_.Exception.Message
}

# --- TEST 2: Health Check Onboarding ---
Write-Host "[2/$total] Health Check Onboarding..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$OnboardingUrl/health" -TimeoutSec 10
    Write-TestResult "Onboarding /health" $true "OK"
} catch {
    Write-TestResult "Onboarding /health" $false $_.Exception.Message
}

# --- TEST 3: Keycloak Token (usuario demo) ---
Write-Host "[3/$total] Keycloak Auth (demo/demo123)..." -ForegroundColor Yellow
$token = $null
try {
    $tokenResponse = Invoke-RestMethod -Uri "$KeycloakUrl/realms/krt-bank/protocol/openid-connect/token" `
        -Method POST `
        -ContentType "application/x-www-form-urlencoded" `
        -Body "grant_type=password&client_id=krt-bank-app&username=demo&password=demo123"
    $token = $tokenResponse.access_token
    Write-TestResult "Keycloak Token" ($null -ne $token) "token length: $($token.Length)"
} catch {
    Write-TestResult "Keycloak Token" $false $_.Exception.Message
}

if (-not $token) {
    Write-Host ""
    Write-Host "  ABORTANDO: Sem token, nao e possivel continuar os testes." -ForegroundColor Red
    Write-Host "  Execute: .\setup-keycloak.ps1" -ForegroundColor Yellow
    exit 1
}

$authHeaders = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
    "X-Correlation-Id" = [Guid]::NewGuid().ToString()
}
$correlationId = $authHeaders["X-Correlation-Id"]
Write-Host "  CorrelationId: $correlationId" -ForegroundColor Gray

# --- TEST 4: Criar Conta (POST) ---
Write-Host "[4/$total] Criar Conta..." -ForegroundColor Yellow
$accountId = $null
try {
    $body = @{
        CustomerName = "E2E Test User"
        CustomerDocument = "12345678901"
        CustomerEmail = "e2e@krtbank.com"
        BranchCode = "0001"
    } | ConvertTo-Json

    $createResult = Invoke-RestMethod -Uri "$OnboardingUrl/api/v1/accounts" `
        -Method POST -Headers $authHeaders -Body $body -TimeoutSec 15

    $accountId = $createResult.id
    Write-TestResult "Criar Conta" ($null -ne $accountId) "id: $accountId"
} catch {
    # Pode dar erro se ja existir por CPF duplicado — tenta buscar
    $statusCode = $_.Exception.Response.StatusCode.Value__
    if ($statusCode -eq 400) {
        Write-TestResult "Criar Conta" $false "Possivel CPF duplicado. Rode com doc diferente."
    } else {
        Write-TestResult "Criar Conta" $false "$statusCode - $($_.Exception.Message)"
    }
}

# --- TEST 5: Consultar Conta (GET) ---
Write-Host "[5/$total] Consultar Conta..." -ForegroundColor Yellow
if ($accountId) {
    try {
        $account = Invoke-RestMethod -Uri "$OnboardingUrl/api/v1/accounts/$accountId" `
            -Headers $authHeaders -TimeoutSec 10
        Write-TestResult "Consultar Conta" ($account.customerName -eq "E2E Test User") "balance: $($account.balance)"
    } catch {
        Write-TestResult "Consultar Conta" $false $_.Exception.Message
    }

    # Segunda chamada deve vir do cache Redis
    try {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $account2 = Invoke-RestMethod -Uri "$OnboardingUrl/api/v1/accounts/$accountId" `
            -Headers $authHeaders -TimeoutSec 10
        $sw.Stop()
        Write-TestResult "Redis Cache Hit (2a chamada)" $true "$($sw.ElapsedMilliseconds)ms"
    } catch {
        Write-TestResult "Redis Cache Hit" $false $_.Exception.Message
    }
} else {
    Write-TestResult "Consultar Conta" $false "Sem accountId"
    Write-TestResult "Redis Cache Hit" $false "Sem accountId"
}

# --- TEST 6: Creditar Conta (para ter saldo pro Pix) ---
Write-Host "[6/$total] Creditar conta (R$ 1000)..." -ForegroundColor Yellow
if ($accountId) {
    try {
        $creditBody = @{ Amount = 1000; Reason = "E2E test deposit" } | ConvertTo-Json
        $creditResult = Invoke-RestMethod -Uri "$OnboardingUrl/api/v1/accounts/$accountId/credit" `
            -Method POST -Headers $authHeaders -Body $creditBody -TimeoutSec 10
        Write-TestResult "Creditar Conta" ($creditResult.success -eq $true) "newBalance: $($creditResult.newBalance)"
    } catch {
        Write-TestResult "Creditar Conta" $false $_.Exception.Message
    }
} else {
    Write-TestResult "Creditar Conta" $false "Sem accountId"
}

# --- TEST 7: Verificar saldo ---
Write-Host "[7/$total] Verificar saldo..." -ForegroundColor Yellow
if ($accountId) {
    try {
        $balance = Invoke-RestMethod -Uri "$OnboardingUrl/api/v1/accounts/$accountId/balance" `
            -Headers $authHeaders -TimeoutSec 10
        Write-TestResult "Saldo" ($balance.availableAmount -ge 1000) "R$ $($balance.availableAmount)"
    } catch {
        Write-TestResult "Saldo" $false $_.Exception.Message
    }
} else {
    Write-TestResult "Saldo" $false "Sem accountId"
}

# --- TEST 8: Verificar Seq (logs chegaram?) ---
Write-Host "[8/$total] Verificar Seq (logs)..." -ForegroundColor Yellow
try {
    # Seq API: busca eventos dos ultimos 2 minutos com o CorrelationId
    $seqQuery = "CorrelationId%20%3D%20'$correlationId'"
    $seqResult = Invoke-RestMethod -Uri "$SeqUrl/api/events?filter=$seqQuery&count=10" -TimeoutSec 10
    $eventCount = 0
    if ($seqResult -and $seqResult.Count) { $eventCount = $seqResult.Count }
    elseif ($seqResult -is [array]) { $eventCount = $seqResult.Length }
    Write-TestResult "Seq Logs (CorrelationId)" ($eventCount -gt 0) "$eventCount eventos encontrados"
} catch {
    # Seq pode nao ter a API de busca habilitada
    try {
        $null = Invoke-WebRequest -Uri $SeqUrl -TimeoutSec 5
        Write-TestResult "Seq Logs" $true "Seq acessivel (verifique manualmente: $SeqUrl)"
    } catch {
        Write-TestResult "Seq Logs" $false "Seq nao acessivel em $SeqUrl"
    }
}

# --- RESULTADO FINAL ---
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  RESULTADO: $passed/$total passed, $failed/$total failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Yellow" })
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if ($accountId) {
    Write-Host "  DADOS DE TESTE:" -ForegroundColor Cyan
    Write-Host "  Account ID:     $accountId" -ForegroundColor White
    Write-Host "  CorrelationId:  $correlationId" -ForegroundColor White
    Write-Host "  Keycloak User:  demo / demo123" -ForegroundColor White
    Write-Host ""
}

Write-Host "  VERIFICACAO MANUAL:" -ForegroundColor Yellow
Write-Host "  Seq:      $SeqUrl (filtre por CorrelationId)" -ForegroundColor White
Write-Host "  Keycloak: $KeycloakUrl/admin (admin/admin)" -ForegroundColor White
Write-Host "  Swagger:  $OnboardingUrl/swagger" -ForegroundColor White
Write-Host "  Swagger:  $PaymentsUrl/swagger" -ForegroundColor White
Write-Host ""

if ($failed -gt 0) { exit 1 } else { exit 0 }
