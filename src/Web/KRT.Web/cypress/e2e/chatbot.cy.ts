describe('Chatbot', () => {
  beforeEach(() => {
    cy.intercept('GET', '**/chatbot/suggestions', { statusCode: 200, body: ['Ver saldo', 'Fazer Pix'] }).as('getSuggestions');
    cy.intercept('POST', '**/chatbot/message', {
      statusCode: 200,
      body: { response: 'Seu saldo e R$ 12.450,80.', category: 'conta', suggestions: ['Ver extrato'], confidence: 0.92 }
    }).as('sendMessage');
    cy.visit('/chatbot');
  });

  it('should show welcome message', () => {
    cy.get('.msg-bubble').first().should('contain', 'assistente');
  });

  it('should send and receive messages', () => {
    cy.get('.chat-input input').type('saldo{enter}');
    cy.wait('@sendMessage');
    cy.get('.msg.user .msg-bubble').should('contain', 'saldo');
    cy.get('.msg.bot .msg-bubble').last().should('contain', 'saldo', { timeout: 3000 });
  });

  it('should show suggestion buttons', () => {
    cy.get('.msg-suggestions button').should('have.length.at.least', 1);
  });
});