describe('Statement (Extrato)', () => {
  beforeEach(() => {
    cy.visit('/statement');
  });

  it('should display statement page', () => {
    cy.url().should('include', '/statement');
  });

  it('should show transaction table or list', () => {
    cy.get('table, .transaction-list, .statement-list').should('exist');
  });

  it('should have filter controls', () => {
    cy.get('input, select, .filter, [data-cy=filter]').should('exist');
  });

  it('should have export buttons', () => {
    cy.get('button').contains(/csv|pdf|export/i).should('exist');
  });

  it('should support pagination', () => {
    cy.get('.pagination, .paginator, [data-cy=pagination], button').should('exist');
  });
});
