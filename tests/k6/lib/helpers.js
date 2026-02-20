// ============================================================
// KRT Bank — k6 Helpers v2.0
// TOKEN CACHE: login só quando token expira (TTL 4 min)
// Pool de usuários pré-criados para testes realistas
// ============================================================

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter, Trend } from 'k6/metrics';
import { BASE_URL, HEALTH_URL, getHeaders, generateCPF, generateEmail, generatePhone, generatePixAmount } from './config.js';

// ——— Custom Metrics ————————————————————————————————
export const pixSuccessRate = new Rate('krt_pix_success_rate');
export const pixDuration = new Trend('krt_pix_duration', true);
export const accountsCreated = new Counter('krt_accounts_created');
export const pixTransactions = new Counter('krt_pix_transactions');
export const fraudDetected = new Counter('krt_fraud_detected');
export const loginSuccess = new Rate('krt_login_success_rate');
export const balanceCheckRate = new Rate('krt_balance_check_rate');
export const statementCheckRate = new Rate('krt_statement_check_rate');
export const dashboardCheckRate = new Rate('krt_dashboard_check_rate');

// ——— Token Cache ————————————————————————————————————
// TTL de 4 minutos (Keycloak default é 5 min)
// Evita re-autenticação desnecessária a cada iteração
const TOKEN_TTL_MS = 15 * 60 * 1000; // 15 minutos (seguro para produção bancária)
const tokenCache = {};

function getCachedToken(cpf) {
    const entry = tokenCache[cpf];
    if (!entry) return null;
    if (Date.now() - entry.timestamp > TOKEN_TTL_MS) {
        delete tokenCache[cpf];
        return null;
    }
    return entry;
}

function setCachedToken(cpf, token, accountId) {
    tokenCache[cpf] = {
        token: token,
        accountId: accountId,
        timestamp: Date.now()
    };
}

// ——— Criar conta (POST /api/v1/auth/register) ——————————————
export function createAccount() {
    const cpf = generateCPF();
    const email = generateEmail();
    const password = 'K6Test@2026!';

    const res = http.post(
        `${BASE_URL}/api/v1/auth/register`,
        JSON.stringify({
            customerName: `K6 User ${Date.now()}`,
            customerDocument: cpf,
            customerEmail: email,
            customerPhone: generatePhone(),
            password: password,
            branchCode: '0001'
        }),
        { headers: getHeaders(), tags: { name: 'POST /auth/register' } }
    );

    const ok = check(res, {
        'register: status 2xx': (r) => r.status >= 200 && r.status < 300,
    });

    if (ok) accountsCreated.add(1);

    let accountId = null;
    if (ok && res.body) {
        try { accountId = JSON.parse(res.body).accountId; } catch {}
    }
    return { cpf, password, accountId, ok };
}

// ——— Login (POST /api/v1/auth/login) ————————————————————————
export function login(cpf, password) {
    const res = http.post(
        `${BASE_URL}/api/v1/auth/login`,
        JSON.stringify({ cpf, password }),
        { headers: getHeaders(), tags: { name: 'POST /auth/login' } }
    );

    const ok = check(res, {
        'login: status 200': (r) => r.status === 200,
    });
    loginSuccess.add(ok ? 1 : 0);

    if (ok && res.body) {
        try {
            const body = JSON.parse(res.body);
            const token = body.token || body.accessToken;
            const accountId = body.accountId;
            // Cachear token
            if (token) setCachedToken(cpf, token, accountId);
            return { token, accountId, ok: true };
        } catch {}
    }
    return { token: null, accountId: null, ok: false };
}

// ——— Register + Login combinado ————————————————————————————
export function registerAndLogin() {
    const account = createAccount();
    if (!account.ok) return null;

    const auth = login(account.cpf, account.password);
    if (!auth.ok) return null;

    return {
        token: auth.token,
        accountId: auth.accountId || account.accountId,
        cpf: account.cpf,
        password: account.password
    };
}

// ——— Setup: criar pool de usuários (chamado uma vez) ————————
export function setupUserPool(poolSize) {
    const users = [];
    const size = poolSize || 20;
    console.log(`Creating user pool with ${size} users...`);

    for (let i = 0; i < size; i++) {
        const user = registerAndLogin();
        if (user) {
            users.push(user);
            if ((i + 1) % 5 === 0) console.log(`  Created ${i + 1}/${size} users`);
        }
        sleep(0.5);
    }

    console.log(`User pool ready: ${users.length}/${size} users created`);
    return users;
}

// ——— Pegar usuário aleatório do pool ————————————————————————
export function getRandomUser(users) {
    if (!users || users.length === 0) return null;
    return users[Math.floor(Math.random() * users.length)];
}

// ——— Garantir token válido (com cache!) ————————————————————
// ANTES: fazia login SEMPRE (Keycloak saturava)
// AGORA: só faz login se token expirou (TTL 4 min)
export function ensureAuth(user) {
    if (!user || !user.cpf) return user;

    // Verificar cache primeiro
    const cached = getCachedToken(user.cpf);
    if (cached) {
        user.token = cached.token;
        user.accountId = cached.accountId || user.accountId;
        return user;
    }

    // Token expirado ou inexistente — fazer login
    const auth = login(user.cpf, user.password || 'K6Test@2026!');
    if (auth.ok) {
        user.token = auth.token;
        user.accountId = auth.accountId || user.accountId;
    }
    return user;
}

// Manter compatibilidade com nome antigo
export const refreshAuth = ensureAuth;

// ——— PIX Transfer (POST /api/v1/pix) ————————————————————————
export function executePixTransfer(token, sourceAccountId) {
    const payload = JSON.stringify({
        sourceAccountId: sourceAccountId,
        pixKey: generateCPF(),
        amount: generatePixAmount(),
        description: `k6 test ${Date.now()}`,
        idempotencyKey: `k6-${Date.now()}-${Math.random().toString(36).slice(2)}`
    });

    const start = Date.now();
    const res = http.post(
        `${BASE_URL}/api/v1/pix`,
        payload,
        { headers: getHeaders(token), tags: { name: 'POST /pix' } }
    );
    pixDuration.add(Date.now() - start);
    pixTransactions.add(1);

    const success = check(res, {
        'pix: status 2xx': (r) => r.status >= 200 && r.status < 300,
    });
    pixSuccessRate.add(success ? 1 : 0);

    if (res.status === 422 || res.status === 400) {
        try {
            if (JSON.parse(res.body).message.toLowerCase().includes('fraud')) fraudDetected.add(1);
        } catch {}
    }

    // Se 401, invalidar cache para forçar re-login
    if (res.status === 401) {
        // Token expirou no meio — próxima iteração vai re-autenticar
        delete tokenCache[sourceAccountId];
    }

    return res;
}

// ——— Consulta Saldo (GET /api/v1/accounts/{id}/balance) ————
export function getBalance(token, accountId) {
    const res = http.get(
        `${BASE_URL}/api/v1/accounts/${accountId}/balance`,
        { headers: getHeaders(token), tags: { name: 'GET /balance' } }
    );
    balanceCheckRate.add(res.status >= 200 && res.status < 300 ? 1 : 0);
    return res;
}

// ——— Extrato (GET /api/v1/statement/{accountId}) ——————————
export function getStatement(token, accountId) {
    const res = http.get(
        `${BASE_URL}/api/v1/statement/${accountId}`,
        { headers: getHeaders(token), tags: { name: 'GET /statement' } }
    );
    statementCheckRate.add(res.status >= 200 && res.status < 300 ? 1 : 0);
    return res;
}

// ——— Dashboard (GET /api/v1/dashboard/summary/{accountId}) ——
export function getDashboard(token, accountId) {
    const res = http.get(
        `${BASE_URL}/api/v1/dashboard/summary/${accountId}`,
        { headers: getHeaders(token), tags: { name: 'GET /dashboard' } }
    );
    dashboardCheckRate.add(res.status >= 200 && res.status < 300 ? 1 : 0);
    return res;
}

// ——— Health Check ———————————————————————————————————————————
export function healthCheck() {
    return http.get(HEALTH_URL, { tags: { name: 'GET /health' } });
}

// ——— Think time —————————————————————————————————————————————
export function thinkTime(min = 1, max = 3) {
    sleep(Math.random() * (max - min) + min);
}

// ——— Distribuição de tráfego realista ———————————————————————
// Simula padrão real de banco digital:
//   40% consulta saldo | 25% extrato | 20% dashboard | 10% PIX | 5% registro
export function realisticBankingAction(user, users) {
    const roll = Math.random() * 100;

    if (roll < 40) {
        // 40% — Consulta saldo
        getBalance(user.token, user.accountId);
    } else if (roll < 65) {
        // 25% — Extrato
        getStatement(user.token, user.accountId);
    } else if (roll < 85) {
        // 20% — Dashboard
        getDashboard(user.token, user.accountId);
    } else if (roll < 95) {
        // 10% — PIX Transfer
        executePixTransfer(user.token, user.accountId);
    } else {
        // 5% — Novo registro + login
        const newUser = registerAndLogin();
        if (newUser && users) users.push(newUser);
    }
}

