# ============================================================
# KRT Bank - Keycloak Setup
# Pré-req: docker-compose up -d (Keycloak rodando em localhost:8080)
# ============================================================

Write-Host "Configurando Keycloak para KRT Bank..." -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Acesse: http://localhost:8080/admin" -ForegroundColor White
Write-Host "   Login: admin / admin" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Crie um Realm chamado: krt-bank" -ForegroundColor White
Write-Host "   - No menu esquerdo, clique no dropdown 'master'" -ForegroundColor Gray
Write-Host "   - Clique 'Create realm'" -ForegroundColor Gray
Write-Host "   - Nome: krt-bank" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Crie um Client:" -ForegroundColor White
Write-Host "   - Clients > Create client" -ForegroundColor Gray
Write-Host "   - Client ID: krt-web" -ForegroundColor Gray
Write-Host "   - Client authentication: OFF (public client)" -ForegroundColor Gray
Write-Host "   - Standard flow: ON" -ForegroundColor Gray
Write-Host "   - Direct access grants: ON" -ForegroundColor Gray
Write-Host "   - Valid redirect URIs: http://localhost:4200/*" -ForegroundColor Gray
Write-Host "   - Web origins: http://localhost:4200" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Crie um usuário de teste:" -ForegroundColor White
Write-Host "   - Users > Add user" -ForegroundColor Gray
Write-Host "   - Username: joao" -ForegroundColor Gray
Write-Host "   - Email: joao@krt.com" -ForegroundColor Gray
Write-Host "   - First name: Joao" -ForegroundColor Gray
Write-Host "   - Last name: Silva" -ForegroundColor Gray
Write-Host "   - Email verified: ON" -ForegroundColor Gray
Write-Host "   - Save > Credentials > Set password: 123456 (Temporary: OFF)" -ForegroundColor Gray
Write-Host ""
Write-Host "5. Teste o token:" -ForegroundColor White
Write-Host '   curl -X POST http://localhost:8080/realms/krt-bank/protocol/openid-connect/token \' -ForegroundColor Yellow
Write-Host '     -d "client_id=krt-web" \' -ForegroundColor Yellow
Write-Host '     -d "username=joao" \' -ForegroundColor Yellow
Write-Host '     -d "password=123456" \' -ForegroundColor Yellow
Write-Host '     -d "grant_type=password"' -ForegroundColor Yellow
Write-Host ""
Write-Host "6. Use o access_token no header:" -ForegroundColor White
Write-Host '   Authorization: Bearer <token>' -ForegroundColor Yellow
Write-Host ""
