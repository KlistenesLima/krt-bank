import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter, Trend } from 'k6/metrics';
import { BASE_URL, getHeaders, generateCPF, generateEmail, generatePhone } from './config.js';

export const pixSuccessRate = new Rate('krt_pix_success_rate');
export const pixDuration = new Trend('krt_pix_duration', true);
export const accountsCreated = new Counter('krt_accounts_created');
export const pixTransactions = new Counter('krt_pix_transactions');
export const fraudDetected = new Counter('krt_fraud_detected');
export const loginSuccess = new Rate('krt_login_success_rate');

// POST /api/v1/auth/register → cria conta publica
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

// POST /api/v1/auth/login
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
            return { token: body.token || body.accessToken, accountId: body.accountId, ok: true };
        } catch {}
    }
    return { token: null, accountId: null, ok: false };
}

// Register + Login combinado
export function registerAndLogin() {
    const account = createAccount();
    if (!account.ok) return null;

    const auth = login(account.cpf, account.password);
    if (!auth.ok) return null;

    return {
        token: auth.token,
        accountId: auth.accountId || account.accountId,
        cpf: account.cpf
    };
}

// POST /api/v1/pix
export function executePixTransfer(token, sourceAccountId) {
    const payload = JSON.stringify({
        sourceAccountId: sourceAccountId,
        pixKey: generateCPF(),
        amount: parseFloat((Math.random() * 999 + 1).toFixed(2)),
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
    return res;
}

// GET /api/v1/statement/{accountId}
export function getStatement(token, accountId) {
    return http.get(`${BASE_URL}/api/v1/statement/${accountId}`,
        { headers: getHeaders(token), tags: { name: 'GET /statement' } });
}

// GET /api/v1/accounts/{id}/balance
export function getBalance(token, accountId) {
    return http.get(`${BASE_URL}/api/v1/accounts/${accountId}/balance`,
        { headers: getHeaders(token), tags: { name: 'GET /balance' } });
}

// GET /api/v1/dashboard/summary/{accountId}
export function getDashboard(token, accountId) {
    return http.get(`${BASE_URL}/api/v1/dashboard/summary/${accountId}`,
        { headers: getHeaders(token), tags: { name: 'GET /dashboard' } });
}

// Health check (direto na payments API, gateway nao roteia)
export function healthCheck() {
    return http.get('http://localhost:5002/api/v1/health', { tags: { name: 'GET /health' } });
}

export function thinkTime(min = 1, max = 3) {
    sleep(Math.random() * (max - min) + min);
}

