describe('Chatbot', () => {
  beforeEach(() => {
    cy.visit('/chatbot');
  });

  it('should display chatbot page', () => {
    cy.url().should('include', '/chatbot');
  });

  it('should show welcome message', () => {
    cy.get('.message, .chat-message, .welcome').should('exist');
  });

  it('should have input field for messages', () => {
    cy.get('input[type=text], textarea, .chat-input').should('exist');
  });

  it('should send a message', () => {
    cy.get('input[type=text], textarea, .chat-input').first().type('saldo');
    cy.get('button[type=submit], .send-button, button').contains(/enviar|send/i).click();
    cy.get('.message, .chat-message').should('have.length.greaterThan', 1);
  });

  it('should show suggestion chips', () => {
    cy.get('.suggestion, .chip, .quick-reply').should('exist');
  });
});
