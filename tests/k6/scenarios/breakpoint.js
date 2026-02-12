import { sleep } from 'k6';
import { registerAndLogin, executePixTransfer, thinkTime } from '../lib/helpers.js';

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
        http_req_duration: ['p(95)<3000'],
        http_req_failed: ['rate<0.15'],
    },
};

export default function () {
    const user = registerAndLogin();
    if (!user) { sleep(0.2); return; }
    executePixTransfer(user.token, user.accountId);
    thinkTime(0.2, 0.5);
}
