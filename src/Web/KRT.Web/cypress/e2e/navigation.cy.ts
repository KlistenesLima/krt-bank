describe('Navigation', () => {
  const routes = [
    { path: '/dashboard-charts', title: 'Dashboard' },
    { path: '/statement', title: 'Extrato' },
    { path: '/contacts', title: 'Contatos' },
    { path: '/goals', title: 'Metas' },
    { path: '/notifications', title: 'Notifica' },
    { path: '/chatbot', title: 'KRT Assistant' },
    { path: '/admin', title: 'Painel' }
  ];

  routes.forEach(route => {
    it(`should navigate to ${route.path}`, () => {
      cy.intercept('GET', '**/api/v1/**', { statusCode: 200, body: {} });
      cy.intercept('POST', '**/api/v1/**', { statusCode: 200, body: {} });
      cy.visit(route.path);
      cy.url().should('include', route.path);
    });
  });
});