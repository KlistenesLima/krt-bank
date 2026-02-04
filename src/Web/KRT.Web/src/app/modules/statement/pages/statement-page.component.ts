import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../../core/services/account.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-statement-page',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Extrato</h1>
        <button mat-icon-button><mat-icon>filter_list</mat-icon></button>
      </header>

      <main class="container fade-in">
        <mat-card class="statement-card">
           <mat-list>
             <div mat-subheader>Hoje</div>
             <ng-container *ngFor="let item of transactions">
                <mat-list-item>
                  <mat-icon matListItemIcon [class.in]="item.amount > 0" [class.out]="item.amount < 0">
                    {{ item.amount > 0 ? 'arrow_downward' : 'arrow_upward' }}
                  </mat-icon>
                  <div matListItemTitle class="tx-title">{{ item.type }}</div>
                  <div matListItemLine class="tx-meta">{{ item.createdAt | date:'HH:mm' }} - Pix</div>
                  <div class="tx-value" [class.positive]="item.amount > 0">
                    {{ item.amount | currency:'BRL' }}
                  </div>
                </mat-list-item>
                <mat-divider></mat-divider>
             </ng-container>
             
             <div mat-subheader>Ontem</div>
             <mat-list-item>
                <mat-icon matListItemIcon class="out">arrow_upward</mat-icon>
                <div matListItemTitle>Spotify Premium</div>
                <div matListItemLine>Pagamento Cartão</div>
                <div class="tx-value">- R$ 21,90</div>
             </mat-list-item>
           </mat-list>
        </mat-card>
      </main>
    </div>
  `,
  styles: [`
    .header-simple { 
      background: var(--primary); color: white; padding: 15px; 
      display: flex; align-items: center; justify-content: space-between;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }
    .header-simple h1 { font-size: 1.2rem; margin: 0; }
    .statement-card { padding: 0; min-height: 80vh; margin-top: 10px; }
    
    .tx-title { font-weight: 600; font-size: 0.95rem; }
    .tx-meta { color: #888; font-size: 0.8rem; }
    .tx-value { font-weight: 700; margin-left: auto; }
    .tx-value.positive { color: var(--accent); }
    
    .mat-icon.in { color: var(--accent); background: rgba(0,208,158,0.1); border-radius: 50%; padding: 4px; }
    .mat-icon.out { color: #ff5252; background: rgba(255,82,82,0.1); border-radius: 50%; padding: 4px; }
  `]
})
export class StatementPageComponent implements OnInit {
  transactions: any[] = [];
  constructor(private accService: AccountService, private location: Location) {}

  ngOnInit() {
    const id = localStorage.getItem('krt_account_id');
    if(id) this.accService.getStatement(id).subscribe((res: any) => this.transactions = res || []);
  }
  goBack() { this.location.back(); }
}
