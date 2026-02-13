// ============================================================
// KRT Bank — SPIKE TEST v2.0 (Token Cache + Tráfego Realista)
//   40% saldo | 25% extrato | 20% dashboard | 10% PIX | 5% registro
// Spike: 100 → 10.000 VUs em 10s
// ============================================================

import { sleep } from 'k6';
import {
    setupUserPool, getRandomUser, ensureAuth,
    realisticBankingAction, thinkTime
} from '../lib/helpers.js';

export const options = {
    stages: [
        { duration: '30s', target: 100 },
        { duration: '1m', target: 100 },
        { duration: '10s', target: 10000 },
        { duration: '3m', target: 10000 },
        { duration: '10s', target: 100 },
        { duration: '2m', target: 100 },
        { duration: '30s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(50)<1000', 'p(95)<5000'],
        http_req_failed: ['rate<0.20'],
        krt_balance_check_rate: ['rate>0.60'],
    },
};

export function setup() {
    return { users: setupUserPool(80) };
}

export default function (data) {
    let user = getRandomUser(data.users);
    if (!user) { sleep(0.3); return; }

    user = ensureAuth(user);
    if (!user.token) { sleep(0.3); return; }

    realisticBankingAction(user, data.users);
    thinkTime(0.3, 1);
}

