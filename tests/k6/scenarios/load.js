import { sleep, group } from 'k6';
import { registerAndLogin, executePixTransfer, getStatement, getBalance, getDashboard, thinkTime } from '../lib/helpers.js';

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
        http_req_duration: ['p(50)<300', 'p(95)<800', 'p(99)<1500'],
        http_req_failed: ['rate<0.10'],
        krt_pix_success_rate: ['rate>0.85'],
        krt_pix_duration: ['p(95)<800'],
    },
};

export default function () {
    const user = registerAndLogin();
    if (!user) { sleep(1); return; }

    group('Dashboard', () => {
        getDashboard(user.token, user.accountId);
        getBalance(user.token, user.accountId);
        thinkTime(1, 2);
    });

    group('PIX Transfer', () => {
        executePixTransfer(user.token, user.accountId);
        thinkTime(2, 4);
    });

    group('Statement', () => {
        getStatement(user.token, user.accountId);
        thinkTime(1, 2);
    });

    if (Math.random() > 0.5) {
        group('Second PIX', () => {
            executePixTransfer(user.token, user.accountId);
            thinkTime(1, 3);
        });
    }
}
