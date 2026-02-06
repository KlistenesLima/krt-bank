# ==========================================
# SCRIPT DE PROVA DE FOGO: FLUXO E2E v3 🚀
# ==========================================

$keycloakUrl = "http://localhost:8080"
$urlOnb = "http://localhost:5000"
$urlPay = "http://localhost:5002"
$ErrorActionPreference = "Stop"

Write-Host "--- 1. AUTENTICAÇÃO (Keycloak) ---" -ForegroundColor Cyan
$bodyAuth = @{
    grant_type = "password"
    client_id  = "krt-api"
    username   = "tester"
    password   = "123"
}

$jwt = $null
$accountId = $null

try {
    # 1. LOGIN
    $tokenResp = Invoke-RestMethod -Uri "$keycloakUrl/realms/krt-bank/protocol/openid-connect/token" -Method Post -Body $bodyAuth -ContentType "application/x-www-form-urlencoded"
    $jwt = $tokenResp.access_token
    Write-Host "   [OK] Acesso Permitido. Token JWT capturado." -ForegroundColor Green
} catch {
    Write-Host "   [ERRO] Falha no Login Keycloak." -ForegroundColor Red
    Write-Host "   Detalhe: $_" -ForegroundColor Gray
    exit
}

if ($jwt) {
    $headers = @{ Authorization = "Bearer $jwt" }
    $email = "ceo.$(Get-Random)@krt.com"

    Write-Host "`n--- 2. MICROSERVIÇO: ONBOARDING (Criar Conta - BLINDADO) ---" -ForegroundColor Cyan
    $bodyConta = @{ CustomerName = "CEO KRT Seguro"; CustomerDocument = "12345678900"; CustomerEmail = $email } | ConvertTo-Json

    try {
        # 2. CHAMADA API ONBOARDING (Agora exige Token!)
        $respConta = Invoke-RestMethod -Uri "$urlOnb/api/v1/accounts" -Method Post -Body $bodyConta -Headers $headers -ContentType "application/json"
        $accountId = $respConta.id
        Write-Host "   [OK] Conta Criada com Sucesso! ID: $accountId" -ForegroundColor Green
    } catch { 
        Write-Host "   [ERRO ONBOARDING] O token foi rejeitado (401) ou Erro de Negócio (400)." -ForegroundColor Red
        if ($_.Exception.Response) {
             $reader = New-Object System.IO.StreamReader $_.Exception.Response.GetResponseStream()
             Write-Host "   Server Response: $($reader.ReadToEnd())" -ForegroundColor Yellow
        }
        exit
    }
}

if ($accountId) {
    Write-Host "`n--- 3. MICROSERVIÇO: PAYMENTS (Pix + Rastreabilidade) ---" -ForegroundColor Cyan
    $bodyPix = @{ SourceAccountId = $accountId; DestinationAccountId = [Guid]::NewGuid(); Amount = 100.00; PixKey = "test"; Description = "Seguro" } | ConvertTo-Json

    try {
        # 3. CHAMADA API PAYMENTS
        $response = Invoke-WebRequest -Uri "$urlPay/api/v1/pix/transfer" -Method Post -Body $bodyPix -Headers $headers -ContentType "application/json"
        $cid = $response.Headers["X-Correlation-Id"]
        Write-Host "   [OK] Pix Enviado!" -ForegroundColor Green
    } catch {
        # Validação de Regra de Negócio (Esperamos 422 - Saldo Insuficiente, pois a conta nasce zerada)
        if ($_.Exception.Response.StatusCode.value__ -eq 422) {
             Write-Host "   [OK] Ciclo Completo: Pix processado e negado por saldo (Esperado)." -ForegroundColor Green
             
             if ($_.Exception.Response.Headers["X-Correlation-Id"]) {
                 $cid = $_.Exception.Response.Headers["X-Correlation-Id"]
                 Write-Host "`n--- RASTREABILIDADE (SEQ) ---" -ForegroundColor Yellow
                 Write-Host "Correlation ID: $cid" -ForegroundColor White
                 Write-Host "Link do Trace: http://localhost:5341/#/events?filter=@Properties['CorrelationId']%20%3D%20'$cid'" -ForegroundColor Cyan
             }
        } else { 
            Write-Host "   [ERRO PAYMENTS] $_" -ForegroundColor Red 
            if ($_.Exception.Response) {
                $reader = New-Object System.IO.StreamReader $_.Exception.Response.GetResponseStream()
                Write-Host "   Server Response: $($reader.ReadToEnd())" -ForegroundColor Yellow
           }
        }
    }
}

Write-Host "`n------------------------------------------------" -ForegroundColor Gray
Read-Host "Pressione ENTER para encerrar..."
