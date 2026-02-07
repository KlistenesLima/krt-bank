import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-statement-page',
  template: `
    <div class="statement-container page-with-nav">
      <header class="page-header">
        <button mat-icon-button (click)="router.navigate(['/dashboard'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Extrato</h1>
        <div style="width:40px"></div>
      </header>

      <!-- Balance summary -->
      <div class="balance-summary">
        <div class="summary-item">
          <span class="summary-label">Saldo atual</span>
          <span class="summary-value">{{ balance | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
        </div>
      </div>

      <!-- Filters -->
      <div class="filters">
        <button class="filter-chip" [class.active]="filter === 'all'" (click)="filter = 'all'">Todos</button>
        <button class="filter-chip" [class.active]="filter === 'CREDIT'" (click)="filter = 'CREDIT'">Entradas</button>
        <button class="filter-chip" [class.active]="filter === 'DEBIT'" (click)="filter = 'DEBIT'">Saídas</button>
      </div>

      <!-- Transactions -->
      <div class="tx-list">
        <div *ngIf="filteredTransactions().length === 0" class="empty-state">
          <mat-icon>receipt_long</mat-icon>
          <p>Nenhuma movimentação</p>
        </div>

        <div class="tx-item" *ngFor="let t of filteredTransactions()">
          <div class="tx-icon" [class.credit]="t.type === 'CREDIT'">
            <mat-icon>{{ t.type === 'CREDIT' ? 'south_west' : 'north_east' }}</mat-icon>
          </div>
          <div class="tx-details">
            <span class="tx-desc">{{ t.description }}</span>
            <span class="tx-date">{{ formatDate(t.createdAt) }}</span>
          </div>
          <span class="tx-amount" [class.credit]="t.type === 'CREDIT'">
            {{ t.type === 'CREDIT' ? '+' : '-' }}{{ t.amount | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
          </span>
        </div>
      </div>
    </div>

    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .statement-container { min-height: 100vh; background: var(--krt-bg); }
    .page-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 20px; background: white; border-bottom: 1px solid var(--krt-divider);
    }
    .page-header h1 { font-size: 1.1rem; font-weight: 700; margin: 0; }

    .balance-summary {
      background: white; padding: 20px; margin: 16px 20px;
      border-radius: var(--krt-radius); box-shadow: var(--krt-shadow-sm);
    }
    .summary-label { font-size: 0.82rem; color: var(--krt-text-muted); display: block; }
    .summary-value { font-size: 1.5rem; font-weight: 800; color: var(--krt-text); }

    .filters {
      display: flex; gap: 8px; padding: 0 20px 16px; overflow-x: auto;
    }
    .filter-chip {
      background: white; border: 1.5px solid var(--krt-border); border-radius: 20px;
      padding: 8px 18px; font-size: 0.82rem; font-weight: 600;
      cursor: pointer; white-space: nowrap; transition: all 0.2s;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .filter-chip.active {
      background: var(--krt-primary); color: white; border-color: var(--krt-primary);
    }

    .tx-list {
      padding: 0 20px 20px; max-width: 500px; margin: 0 auto;
    }
    .empty-state {
      text-align: center; padding: 40px;
      background: white; border-radius: var(--krt-radius);
    }
    .empty-state mat-icon { font-size: 40px; width: 40px; height: 40px; color: var(--krt-text-muted); }
    .empty-state p { color: var(--krt-text-muted); margin-top: 8px; }

    .tx-item {
      display: flex; align-items: center; gap: 12px;
      background: white; padding: 16px; margin-bottom: 8px;
      border-radius: var(--krt-radius-sm); box-shadow: var(--krt-shadow-sm);
    }
    .tx-icon {
      width: 40px; height: 40px; border-radius: 12px;
      background: #FFF0F0; color: var(--krt-danger);
      display: flex; align-items: center; justify-content: center; flex-shrink: 0;
    }
    .tx-icon.credit { background: #E8F5E9; color: var(--krt-success); }
    .tx-details { flex: 1; }
    .tx-desc { display: block; font-size: 0.88rem; font-weight: 500; }
    .tx-date { font-size: 0.75rem; color: var(--krt-text-muted); }
    .tx-amount { font-weight: 700; font-size: 0.9rem; color: var(--krt-danger); white-space: nowrap; }
    .tx-amount.credit { color: var(--krt-success); }
  `]
})
export class StatementPageComponent implements OnInit {
  transactions: any[] = [];
  balance = 0;
  filter = 'all';

  constructor(public router: Router) {}

  ngOnInit() {
    this.balance = parseFloat(localStorage.getItem('krt_account_balance') || '0');
    this.transactions = JSON.parse(localStorage.getItem('krt_transactions') || '[]');
  }

  filteredTransactions() {
    if (this.filter === 'all') return this.transactions;
    return this.transactions.filter((t: any) => t.type === this.filter);
  }

  formatDate(d: string): string {
    if (!d) return '';
    const date = new Date(d);
    return date.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' }) + ' · '
         + date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
  }
}
