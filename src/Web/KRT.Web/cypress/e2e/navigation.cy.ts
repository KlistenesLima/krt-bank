describe('Navigation', () => {
  const routes = [
    { path: '/dashboard', name: 'Dashboard' },
    { path: '/statement', name: 'Extrato' },
    { path: '/contacts', name: 'Contatos' },
    { path: '/boletos', name: 'Boletos' },
    { path: '/scheduled-pix', name: 'Pix Agendado' },
    { path: '/virtual-card', name: 'Cartao Virtual' },
    { path: '/notifications', name: 'Notificacoes' },
    { path: '/profile', name: 'Perfil' },
    { path: '/chatbot', name: 'Chatbot' },
    { path: '/goals', name: 'Metas' },
  ];

  routes.forEach(({ path, name }) => {
    it('should navigate to ' + name + ' (' + path + ')', () => {
      cy.visit(path);
      cy.url().should('include', path);
      cy.get('body').should('not.contain', 'Cannot GET');
    });
  });
});
