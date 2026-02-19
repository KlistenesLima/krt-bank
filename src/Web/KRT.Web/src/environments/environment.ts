export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  adminApiKey: 'REDACTED_ADMIN_KEY',
  keycloak: {
    url: 'http://localhost:8080',
    realm: 'krt-bank',
    clientId: 'krt-bank-app'
  }
};
