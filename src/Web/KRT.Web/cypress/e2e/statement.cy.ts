describe('Statement', () => {
  beforeEach(() => {
    cy.intercept('GET', '**/statement/**', {
      statusCode: 200,
      body: {
        items: [
          { id: '1', date: '2026-01-15T10:30:00Z', type: 'PIX_SENT', category: 'Alimentacao', amount: -250, description: 'Pix enviado', counterpartyName: 'Maria Silva', isCredit: false },
          { id: '2', date: '2026-01-14T09:00:00Z', type: 'PIX_RECEIVED', category: 'Outros', amount: 1500, description: 'Pix recebido', counterpartyName: 'Joao Santos', isCredit: true }
        ],
        totalItems: 2, totalPages: 1,
        summary: { totalIncome: 1500, totalExpenses: 250, net: 1250 }
      }
    }).as('getStatement');

    cy.visit('/statement');
  });

  it('should display statement page', () => {
    cy.get('h2').should('contain', 'Extrato');
  });

  it('should show transactions table', () => {
    cy.wait('@getStatement');
    cy.get('tbody tr').should('have.length', 2);
  });

  it('should show summary bar', () => {
    cy.wait('@getStatement');
    cy.get('.summary-bar').should('exist');
    cy.get('.income').should('contain', '1');
  });

  it('should have export buttons', () => {
    cy.get('.btn-export.csv').should('exist');
    cy.get('.btn-export.pdf').should('exist');
  });

  it('should have filter controls', () => {
    cy.get('.filters-panel').should('exist');
    cy.get('.btn-filter').should('exist');
    cy.get('.btn-clear').should('exist');
  });
});