// ============================================================
// KRT Bank — SMOKE TEST
// Validação funcional de todos os endpoints
// VUs: 5 | Duration: 1 min
// ============================================================

import { check, sleep } from 'k6';
import http from 'k6/http';
import { BASE_URL, getHeaders, generateCPF, generateEmail, generatePhone } from '../lib/config.js';

export const options = {
    vus: 5,
    duration: '1m',
    thresholds: {
        http_req_duration: ['p(95)<2000'],
        http_req_failed: ['rate<0.30'],
    },
};

export default function () {
    // 1. Health check (direto na Payments API)
    const health = http.get('http://localhost:5002/api/v1/health', { tags: { name: 'GET /health' } });
    check(health, { 'health: status 200': (r) => r.status === 200 });

    // 2. Register
    const cpf = generateCPF();
    const pw = 'SmokeTest@2026!';
    const reg = http.post(`${BASE_URL}/api/v1/auth/register`, JSON.stringify({
        customerName: `Smoke ${Date.now()}`,
        customerDocument: cpf,
        customerEmail: generateEmail(),
        customerPhone: generatePhone(),
        password: pw,
        branchCode: '0001'
    }), { headers: getHeaders(), tags: { name: 'POST /auth/register' } });

    check(reg, { 'register: status 2xx': (r) => r.status >= 200 && r.status < 300 });

    // 3. Login + PIX + Reads
    if (reg.status >= 200 && reg.status < 300) {
        const loginRes = http.post(`${BASE_URL}/api/v1/auth/login`, JSON.stringify({
            cpf: cpf, password: pw
        }), { headers: getHeaders(), tags: { name: 'POST /auth/login' } });

        const loginOk = check(loginRes, { 'login: status 200': (r) => r.status === 200 });

        if (loginOk) {
            try {
                const body = JSON.parse(loginRes.body);
                const token = body.token || body.accessToken;
                const accountId = body.accountId;

                if (token && accountId) {
                    // Balance
                    const bal = http.get(`${BASE_URL}/api/v1/accounts/${accountId}/balance`,
                        { headers: getHeaders(token), tags: { name: 'GET /balance' } });
                    check(bal, { 'balance: status 2xx': (r) => r.status >= 200 && r.status < 300 });

                    // Statement
                    const stmt = http.get(`${BASE_URL}/api/v1/statement/${accountId}`,
                        { headers: getHeaders(token), tags: { name: 'GET /statement' } });
                    check(stmt, { 'statement: status 2xx': (r) => r.status >= 200 && r.status < 300 });

                    // Dashboard
                    const dash = http.get(`${BASE_URL}/api/v1/dashboard/summary/${accountId}`,
                        { headers: getHeaders(token), tags: { name: 'GET /dashboard' } });
                    check(dash, { 'dashboard: status 2xx': (r) => r.status >= 200 && r.status < 300 });

                    // PIX
                    const pix = http.post(`${BASE_URL}/api/v1/pix`, JSON.stringify({
                        sourceAccountId: accountId,
                        pixKey: generateCPF(),
                        amount: 10.50,
                        description: 'k6 smoke',
                        idempotencyKey: `smoke-${Date.now()}`
                    }), { headers: getHeaders(token), tags: { name: 'POST /pix' } });
                    check(pix, { 'pix: status 2xx': (r) => r.status >= 200 && r.status < 300 });
                }
            } catch {}
        }
    }

    sleep(1);
}
