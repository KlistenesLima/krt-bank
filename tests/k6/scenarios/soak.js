import { sleep, group } from 'k6';
import { registerAndLogin, executePixTransfer, getStatement, getBalance, getDashboard, thinkTime } from '../lib/helpers.js';

export const options = {
    stages: [
        { duration: '5m', target: 500 },
        { duration: '110m', target: 500 },
        { duration: '5m', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<800'],
        http_req_failed: ['rate<0.02'],
        krt_pix_success_rate: ['rate>0.95'],
    },
};

export default function () {
    const user = registerAndLogin();
    if (!user) { sleep(1); return; }

    group('Full Session', () => {
        getDashboard(user.token, user.accountId);
        getBalance(user.token, user.accountId);
        thinkTime(2, 5);
        executePixTransfer(user.token, user.accountId);
        thinkTime(3, 6);
        getStatement(user.token, user.accountId);
        thinkTime(2, 4);
        if (Math.random() < 0.3) {
            executePixTransfer(user.token, user.accountId);
            thinkTime(2, 5);
        }
    });
}
