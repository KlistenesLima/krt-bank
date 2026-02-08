// Cypress E2E support file
Cypress.Commands.add('setAccountId', (id: string = '00000000-0000-0000-0000-000000000001') => {
  cy.window().then(win => win.localStorage.setItem('account_id', id));
});

Cypress.Commands.add('interceptApi', () => {
  cy.intercept('GET', '**/api/v1/**', { statusCode: 200, body: {} }).as('apiCall');
});

declare namespace Cypress {
  interface Chainable {
    setAccountId(id?: string): Chainable<void>;
    interceptApi(): Chainable<void>;
  }
}