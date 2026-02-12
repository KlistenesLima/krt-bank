import { sleep } from 'k6';
import { registerAndLogin, executePixTransfer, thinkTime } from '../lib/helpers.js';

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
        http_req_failed: ['rate<0.15'],
        krt_pix_success_rate: ['rate>0.80'],
    },
};

export default function () {
    const user = registerAndLogin();
    if (!user) { sleep(0.3); return; }
    executePixTransfer(user.token, user.accountId);
    thinkTime(0.3, 1);
}
