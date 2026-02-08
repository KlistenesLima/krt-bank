describe('Dashboard', () => {
  beforeEach(() => {
    cy.visit('/dashboard');
  });

  it('should display dashboard page', () => {
    cy.url().should('include', '/dashboard');
  });

  it('should show balance card', () => {
    cy.get('[data-cy=balance-card], .balance-card, .card').should('exist');
  });

  it('should display chart elements', () => {
    cy.get('canvas, .chart-container, app-dashboard-charts').should('exist');
  });

  it('should show loading state initially', () => {
    cy.get('.loading, .spinner, [data-cy=loading]').should('exist');
  });
});
