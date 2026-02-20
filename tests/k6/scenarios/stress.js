// ============================================================
// KRT Bank — STRESS TEST v2.0 (Token Cache + Tráfego Realista)
//   40% saldo | 25% extrato | 20% dashboard | 10% PIX | 5% registro
// Ramp: 0 → 500 → 2000 → 5000 → 2000 → 0 VUs | ~21 min
// ============================================================

import { sleep } from 'k6';
import {
    setupUserPool, getRandomUser, ensureAuth,
    realisticBankingAction, thinkTime
} from '../lib/helpers.js';

export const options = {
    stages: [
        { duration: '2m', target: 500 },
        { duration: '3m', target: 2000 },
        { duration: '5m', target: 5000 },
        { duration: '5m', target: 5000 },
        { duration: '3m', target: 2000 },
        { duration: '2m', target: 500 },
        { duration: '1m', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(50)<500', 'p(95)<3000', 'p(99)<8000'],
        http_req_failed: ['rate<0.15'],
        krt_balance_check_rate: ['rate>0.70'],
        krt_pix_success_rate: ['rate>0.60'],
    },
};

export function setup() {
    return { users: setupUserPool(80) };
}

export default function (data) {
    let user = getRandomUser(data.users);
    if (!user) { sleep(0.5); return; }

    user = ensureAuth(user);
    if (!user.token) { sleep(0.5); return; }

    realisticBankingAction(user, data.users);
    thinkTime(0.5, 2);
}

