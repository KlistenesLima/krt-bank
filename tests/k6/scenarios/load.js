// ============================================================
// KRT Bank — LOAD TEST v2.0 (Token Cache + Tráfego Realista)
// Pool pré-criado + token cache TTL 4 min
//   40% saldo | 25% extrato | 20% dashboard | 10% PIX | 5% registro
// Ramp: 0 → 100 → 500 → 1000 → 500 → 0 VUs | ~18 min
// ============================================================

import { sleep } from 'k6';
import {
    setupUserPool, getRandomUser, ensureAuth,
    realisticBankingAction, thinkTime
} from '../lib/helpers.js';
import { USER_POOL_SIZE } from '../lib/config.js';

export const options = {
    stages: [
        { duration: '2m', target: 100 },
        { duration: '3m', target: 500 },
        { duration: '5m', target: 1000 },
        { duration: '5m', target: 1000 },
        { duration: '2m', target: 500 },
        { duration: '1m', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(50)<500', 'p(95)<2000', 'p(99)<5000'],
        http_req_failed: ['rate<0.10'],
        krt_balance_check_rate: ['rate>0.80'],
        krt_statement_check_rate: ['rate>0.80'],
        krt_dashboard_check_rate: ['rate>0.80'],
        krt_pix_success_rate: ['rate>0.70'],
    },
};

export function setup() {
    return { users: setupUserPool(USER_POOL_SIZE) };
}

export default function (data) {
    let user = getRandomUser(data.users);
    if (!user) { sleep(1); return; }

    // Token cache: só faz login se expirou (TTL 4 min)
    user = ensureAuth(user);
    if (!user.token) { sleep(1); return; }

    realisticBankingAction(user, data.users);
    thinkTime(1, 3);
}
