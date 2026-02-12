export const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export function getHeaders(token) {
    const headers = { 'Content-Type': 'application/json', 'Accept': 'application/json' };
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return headers;
}

export function generateCPF() {
    const rand = (max) => Math.floor(Math.random() * max);
    const n = Array.from({ length: 9 }, () => rand(9));
    let d1 = n.reduce((sum, v, i) => sum + v * (10 - i), 0) % 11;
    d1 = d1 < 2 ? 0 : 11 - d1;
    n.push(d1);
    let d2 = n.reduce((sum, v, i) => sum + v * (11 - i), 0) % 11;
    d2 = d2 < 2 ? 0 : 11 - d2;
    n.push(d2);
    return n.join('');
}

export function generateEmail() {
    return `k6_${Date.now()}_${Math.floor(Math.random() * 99999)}@krtbank.com`;
}

export function generatePhone() {
    return `+55119${Math.floor(Math.random() * 99999999).toString().padStart(8, '0')}`;
}
