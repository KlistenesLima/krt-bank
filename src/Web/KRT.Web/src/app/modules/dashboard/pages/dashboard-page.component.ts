import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../../core/services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  template: `
    <div class="app-layout" *ngIf="account">
      <header class="header-section">
        <div class="header-content container">
            <div class="user-greeting" (click)="goToProfile()">
                <span class="hello">Olá,</span>
                <span class="name">{{ account.customerName }} <mat-icon class="tiny-icon">expand_more</mat-icon></span>
            </div>
            <button mat-icon-button class="logout-btn" (click)="logout()">
                <mat-icon>power_settings_new</mat-icon>
            </button>
        </div>
      </header>

      <main class="container main-content fade-in">
        <mat-card class="balance-card">
            <mat-card-content>
                <div class="balance-top">
                    <span class="label">Saldo disponível</span>
                    <mat-icon>visibility</mat-icon>
                </div>
                <div class="balance-value">
                    {{ balance | currency:'BRL' }}
                </div>
            </mat-card-content>
        </mat-card>

        <div class="shortcuts-grid">
            <div class="shortcut" (click)="goToPix()">
                <div class="icon-box"><mat-icon>pix</mat-icon></div>
                <span>Área Pix</span>
            </div>
            <div class="shortcut" (click)="goToCards()">
                <div class="icon-box"><mat-icon>credit_card</mat-icon></div> <span>Cartões</span>
            </div>
            <div class="shortcut">
                <div class="icon-box"><mat-icon>payments</mat-icon></div>
                <span>Pagar</span>
            </div>
            <div class="shortcut" (click)="goToStatement()">
                <div class="icon-box"><mat-icon>receipt_long</mat-icon></div>
                <span>Extrato</span>
            </div>
        </div>

        <div class="statement-section">
            <div class="section-header">
               <h3>Últimas transações</h3>
               <button mat-button color="primary" (click)="goToStatement()">Ver tudo</button>
            </div>
            
            <mat-card class="statement-list">
                <mat-list>
                    <div *ngIf="statement.length === 0" class="empty-state">
                        <mat-icon>savings</mat-icon>
                        <p>Nenhuma movimentação recente.</p>
                    </div>
                    <ng-container *ngFor="let item of statement.slice(0, 3)">
                        <mat-list-item (click)="goToReceipt('123')">
                            <mat-icon matListItemIcon class="tx-icon">payments</mat-icon>
                            <div matListItemTitle class="tx-title">{{ item.type }}</div>
                            <div matListItemLine class="tx-date">{{ item.createdAt | date:'dd/MM HH:mm' }}</div>
                            <div class="tx-amount" [class.positive]="item.amount > 0">
                                {{ item.amount | currency:'BRL' }}
                            </div>
                        </mat-list-item>
                        <mat-divider></mat-divider>
                    </ng-container>
                </mat-list>
            </mat-card>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .header-section { background: var(--primary); color: white; padding-bottom: 60px; border-radius: 0 0 24px 24px; box-shadow: 0 4px 12px rgba(0,71,187,0.3); }
    .header-content { display: flex; justify-content: space-between; align-items: center; padding-top: 20px; }
    .user-greeting { display: flex; flex-direction: column; cursor: pointer; }
    .hello { font-weight: 300; font-size: 1rem; opacity: 0.9; }
    .name { font-weight: 700; font-size: 1.4rem; display: flex; align-items: center; }
    .tiny-icon { font-size: 18px; width: 18px; height: 18px; margin-left: 5px; }
    .logout-btn { color: white; background: rgba(255,255,255,0.2); border-radius: 12px; }
    .main-content { margin-top: -50px; }
    .balance-card { margin-bottom: 30px; padding: 10px; }
    .balance-top { display: flex; justify-content: space-between; color: var(--text-secondary); margin-bottom: 8px; font-size: 0.9rem; }
    .balance-value { font-size: 2.2rem; font-weight: 700; color: var(--primary-dark); }
    .shortcuts-grid { display: flex; justify-content: space-between; gap: 10px; margin-bottom: 35px; }
    .shortcut { display: flex; flex-direction: column; align-items: center; cursor: pointer; width: 25%; }
    .icon-box { width: 56px; height: 56px; border-radius: 20px; background: var(--white); color: var(--primary); box-shadow: 0 4px 12px rgba(0,0,0,0.05); display: flex; justify-content: center; align-items: center; margin-bottom: 10px; transition: transform 0.2s; }
    .shortcut:hover .icon-box { transform: translateY(-3px); color: var(--accent); }
    .shortcut span { font-size: 0.85rem; font-weight: 500; color: var(--text-secondary); }
    .section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
    .statement-list { padding: 0; overflow: hidden; }
    .empty-state { padding: 40px; text-align: center; color: #ccc; }
    .tx-icon { color: var(--primary); background: rgba(0,71,187,0.05); border-radius: 50%; padding: 8px; }
    .tx-title { font-weight: 600; }
    .tx-amount { font-weight: 700; margin-left: auto; font-size: 1rem; }
    .tx-amount.positive { color: var(--accent); }
    mat-list-item { cursor: pointer; }
    mat-list-item:hover { background-color: #f9f9f9; }
  `]
})
export class DashboardPageComponent implements OnInit {
  account: any;
  balance: number = 0;
  statement: any[] = [];
  accountId = localStorage.getItem('krt_account_id');

  constructor(private accountService: AccountService, private router: Router) {}

  ngOnInit() {
    if(!this.accountId) { this.router.navigate(['/login']); return; }
    
    this.accountService.getById(this.accountId).subscribe(res => {
        if (typeof res === 'string') this.account = { customerName: 'Cliente KRT' };
        else this.account = res;
    });
    
    this.accountService.getBalance(this.accountId).subscribe((res: any) => this.balance = res.availableAmount || 0);
    this.accountService.getStatement(this.accountId).subscribe((res: any) => this.statement = res || []);
  }
  goToPix() { this.router.navigate(['/pix']); }
  goToStatement() { this.router.navigate(['/extract']); }
  goToProfile() { this.router.navigate(['/profile']); }
  goToCards() { this.router.navigate(['/cards']); }
  goToReceipt(id: string) { this.router.navigate(['/receipt', id]); }
  
  logout() {
    localStorage.removeItem('krt_account_id');
    this.router.navigate(['/login']);
  }
}
