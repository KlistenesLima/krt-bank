export const environment = {
  production: false,
  // Tudo via Gateway YARP — nunca chamar serviços diretamente
  apiUrl: 'http://localhost:5000/api/v1',
  // URLs diretas (só para fallback/debug)
  gatewayUrl: 'http://localhost:5000',
  onboardingDirectUrl: 'http://localhost:5001/api/v1',
  paymentsDirectUrl: 'http://localhost:5002/api/v1'
};
