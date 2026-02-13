// ============================================================
// KRT Bank — BREAKPOINT TEST v2.0 (Token Cache + Tráfego Realista)
//   40% saldo | 25% extrato | 20% dashboard | 10% PIX | 5% registro
// Ramp contínuo: 0 → 20.000 VUs em 30 min
// ============================================================

import { sleep } from 'k6';
import {
    setupUserPool, getRandomUser, ensureAuth,
    realisticBankingAction, thinkTime
} from '../lib/helpers.js';

export const options = {
    executor: 'ramping-arrival-rate',
    startRate: 10,
    timeUnit: '1s',
    preAllocatedVUs: 5000,
    maxVUs: 20000,
    stages: [
        { duration: '2m', target: 50 },
        { duration: '3m', target: 200 },
        { duration: '5m', target: 500 },
        { duration: '5m', target: 1000 },
        { duration: '5m', target: 2000 },
        { duration: '5m', target: 5000 },
        { duration: '5m', target: 10000 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<5000'],
        http_req_failed: ['rate<0.20'],
    },
};

export function setup() {
    return { users: setupUserPool(100) };
}

export default function (data) {
    let user = getRandomUser(data.users);
    if (!user) { sleep(0.2); return; }

    user = ensureAuth(user);
    if (!user.token) { sleep(0.2); return; }

    realisticBankingAction(user, data.users);
    thinkTime(0.2, 0.5);
}

