// ============================================================
// KRT Bank — SOAK TEST v2.0 (Token Cache + Tráfego Realista)
//   40% saldo | 25% extrato | 20% dashboard | 10% PIX | 5% registro
// VUs: 500 constantes | Duration: 2 horas
// ============================================================

import { sleep } from 'k6';
import {
    setupUserPool, getRandomUser, ensureAuth,
    realisticBankingAction, thinkTime
} from '../lib/helpers.js';

export const options = {
    stages: [
        { duration: '5m', target: 500 },
        { duration: '110m', target: 500 },
        { duration: '5m', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<1000'],
        http_req_failed: ['rate<0.05'],
        krt_balance_check_rate: ['rate>0.90'],
        krt_statement_check_rate: ['rate>0.90'],
        krt_pix_success_rate: ['rate>0.90'],
    },
};

export function setup() {
    return { users: setupUserPool(100) };
}

export default function (data) {
    let user = getRandomUser(data.users);
    if (!user) { sleep(1); return; }

    user = ensureAuth(user);
    if (!user.token) { sleep(1); return; }

    realisticBankingAction(user, data.users);
    thinkTime(2, 5);
}

