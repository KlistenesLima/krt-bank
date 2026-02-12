import { sleep, group } from 'k6';
import { registerAndLogin, executePixTransfer, getStatement, getBalance, thinkTime } from '../lib/helpers.js';

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
        http_req_duration: ['p(50)<500', 'p(95)<2000', 'p(99)<5000'],
        http_req_failed: ['rate<0.10'],
        krt_pix_success_rate: ['rate>0.85'],
    },
};

export default function () {
    const user = registerAndLogin();
    if (!user) { sleep(0.5); return; }

    group('PIX Under Stress', () => {
        executePixTransfer(user.token, user.accountId);
        thinkTime(0.5, 2);
    });

    group('Read Under Stress', () => {
        getBalance(user.token, user.accountId);
        getStatement(user.token, user.accountId);
        thinkTime(0.5, 1);
    });
}
