import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { PaymentService } from '../../../core/services/payment.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-statement-page',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Extrato</h1>
        <button mat-icon-button><mat-icon>filter_list</mat-icon></button>
      </header>

      <div class="filters-container">
        <mat-chip-listbox aria-label="Filtro">
            <mat-chip-option selected (click)="filter('all')">Tudo</mat-chip-option>
            <mat-chip-option (click)="filter('sent')">Enviados</mat-chip-option>
            <mat-chip-option (click)="filter('received')">Recebidos</mat-chip-option>
        </mat-chip-listbox>
      </div>

      <main class="container fade-in">
        <div *ngIf="filteredList.length === 0" class="empty-state">
            <mat-icon>search_off</mat-icon>
            <p>Nenhuma transação encontrada.</p>
        </div>

        <mat-card class="statement-card" *ngIf="filteredList.length > 0">
           <mat-list>
              <ng-container *ngFor="let item of filteredList">
                 <mat-list-item (click)="goToReceipt(item.id)">
                    <mat-icon matListItemIcon [class.in]="item.sourceAccountId !== accountId" [class.out]="item.sourceAccountId === accountId">
                        {{ item.sourceAccountId === accountId ? 'arrow_upward' : 'arrow_downward' }}
                    </mat-icon>
                    <div matListItemTitle>Pix {{ item.sourceAccountId === accountId ? 'Enviado' : 'Recebido' }}</div>
                    <div matListItemLine class="tx-meta">{{ item.createdAt | date:'dd/MM/yyyy HH:mm' }} · {{ item.status }}</div>
                    <div class="tx-amount" [class.positive]="item.sourceAccountId !== accountId">
                       {{ item.sourceAccountId === accountId ? '-' : '+' }}{{ item.amount | currency:'BRL' }}
                    </div>
                 </mat-list-item>
                 <mat-divider></mat-divider>
              </ng-container>
           </mat-list>
        </mat-card>
      </main>
    </div>
  `,
  styles: [`
    .filters-container { padding: 10px 20px; background: white; overflow-x: auto; }
    .empty-state { text-align: center; padding: 40px; color: #999; }
    .empty-state mat-icon { font-size: 48px; height: 48px; width: 48px; margin-bottom: 10px; opacity: 0.5; }
    .statement-card { padding: 0; }
    .in { color: var(--accent); background: rgba(0,208,158,0.1); border-radius: 50%; padding: 8px; }
    .out { color: #ff5252; background: rgba(255,82,82,0.1); border-radius: 50%; padding: 8px; }
    .tx-amount { font-weight: 700; margin-left: auto; }
    .tx-amount.positive { color: var(--accent); }
    .tx-meta { font-size: 0.8rem; color: #888; }
    mat-list-item { cursor: pointer; }
    mat-list-item:hover { background-color: #f5f5f5; }
  `]
})
export class StatementPageComponent implements OnInit {
  allTransactions: any[] = [];
  filteredList: any[] = [];
  accountId: string | null;

  constructor(
    private location: Location,
    private paymentService: PaymentService,
    private authService: AuthService,
    private router: Router
  ) {
    this.accountId = this.authService.accountId;
  }

  ngOnInit() {
    if (!this.accountId) return;
    this.paymentService.getHistory(this.accountId).subscribe({
      next: (res: any) => {
        this.allTransactions = res?.data || res || [];
        this.filteredList = this.allTransactions;
      },
      error: () => { this.allTransactions = []; this.filteredList = []; }
    });
  }

  filter(type: string) {
    if (type === 'all') this.filteredList = this.allTransactions;
    else if (type === 'sent') this.filteredList = this.allTransactions.filter(x => x.sourceAccountId === this.accountId);
    else if (type === 'received') this.filteredList = this.allTransactions.filter(x => x.sourceAccountId !== this.accountId);
  }

  goToReceipt(id: string) { this.router.navigate(['/receipt', id]); }
  goBack() { this.location.back(); }
}
