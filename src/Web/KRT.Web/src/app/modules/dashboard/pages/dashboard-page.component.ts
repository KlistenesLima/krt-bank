import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { AccountService } from '../../../core/services/account.service';
import { PaymentService } from '../../../core/services/payment.service';

@Component({
  selector: 'app-dashboard-page',
  template: `
    <div class="dashboard-shell">
      <!-- HEADER INTEGRADO COM SALDO -->
      <header class="hero-header">
        <div class="hero-inner">
          <div class="hero-top">
            <div class="user-info" (click)="toggleDropdown()">
              <div class="avatar-ring">
                <div class="avatar">{{ getInitials() }}</div>
              </div>
              <div class="greeting">
                <span class="greeting-text">{{ getGreeting() }}</span>
                <h2 class="user-name">{{ userName }}</h2>
              </div>
            </div>
            <div class="header-actions">
              <button class="glass-btn" (click)="router.navigate(['/inbox'])">
                <mat-icon>notifications_none</mat-icon>
              </button>
              <div class="avatar-menu-wrapper">
                <button class="glass-btn" (click)="toggleDropdown()">
                  <mat-icon>account_circle</mat-icon>
                </button>
                <!-- Dropdown -->
                <div class="dropdown" *ngIf="showDropdown">
                  <div class="dropdown-card" (click)="$event.stopPropagation()">
                    <div class="dd-header">
                      <div class="dd-avatar">{{ getInitials() }}</div>
                      <div>
                        <strong>{{ userName }}</strong>
                        <span>{{ userEmail }}</span>
                      </div>
                    </div>
                    <div class="dd-sep"></div>
                    <button class="dd-item" (click)="router.navigate(['/dashboard']); showDropdown=false">
                      <mat-icon>dashboard</mat-icon> Dashboard
                    </button>
                    <button class="dd-item" (click)="router.navigate(['/extract']); showDropdown=false">
                      <mat-icon>receipt_long</mat-icon> Extrato
                    </button>
                    <button class="dd-item admin" *ngIf="isUserAdmin" (click)="router.navigate(['/admin']); showDropdown=false">
                      <mat-icon>admin_panel_settings</mat-icon> Command Center
                    </button>
                    <button class="dd-item admin" *ngIf="isUserAdmin" (click)="router.navigate(['/admin/users']); showDropdown=false">
                      <mat-icon>group</mat-icon> Gerenciar Usuarios
                    </button>
                    <div class="dd-sep"></div>
                    <button class="dd-item logout" (click)="logout()">
                      <mat-icon>logout</mat-icon> Sair
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- SALDO DENTRO DO HEADER -->
          <div class="balance-area">
            <div class="balance-row">
              <span class="balance-label">Saldo disponivel</span>
              <button class="eye-btn" (click)="toggleEye()">
                <mat-icon>{{ showBalance ? 'visibility' : 'visibility_off' }}</mat-icon>
              </button>
            </div>
            <div class="balance-value" [class.hidden]="!showBalance">
              {{ showBalance ? (balance | currency:'BRL') : '•••••••' }}
            </div>
            <span class="account-tag" *ngIf="showBalance">Ag 0001 · Conta {{ accountId?.substring(0, 8) }}</span>
          </div>
        </div>
      </header>

      <!-- SKELETON -->
      <div class="content-area" *ngIf="loading">
        <div class="skel-grid">
          <div class="skel skel-action" *ngFor="let s of [1,2,3,4,5,6]"></div>
        </div>
        <div class="skel skel-bar"></div>
        <div class="skel skel-tx" *ngFor="let s of [1,2,3]"></div>
      </div>

      <!-- CONTEUDO PRINCIPAL -->
      <div class="content-area" *ngIf="!loading">
        <!-- Quick Actions -->
        <section class="section slide-up">
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
              <span>Cartoes</span>
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

        <!-- PIX Keys -->
        <section class="section slide-up d1" (click)="router.navigate(['/pix/keys'])">
          <div class="pix-keys-card">
            <div class="pix-left">
              <div class="pix-icon-wrap"><mat-icon>vpn_key</mat-icon></div>
              <div>
                <strong>Minhas chaves Pix</strong>
                <span>Gerencie suas chaves</span>
              </div>
            </div>
            <mat-icon class="chevron">chevron_right</mat-icon>
          </div>
        </section>

        <!-- Transactions -->
        <section class="section slide-up d2">
          <div class="section-row">
            <h3 class="section-title">Movimentacoes recentes</h3>
            <a class="link" (click)="router.navigate(['/extract'])">Ver tudo</a>
          </div>

          <div *ngIf="transactions.length === 0" class="empty-card">
            <div class="empty-icon"><mat-icon>account_balance_wallet</mat-icon></div>
            <strong>Nenhuma movimentacao ainda</strong>
            <span>Faca seu primeiro Pix!</span>
          </div>

          <div class="tx-list" *ngIf="transactions.length > 0">
            <div class="tx-item" *ngFor="let t of transactions">
              <div class="tx-dot" [class.credit]="t.type === 'CREDIT'">
                <mat-icon>{{ t.type === 'CREDIT' ? 'south_west' : 'north_east' }}</mat-icon>
              </div>
              <div class="tx-info">
                <span class="tx-desc">{{ t.description }}</span>
                <span class="tx-date">{{ t.createdAt | date:'dd/MM · HH:mm' }}</span>
              </div>
              <span class="tx-val" [class.credit]="t.type === 'CREDIT'">
                {{ t.type === 'CREDIT' ? '+' : '-' }}{{ t.amount | currency:'BRL':'symbol':'1.2-2' }}
              </span>
            </div>
          </div>
        </section>
      </div>

      <app-bottom-nav></app-bottom-nav>
    </div>
  `,
  styles: [`
    /* === SHELL === */
    .dashboard-shell {
      min-height: 100vh;
      background: #f0f2f5;
      padding-bottom: 90px;
    }

    /* === HERO HEADER === */
    .hero-header {
      background: linear-gradient(135deg, #0047BB 0%, #0035a0 40%, #002a70 100%);
      padding: 0 0 32px;
      position: relative;
    }
    .hero-header::after {
      content: ''; position: absolute; bottom: -1px; left: 0; right: 0; height: 24px;
      background: #f0f2f5; border-radius: 24px 24px 0 0;
    }
    .hero-inner {
      max-width: 500px; margin: 0 auto; padding: 0 20px;
      position: relative; z-index: 2;
    }

    /* Top row */
    .hero-top {
      display: flex; justify-content: space-between; align-items: center;
      padding: 20px 0 24px;
    }
    .user-info { display: flex; align-items: center; gap: 12px; cursor: pointer; }
    .avatar-ring {
      width: 48px; height: 48px; border-radius: 16px;
      background: linear-gradient(135deg, rgba(255,255,255,0.2), rgba(255,255,255,0.05));
      padding: 2px; display: flex; align-items: center; justify-content: center;
    }
    .avatar {
      width: 100%; height: 100%; border-radius: 14px;
      background: rgba(255,255,255,0.15); color: white;
      display: flex; align-items: center; justify-content: center;
      font-weight: 800; font-size: 0.9rem; backdrop-filter: blur(10px);
    }
    .greeting-text { color: rgba(255,255,255,0.75); font-size: 0.78rem; font-weight: 500; }
    .user-name { color: white; font-size: 1.1rem; font-weight: 700; margin: 2px 0 0; }

    .header-actions { display: flex; gap: 8px; }
    .glass-btn {
      width: 42px; height: 42px; border-radius: 14px; border: none;
      background: rgba(255,255,255,0.08); color: rgba(255,255,255,0.8);
      display: flex; align-items: center; justify-content: center;
      cursor: pointer; transition: all 0.2s;
      backdrop-filter: blur(8px);
    }
    .glass-btn:hover { background: rgba(255,255,255,0.16); color: white; }
    .glass-btn mat-icon { font-size: 22px; width: 22px; height: 22px; }

    /* Balance area */
    .balance-area { padding: 8px 0 0; }
    .balance-row { display: flex; align-items: center; gap: 8px; }
    .balance-label { font-size: 0.82rem; color: rgba(255,255,255,0.7); font-weight: 500; }
    .eye-btn {
      background: none; border: none; padding: 2px; cursor: pointer;
      color: rgba(255,255,255,0.4); display: flex;
    }
    .eye-btn mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .eye-btn:hover { color: rgba(255,255,255,0.7); }
    .balance-value {
      font-size: 2rem; font-weight: 800; color: white;
      margin: 6px 0 4px; letter-spacing: -0.5px;
      text-shadow: 0 2px 12px rgba(0,0,0,0.15);
    }
    .balance-value.hidden { color: rgba(255,255,255,0.3); letter-spacing: 4px; font-size: 1.5rem; }
    .account-tag {
      font-size: 0.75rem; color: rgba(255,255,255,0.6);
      background: rgba(255,255,255,0.12); padding: 4px 12px;
      border-radius: 20px; display: inline-block;
    }

    /* === DROPDOWN === */
    .avatar-menu-wrapper { position: relative; }
    .dropdown {
      position: fixed; inset: 0; z-index: 9999;
      background: rgba(0,0,0,0.3); backdrop-filter: blur(4px);
    }
    .dropdown-card {
      position: absolute; top: 70px; right: 20px; width: 270px;
      background: #ffffff; border-radius: 20px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.25);
      overflow: hidden; animation: dropIn 0.2s ease;
    }
    @keyframes dropIn { from { opacity: 0; transform: translateY(-8px) scale(0.95); } to { opacity: 1; transform: translateY(0) scale(1); } }

    .dd-header {
      display: flex; align-items: center; gap: 12px;
      padding: 18px 18px 14px; background: #f8fafc;
    }
    .dd-avatar {
      width: 44px; height: 44px; border-radius: 14px;
      background: linear-gradient(135deg, #0047BB, #002a70); color: white;
      display: flex; align-items: center; justify-content: center;
      font-weight: 800; font-size: 0.85rem;
    }
    .dd-header strong { display: block; font-size: 0.9rem; color: #1a1a2e; }
    .dd-header span { font-size: 0.75rem; color: #9ca3af; }
    .dd-sep { height: 1px; background: #f0f0f0; }
    .dd-item {
      display: flex; align-items: center; gap: 10px;
      width: 100%; padding: 13px 18px; border: none; background: none;
      font-size: 0.88rem; color: #374151; cursor: pointer;
      transition: background 0.15s; text-align: left;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .dd-item:hover { background: #f8fafc; }
    .dd-item mat-icon { font-size: 20px; width: 20px; height: 20px; color: #9ca3af; }
    .dd-item.admin { color: #3b82f6; font-weight: 600; }
    .dd-item.admin mat-icon { color: #3b82f6; }
    .dd-item.logout { color: #ef4444; }
    .dd-item.logout mat-icon { color: #ef4444; }

    /* === CONTENT === */
    .content-area {
      max-width: 500px; margin: 0 auto;
      padding: 0 20px;
    }
    .section { margin-bottom: 16px; }
    .section-title {
      font-size: 0.95rem; font-weight: 800; color: #1a1a2e;
      margin: 0 0 14px; letter-spacing: -0.2px;
    }
    .section-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 14px; }
    .section-row .section-title { margin: 0; }
    .link { font-size: 0.82rem; color: #0047BB; font-weight: 700; cursor: pointer; text-decoration: none; }

    /* Actions */
    .actions-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
    .action-card {
      background: #ffffff; border: none; border-radius: 18px;
      padding: 18px 8px; display: flex; flex-direction: column;
      align-items: center; gap: 10px; cursor: pointer;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.03);
      transition: all 0.25s;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .action-card:hover { transform: translateY(-3px); box-shadow: 0 8px 24px rgba(0,0,0,0.08); }
    .action-card:active { transform: translateY(-1px); }
    .action-card span { font-size: 0.78rem; font-weight: 700; color: #374151; }
    .action-icon {
      width: 50px; height: 50px; border-radius: 16px;
      display: flex; align-items: center; justify-content: center;
    }
    .action-icon mat-icon { font-size: 24px; width: 24px; height: 24px; color: white; }
    .action-icon.pix { background: linear-gradient(135deg, #00D4AA, #00B894); }
    .action-icon.boleto { background: linear-gradient(135deg, #FF6B35, #E55D2B); }
    .action-icon.extract { background: linear-gradient(135deg, #7C4DFF, #651FFF); }
    .action-icon.cards { background: linear-gradient(135deg, #0047BB, #003399); }
    .action-icon.recharge { background: linear-gradient(135deg, #FF4081, #F50057); }
    .action-icon.invest { background: linear-gradient(135deg, #FFD600, #FFC107); }

    /* PIX Keys */
    .pix-keys-card {
      background: #ffffff; border-radius: 18px; padding: 18px 20px;
      display: flex; justify-content: space-between; align-items: center;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.03);
      cursor: pointer; transition: all 0.2s;
    }
    .pix-keys-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.08); }
    .pix-left { display: flex; align-items: center; gap: 14px; }
    .pix-icon-wrap {
      width: 42px; height: 42px; border-radius: 14px;
      background: linear-gradient(135deg, #00D4AA, #00B894);
      display: flex; align-items: center; justify-content: center;
    }
    .pix-icon-wrap mat-icon { color: white; font-size: 20px; width: 20px; height: 20px; }
    .pix-left strong { display: block; font-size: 0.9rem; color: #1a1a2e; }
    .pix-left span { font-size: 0.78rem; color: #9ca3af; }
    .chevron { color: #d1d5db; }

    /* Transactions */
    .empty-card {
      text-align: center; padding: 44px 20px;
      background: #ffffff; border-radius: 18px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.03);
    }
    .empty-icon {
      width: 56px; height: 56px; border-radius: 18px;
      background: #f3f4f6; display: flex; align-items: center; justify-content: center;
      margin: 0 auto 14px;
    }
    .empty-icon mat-icon { font-size: 28px; width: 28px; height: 28px; color: #9ca3af; }
    .empty-card strong { display: block; color: #1a1a2e; font-size: 0.95rem; margin-bottom: 4px; }
    .empty-card span { color: #9ca3af; font-size: 0.85rem; }

    .tx-list {
      background: #ffffff; border-radius: 18px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.03);
      overflow: hidden;
    }
    .tx-item {
      display: flex; align-items: center; gap: 14px;
      padding: 16px 18px; border-bottom: 1px solid #f5f5f5;
      transition: background 0.15s;
    }
    .tx-item:last-child { border-bottom: none; }
    .tx-item:hover { background: #fafbfc; }
    .tx-dot {
      width: 42px; height: 42px; border-radius: 14px;
      background: #fef2f2; color: #ef4444;
      display: flex; align-items: center; justify-content: center;
      flex-shrink: 0;
    }
    .tx-dot.credit { background: #f0fdf4; color: #16a34a; }
    .tx-dot mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .tx-info { flex: 1; }
    .tx-desc { display: block; font-size: 0.9rem; font-weight: 600; color: #1a1a2e; }
    .tx-date { font-size: 0.75rem; color: #9ca3af; }
    .tx-val { font-weight: 800; font-size: 0.92rem; color: #ef4444; white-space: nowrap; }
    .tx-val.credit { color: #16a34a; }

    /* Skeleton */
    .skel {
      background: linear-gradient(90deg, #e5e7eb 25%, #f3f4f6 50%, #e5e7eb 75%);
      background-size: 200% 100%; animation: shimmer 1.5s infinite; border-radius: 14px;
    }
    @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }
    .skel-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; margin-bottom: 16px; }
    .skel-action { height: 90px; }
    .skel-bar { height: 64px; margin-bottom: 16px; }
    .skel-tx { height: 68px; margin-bottom: 8px; }

    /* Animations */
    .slide-up { animation: slideUp 0.5s ease forwards; opacity: 0; }
    .d1 { animation-delay: 0.08s; }
    .d2 { animation-delay: 0.16s; }
    @keyframes slideUp {
      from { opacity: 0; transform: translateY(16px); }
      to { opacity: 1; transform: translateY(0); }
    }

    /* Responsive */
    @media (max-width: 480px) {
      .hero-inner { padding: 0 16px; }
      .content-area { padding: 0 16px; }
      .dropdown-card { width: calc(100vw - 40px); right: 20px; max-width: 300px; }
      .balance-value { font-size: 1.75rem; }
      .actions-grid { gap: 8px; }
      .action-card { padding: 14px 6px; border-radius: 14px; }
      .action-icon { width: 44px; height: 44px; border-radius: 14px; }
      .action-card span { font-size: 0.72rem; }
      .tx-item { padding: 14px 14px; gap: 12px; }
      .tx-desc { font-size: 0.85rem; }
      .tx-val { font-size: 0.85rem; }
    }
    @media (max-width: 360px) {
      .actions-grid { grid-template-columns: repeat(3, 1fr); gap: 6px; }
      .action-card { padding: 12px 4px; }
      .action-icon { width: 40px; height: 40px; }
      .action-card span { font-size: 0.68rem; }
      .balance-value { font-size: 1.5rem; }
      .user-name { font-size: 1rem; }
    }
  `]
})
export class DashboardPageComponent implements OnInit {
  userName = '';
  accountId = '';
  balance = 0;
  showBalance = true;
  transactions: any[] = [];
  loading = true;
  isUserAdmin = false;
  showDropdown = false;
  userEmail = '';

  constructor(
    public router: Router,
    private auth: AuthService,
    private accountService: AccountService,
    private paymentService: PaymentService
  ) {}

  ngOnInit() {
    this.accountId = localStorage.getItem('krt_account_id') || '';
    this.userName = localStorage.getItem('krt_account_name') || 'Usuario';
    this.balance = parseFloat(localStorage.getItem('krt_account_balance') || '0');
    this.showBalance = localStorage.getItem('krt_show_balance') !== 'false';
    this.isUserAdmin = this.auth.isAdmin();
    this.userEmail = localStorage.getItem('krt_account_email') || '';
    this.loading = false;
    if (this.accountId) {
      this.paymentService.getStatement(this.accountId, 1, 5).subscribe({
        next: (res) => {
          this.transactions = res.items.map((entry: any) => ({
            type: entry.isCredit ? 'CREDIT' : 'DEBIT',
            description: entry.description || entry.type,
            amount: entry.amount,
            createdAt: entry.date || entry.createdAt,
            status: 'Completed'
          }));
        },
        error: () => { this.transactions = []; }
      });
    }
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

  toggleDropdown() { this.showDropdown = !this.showDropdown; }

  toggleEye() {
    this.showBalance = !this.showBalance;
    localStorage.setItem('krt_show_balance', String(this.showBalance));
  }

  logout() { this.auth.logout(); }

  refreshBalanceFromApi(): void {
    const accountId = this.auth.getAccountId();
    if (accountId) {
      this.accountService.getBalance(accountId).subscribe({
        next: (res) => { this.balance = res.availableAmount; this.auth.updateBalance(res.availableAmount); },
        error: () => { this.balance = this.auth.getBalance(); }
      });
    }
  }
}

