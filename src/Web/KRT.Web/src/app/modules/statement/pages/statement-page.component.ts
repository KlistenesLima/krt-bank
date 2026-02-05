import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { AccountService } from '../../../core/services/account.service';
import { Router } from '@angular/router';

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
        <mat-chip-listbox aria-label="Seleção de filtro">
            <mat-chip-option selected (click)="filter('all')">Tudo</mat-chip-option>
            <mat-chip-option (click)="filter('in')">Entradas</mat-chip-option>
            <mat-chip-option (click)="filter('out')">Saídas</mat-chip-option>
            <mat-chip-option (click)="filter('future')">Futuros</mat-chip-option>
        </mat-chip-listbox>
      </div>

      <main class="container fade-in">
        <div *ngIf="filteredStatement.length === 0" class="empty-state">
            <mat-icon>search_off</mat-icon>
            <p>Nenhuma transação encontrada.</p>
        </div>

        <mat-card class="statement-card">
           <mat-list>
              <ng-container *ngFor="let item of filteredStatement">
                 <mat-list-item (click)="goToReceipt(item)">
                    <mat-icon matListItemIcon [class.in]="item.amount > 0" [class.out]="item.amount < 0">
                        {{ item.amount > 0 ? 'arrow_downward' : 'arrow_upward' }}
                    </mat-icon>
                    <div matListItemTitle>{{ item.type }}</div>
                    <div matListItemLine class="tx-meta">{{ item.createdAt | date:'dd/MM HH:mm' }}</div>
                    <div class="tx-amount" [class.positive]="item.amount > 0">
                       {{ item.amount | currency:'BRL' }}
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
    .filters-container { padding: 10px 20px; background: white; overflow-x: auto; white-space: nowrap; }
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
  rawStatement: any[] = [];
  filteredStatement: any[] = [];
  constructor(private location: Location, private accService: AccountService, private router: Router) {}
  ngOnInit() {
    const id = localStorage.getItem('krt_account_id');
    if(id) {
        this.accService.getStatement(id).subscribe((res: any) => {
            this.rawStatement = res || [];
            this.filteredStatement = this.rawStatement;
        });
    }
  }
  filter(type: string) {
      if (type === 'all') this.filteredStatement = this.rawStatement;
      else if (type === 'in') this.filteredStatement = this.rawStatement.filter(x => x.amount > 0);
      else if (type === 'out') this.filteredStatement = this.rawStatement.filter(x => x.amount < 0);
      else this.filteredStatement = [];
  }
  goToReceipt(item: any) { this.router.navigate(['/receipt', 'TX-' + Math.floor(Math.random() * 10000)]); }
  goBack() { this.location.back(); }
}
