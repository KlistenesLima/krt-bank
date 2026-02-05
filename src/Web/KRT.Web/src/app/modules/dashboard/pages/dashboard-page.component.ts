import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../../core/services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  template: `
    <div class="app-layout">
      <div *ngIf="loading" class="skeleton-layout">
         <div class="header-section sk-header"></div>
         <main class="container sk-main">
            <div class="skeleton sk-card"></div>
            <div class="sk-grid">
               <div class="skeleton sk-circle"></div>
               <div class="skeleton sk-circle"></div>
               <div class="skeleton sk-circle"></div>
               <div class="skeleton sk-circle"></div>
            </div>
            <div class="skeleton sk-text" style="width: 50%"></div>
            <div class="skeleton sk-card" style="height: 200px"></div>
         </main>
      </div>

      <div *ngIf="account && !loading" class="fade-in">
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

          <main class="container main-content">
            <mat-card class="balance-card">
                <mat-card-content>
                    <div class="balance-top">
                        <span class="label">Saldo disponível</span>
                        <button mat-icon-button (click)="toggleEye()">
                            <mat-icon>{{ showBalance ? 'visibility' : 'visibility_off' }}</mat-icon>
                        </button>
                    </div>
                    <div class="balance-value" [class.blur-text]="!showBalance">
                        {{ showBalance ? (balance | currency:'BRL') : 'R$ ••••' }}
                    </div>
                </mat-card-content>
            </mat-card>

            <div class="shortcuts-grid">
                <button class="shortcut-btn" (click)="goToPix()">
                    <div class="shortcut-icon"><mat-icon>account_balance</mat-icon></div>
                    <span class="shortcut-label">Pix</span>
                </button>
                <button class="shortcut-btn" (click)="goToBoleto()">
                    <div class="shortcut-icon"><mat-icon>qr_code_scanner</mat-icon></div>
                    <span class="shortcut-label">Pagar</span>
                </button>
                <button class="shortcut-btn" (click)="goToInvestments()">
                    <div class="shortcut-icon"><mat-icon>savings</mat-icon></div>
                    <span class="shortcut-label">Investir</span>
                </button>
                <button class="shortcut-btn" (click)="goToRecharge()">
                    <div class="shortcut-icon"><mat-icon>smartphone</mat-icon></div>
                    <span class="shortcut-label">Recarga</span>
                </button>
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
                                <div class="tx-amount" [class.positive]="item.amount > 0" [class.blur-text]="!showBalance">
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

      <app-bottom-nav></app-bottom-nav>
    </div>
  `,
  styles: [`
    .header-section { background: var(--primary); color: white; padding-bottom: 70px; border-radius: 0 0 24px 24px; box-shadow: 0 4px 12px rgba(0,71,187,0.3); }
    .header-content { display: flex; justify-content: space-between; align-items: center; padding-top: 20px; }
    .user-greeting { display: flex; flex-direction: column; cursor: pointer; }
    .hello { font-weight: 300; font-size: 1rem; opacity: 0.9; }
    .name { font-weight: 700; font-size: 1.4rem; display: flex; align-items: center; }
    .tiny-icon { font-size: 18px; margin-left: 5px; }
    .logout-btn { color: white; background: rgba(255,255,255,0.2); border-radius: 12px; }
    .main-content { margin-top: -60px; padding-bottom: 20px; }
    .balance-card { margin-bottom: 25px; padding: 10px; }
    .balance-top { display: flex; justify-content: space-between; color: var(--text-secondary); margin-bottom: 8px; font-size: 0.9rem; align-items: center; }
    .balance-value { font-size: 2.2rem; font-weight: 700; color: var(--primary-dark); transition: filter 0.3s; }
    .blur-text { filter: blur(6px); opacity: 0.6; user-select: none; }
    .shortcuts-grid { display: flex; justify-content: space-between; gap: 10px; margin-bottom: 30px; }
    .section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
    .statement-list { padding: 0; overflow: hidden; }
    .empty-state { padding: 40px; text-align: center; color: #ccc; }
    .tx-icon { color: var(--primary); background: rgba(0,71,187,0.05); border-radius: 50%; padding: 8px; }
    .tx-title { font-weight: 600; }
    .tx-amount { font-weight: 700; margin-left: auto; font-size: 1rem; transition: filter 0.3s; }
    .tx-amount.positive { color: var(--accent); }
    mat-list-item { cursor: pointer; }
    mat-list-item:hover { background-color: #f9f9f9; }
    
    /* SKELETON STYLES */
    .sk-header { height: 120px; background: #e0e0e0; border-radius: 0 0 24px 24px; }
    .sk-main { margin-top: -60px; }
    .sk-grid { display: flex; justify-content: space-between; margin-bottom: 30px; }
  `]
})
export class DashboardPageComponent implements OnInit {
  account: any;
  balance: number = 0;
  statement: any[] = [];
  loading = true;
  showBalance = true;
  accountId = localStorage.getItem('krt_account_id');

  constructor(private accountService: AccountService, private router: Router) {}

  ngOnInit() {
    if(!this.accountId) { this.router.navigate(['/login']); return; }
    const savedState = localStorage.getItem('krt_show_balance');
    if (savedState !== null) this.showBalance = savedState === 'true';

    // Tempo ajustado para ver o efeito sem piscar rápido demais
    setTimeout(() => { this.loadData(); }, 1200); 
  }

  loadData() {
    this.accountService.getById(this.accountId!).subscribe({
        next: (res) => {
            if (typeof res === 'string') this.account = { customerName: 'Cliente KRT' };
            else this.account = res;
            this.accountService.getBalance(this.accountId!).subscribe((b: any) => this.balance = b.availableAmount || 0);
            this.accountService.getStatement(this.accountId!).subscribe((s: any) => {
                this.statement = s || [];
                this.loading = false;
            });
        },
        error: () => { this.router.navigate(['/login']); this.loading = false; }
    });
  }
  
  toggleEye() {
      this.showBalance = !this.showBalance;
      localStorage.setItem('krt_show_balance', String(this.showBalance));
  }

  goToPix() { this.router.navigate(['/pix']); }
  goToBoleto() { this.router.navigate(['/boleto']); }
  goToInvestments() { this.router.navigate(['/investments']); }
  goToRecharge() { this.router.navigate(['/recharge']); }
  goToStatement() { this.router.navigate(['/extract']); }
  goToProfile() { this.router.navigate(['/profile']); }
  goToCards() { this.router.navigate(['/cards']); }
  goToReceipt(id: string) { this.router.navigate(['/receipt', id]); }
  logout() { localStorage.removeItem('krt_account_id'); this.router.navigate(['/login']); }
}
