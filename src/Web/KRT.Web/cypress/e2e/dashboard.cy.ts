describe('Dashboard', () => {
  beforeEach(() => {
    cy.intercept('GET', '**/dashboard/summary/**', {
      statusCode: 200,
      body: { balance: 12450.80, incomeThisMonth: 8200, expensesThisMonth: 5100, totalTransactions: 42 }
    }).as('getSummary');

    cy.intercept('GET', '**/dashboard/balance-history/**', {
      statusCode: 200,
      body: { history: Array.from({ length: 30 }, (_, i) => ({ date: `2026-01-${String(i+1).padStart(2, '0')}`, balance: 10000 + Math.random() * 5000 })) }
    }).as('getHistory');

    cy.intercept('GET', '**/dashboard/spending-categories/**', {
      statusCode: 200,
      body: { categories: [{ category: 'Alimentacao', amount: 1250, color: '#FF6384' }] }
    }).as('getCategories');

    cy.intercept('GET', '**/dashboard/monthly-summary/**', {
      statusCode: 200,
      body: { months: [{ month: 'Jan/26', income: 8000, expenses: 5000 }] }
    }).as('getMonthly');

    cy.visit('/dashboard-charts');
  });

  it('should display dashboard title', () => {
    cy.get('h2').should('contain', 'Dashboard');
  });

  it('should show summary cards after loading', () => {
    cy.wait('@getSummary');
    cy.get('.card-stat').should('have.length', 4);
    cy.get('.card-stat.blue .value').should('contain', '12');
  });

  it('should render chart containers', () => {
    cy.get('.chart-container').should('have.length.at.least', 1);
    cy.get('canvas').should('exist');
  });
});