import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { AccountService } from '../../../core/services/account.service';

@Component({
  selector: 'app-dashboard-page',
  template: `
    <div class="dashboard-container page-with-nav">
      <!-- HEADER -->
      <header class="dash-header">
        <div class="header-content">
          <div class="user-info">
            <div class="avatar">{{ getInitials() }}</div>
            <div class="greeting">
              <span class="greeting-text">{{ getGreeting() }}</span>
              <h2 class="user-name">{{ userName }}</h2>
            </div>
          </div>
          <div class="header-actions">
            <button class="icon-btn" (click)="router.navigate(['/inbox'])">
              <mat-icon>notifications_none</mat-icon>
            </button>
            <button class="icon-btn" (click)="logout()">
              <mat-icon>logout</mat-icon>
            </button>
          </div>
        </div>
      </header>

      <!-- BALANCE CARD -->
      <section class="balance-section fade-in">
        <div class="balance-card">
          <div class="balance-top">
            <span class="balance-label">Saldo disponível</span>
            <button class="eye-btn" (click)="toggleEye()">
              <mat-icon>{{ showBalance ? 'visibility' : 'visibility_off' }}</mat-icon>
            </button>
          </div>
          <div class="balance-amount" [class.hidden-balance]="!showBalance">
            {{ showBalance ? (balance | currency:'BRL':'symbol':'1.2-2':'pt-BR') : '••••••' }}
          </div>
          <div class="balance-bottom" *ngIf="showBalance">
            <span class="account-info">Ag 0001 · Conta {{ accountId?.substring(0, 8) }}</span>
          </div>
        </div>
      </section>

      <!-- QUICK ACTIONS -->
      <section class="actions-section">
        <h3 class="section-title">Atalhos</h3>
        <div class="actions-grid">
          <button class="action-card" (click)="router.navigate(['/pix'])">
            <div class="action-icon pix"><mat-icon>flash_on</mat-icon></div>
            <span>Pix</span>
          </button>
          <button class="action-card" (click)="router.navigate(['/boleto'])">
            <div class="action-icon boleto"><mat-icon>receipt_long</mat-icon></div>
            <span>Boleto</span>
          </button>
          <button class="action-card" (click)="router.navigate(['/extract'])">
            <div class="action-icon extract"><mat-icon>swap_vert</mat-icon></div>
            <span>Extrato</span>
          </button>
          <button class="action-card" (click)="router.navigate(['/cards'])">
            <div class="action-icon cards"><mat-icon>credit_card</mat-icon></div>
            <span>Cartões</span>
          </button>
          <button class="action-card" (click)="router.navigate(['/recharge'])">
            <div class="action-icon recharge"><mat-icon>phone_android</mat-icon></div>
            <span>Recarga</span>
          </button>
          <button class="action-card" (click)="router.navigate(['/investments'])">
            <div class="action-icon invest"><mat-icon>trending_up</mat-icon></div>
            <span>Investir</span>
          </button>
        </div>
      </section>

      <!-- PIX KEYS -->
      <section class="pix-keys-section" (click)="router.navigate(['/pix/keys'])">
        <div class="pix-keys-card">
          <div class="pix-keys-left">
            <mat-icon class="pix-icon">vpn_key</mat-icon>
            <div>
              <strong>Minhas chaves Pix</strong>
              <p>Gerencie suas chaves</p>
            </div>
          </div>
          <mat-icon>chevron_right</mat-icon>
        </div>
      </section>

      <!-- RECENT TRANSACTIONS -->
      <section class="transactions-section">
        <div class="section-header">
          <h3 class="section-title">Movimentações recentes</h3>
          <a class="see-all" (click)="router.navigate(['/extract'])">Ver tudo</a>
        </div>

        <div *ngIf="transactions.length === 0" class="empty-state">
          <mat-icon>account_balance_wallet</mat-icon>
          <p>Nenhuma movimentação ainda</p>
          <span>Faça seu primeiro Pix!</span>
        </div>

        <div class="transaction-list" *ngIf="transactions.length > 0">
          <div class="transaction-item" *ngFor="let t of transactions">
            <div class="tx-icon" [class.credit]="t.type === 'CREDIT'">
              <mat-icon>{{ t.type === 'CREDIT' ? 'south_west' : 'north_east' }}</mat-icon>
            </div>
            <div class="tx-info">
              <span class="tx-desc">{{ t.description }}</span>
              <span class="tx-date">{{ t.createdAt | date:'dd/MM · HH:mm' }}</span>
            </div>
            <span class="tx-amount" [class.credit]="t.type === 'CREDIT'">
              {{ t.type === 'CREDIT' ? '+' : '-' }}{{ t.amount | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
            </span>
          </div>
        </div>
      </section>
    </div>

    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .dashboard-container {
      min-height: 100vh;
      background: var(--krt-bg);
    }

    /* HEADER */
    .dash-header {
      background: var(--krt-gradient);
      padding: 20px 20px 60px;
      border-radius: 0 0 32px 32px;
    }
    .header-content {
      display: flex; justify-content: space-between; align-items: center;
      max-width: 500px; margin: 0 auto;
    }
    .user-info { display: flex; align-items: center; gap: 12px; }
    .avatar {
      width: 44px; height: 44px; border-radius: 14px;
      background: rgba(255,255,255,0.2); color: white;
      display: flex; align-items: center; justify-content: center;
      font-weight: 700; font-size: 0.95rem;
      backdrop-filter: blur(10px);
    }
    .greeting-text { color: rgba(255,255,255,0.7); font-size: 0.8rem; }
    .user-name { color: white; font-size: 1.1rem; font-weight: 600; margin: 0; }
    .header-actions { display: flex; gap: 4px; }
    .icon-btn {
      background: rgba(255,255,255,0.12); border: none; border-radius: 12px;
      width: 40px; height: 40px; display: flex; align-items: center; justify-content: center;
      color: white; cursor: pointer; transition: background 0.2s;
    }
    .icon-btn:hover { background: rgba(255,255,255,0.2); }
    .icon-btn mat-icon { font-size: 22px; width: 22px; height: 22px; }

    /* BALANCE CARD */
    .balance-section {
      padding: 0 20px; margin-top: -40px;
      max-width: 500px; margin-left: auto; margin-right: auto;
    }
    .balance-card {
      background: white; border-radius: var(--krt-radius-lg); padding: 24px;
      box-shadow: var(--krt-shadow-lg);
    }
    .balance-top { display: flex; justify-content: space-between; align-items: center; }
    .balance-label { font-size: 0.85rem; color: var(--krt-text-secondary); font-weight: 500; }
    .eye-btn {
      background: none; border: none; cursor: pointer;
      color: var(--krt-text-muted); padding: 4px;
    }
    .balance-amount {
      font-size: 2rem; font-weight: 800; color: var(--krt-text);
      margin: 8px 0 4px; letter-spacing: -0.5px;
    }
    .hidden-balance { color: var(--krt-text-muted); letter-spacing: 4px; }
    .balance-bottom { margin-top: 4px; }
    .account-info { font-size: 0.78rem; color: var(--krt-text-muted); }

    /* ACTIONS */
    .actions-section {
      padding: 24px 20px 0;
      max-width: 500px; margin: 0 auto;
    }
    .section-title {
      font-size: 1rem; font-weight: 700; color: var(--krt-text);
      margin-bottom: 16px;
    }
    .actions-grid {
      display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px;
    }
    .action-card {
      background: white; border: none; border-radius: var(--krt-radius);
      padding: 16px 8px; display: flex; flex-direction: column;
      align-items: center; gap: 8px; cursor: pointer;
      box-shadow: var(--krt-shadow-sm);
      transition: all 0.2s;
    }
    .action-card:hover { transform: translateY(-2px); box-shadow: var(--krt-shadow); }
    .action-card span { font-size: 0.78rem; font-weight: 600; color: var(--krt-text); }
    .action-icon {
      width: 48px; height: 48px; border-radius: 14px;
      display: flex; align-items: center; justify-content: center;
    }
    .action-icon mat-icon { font-size: 24px; width: 24px; height: 24px; color: white; }
    .action-icon.pix { background: linear-gradient(135deg, #00D4AA, #00B894); }
    .action-icon.boleto { background: linear-gradient(135deg, #FF6B35, #E55D2B); }
    .action-icon.extract { background: linear-gradient(135deg, #7C4DFF, #651FFF); }
    .action-icon.cards { background: linear-gradient(135deg, #0047BB, #003399); }
    .action-icon.recharge { background: linear-gradient(135deg, #FF4081, #F50057); }
    .action-icon.invest { background: linear-gradient(135deg, #FFD600, #FFC107); }

    /* PIX KEYS */
    .pix-keys-section {
      padding: 16px 20px;
      max-width: 500px; margin: 0 auto; cursor: pointer;
    }
    .pix-keys-card {
      background: white; border-radius: var(--krt-radius); padding: 16px 20px;
      display: flex; justify-content: space-between; align-items: center;
      box-shadow: var(--krt-shadow-sm);
      transition: all 0.2s;
    }
    .pix-keys-card:hover { box-shadow: var(--krt-shadow); }
    .pix-keys-left { display: flex; align-items: center; gap: 12px; }
    .pix-keys-left strong { font-size: 0.9rem; color: var(--krt-text); }
    .pix-keys-left p { font-size: 0.78rem; color: var(--krt-text-muted); margin: 0; }
    .pix-icon { color: var(--krt-accent-dark); }

    /* TRANSACTIONS */
    .transactions-section {
      padding: 8px 20px 20px;
      max-width: 500px; margin: 0 auto;
    }
    .section-header { display: flex; justify-content: space-between; align-items: center; }
    .see-all {
      font-size: 0.85rem; color: var(--krt-primary); font-weight: 600;
      cursor: pointer; text-decoration: none;
    }
    .empty-state {
      text-align: center; padding: 40px 20px;
      background: white; border-radius: var(--krt-radius);
      box-shadow: var(--krt-shadow-sm);
    }
    .empty-state mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--krt-text-muted); margin-bottom: 12px; }
    .empty-state p { font-weight: 600; color: var(--krt-text); margin-bottom: 4px; }
    .empty-state span { font-size: 0.85rem; color: var(--krt-text-muted); }

    .transaction-list {
      background: white; border-radius: var(--krt-radius);
      box-shadow: var(--krt-shadow-sm); overflow: hidden;
    }
    .transaction-item {
      display: flex; align-items: center; gap: 12px;
      padding: 16px; border-bottom: 1px solid var(--krt-divider);
    }
    .transaction-item:last-child { border-bottom: none; }
    .tx-icon {
      width: 40px; height: 40px; border-radius: 12px;
      background: #FFF0F0; color: var(--krt-danger);
      display: flex; align-items: center; justify-content: center;
    }
    .tx-icon.credit { background: #E8F5E9; color: var(--krt-success); }
    .tx-info { flex: 1; }
    .tx-desc { display: block; font-size: 0.9rem; font-weight: 500; color: var(--krt-text); }
    .tx-date { font-size: 0.75rem; color: var(--krt-text-muted); }
    .tx-amount { font-weight: 700; font-size: 0.9rem; color: var(--krt-danger); }
    .tx-amount.credit { color: var(--krt-success); }
  `]
})
export class DashboardPageComponent implements OnInit {
  userName = '';
  accountId = '';
  balance = 0;
  showBalance = true;
  transactions: any[] = [];
  loading = true;

  constructor(
    public router: Router,
    private auth: AuthService,
    private accountService: AccountService
  ) {}

  ngOnInit() {
    this.accountId = localStorage.getItem('krt_account_id') || '';
    this.userName = localStorage.getItem('krt_account_name') || 'Usuário';
    this.balance = parseFloat(localStorage.getItem('krt_account_balance') || '0');
    this.showBalance = localStorage.getItem('krt_show_balance') !== 'false';
    this.transactions = JSON.parse(localStorage.getItem('krt_transactions') || '[]');
    this.loading = false;
  }

  getInitials(): string {
    return this.userName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
  }

  getGreeting(): string {
    const h = new Date().getHours();
    if (h < 12) return 'Bom dia,';
    if (h < 18) return 'Boa tarde,';
    return 'Boa noite,';
  }

  toggleEye() {
    this.showBalance = !this.showBalance;
    localStorage.setItem('krt_show_balance', String(this.showBalance));
  }

  logout() {
    this.auth.logout();
  }

  /** Busca saldo real da API */
  refreshBalanceFromApi(): void {
    const accountId = this.auth.getAccountId();
    if (accountId) {
      this.accountService.getBalance(accountId).subscribe({
        next: (res) => {
          this.balance = res.availableAmount;
          this.auth.updateBalance(res.availableAmount);
        },
        error: (err) => {
          console.warn('Falha ao buscar saldo da API, usando cache local', err);
          this.balance = this.auth.getBalance();
        }
      });
    }
  }
}



