Cypress.Commands.add('login', () => {
  cy.visit('/');
});

declare namespace Cypress {
  interface Chainable {
    login(): Chainable<void>;
  }
}
