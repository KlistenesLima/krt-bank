import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { CardService, VirtualCard, CardBill, CardBillCharge } from '../../../core/services/card.service';

@Component({
  selector: 'app-cards-page',
  template: `
    <div class="cards-container page-with-nav">
      <header class="page-header">
        <button mat-icon-button (click)="router.navigate(['/dashboard'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Cartoes</h1>
        <div style="width:40px"></div>
      </header>

      <!-- Loading -->
      <div *ngIf="loading" class="loading-state">
        <mat-icon class="spin">sync</mat-icon>
        <p>Carregando cartao...</p>
      </div>

      <!-- No card -->
      <div *ngIf="!loading && !card" class="empty-state fade-in">
        <mat-icon style="font-size:48px;width:48px;height:48px;color:var(--krt-primary)">credit_card</mat-icon>
        <p>Voce ainda nao tem um cartao virtual</p>
        <button class="btn-primary" (click)="createCard()">Criar Cartao Virtual</button>
      </div>

      <!-- Card display -->
      <ng-container *ngIf="!loading && card">
        <!-- STEP: main -->
        <ng-container *ngIf="step === 'main'">
          <div class="card-display fade-in">
            <div class="credit-card">
              <div class="card-top">
                <span class="card-brand">KRT Bank</span>
                <mat-icon>contactless</mat-icon>
              </div>
              <div class="card-number">
                <span>{{ showCardNumber ? formatCardNumber(card.cardNumber) : card.maskedNumber }}</span>
              </div>
              <div class="card-bottom">
                <div>
                  <span class="card-label">TITULAR</span>
                  <span class="card-value">{{ card.cardholderName }}</span>
                </div>
                <div>
                  <span class="card-label">VALIDADE</span>
                  <span class="card-value">{{ card.expiration }}</span>
                </div>
              </div>
            </div>

            <button class="show-number-btn" (click)="showCardNumber = !showCardNumber">
              <mat-icon>{{ showCardNumber ? 'visibility_off' : 'visibility' }}</mat-icon>
              {{ showCardNumber ? 'Esconder numero' : 'Mostrar numero' }}
            </button>
          </div>

          <div class="card-info">
            <div class="info-card">
              <div class="info-row">
                <span>Fatura atual</span>
                <strong [class.danger]="card.spentThisMonth > 0">R$ {{ card.spentThisMonth | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row">
                <span>Limite disponivel</span>
                <strong class="success">R$ {{ card.remainingLimit | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row">
                <span>Limite total</span>
                <strong>R$ {{ card.spendingLimit | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row">
                <span>Vencimento</span>
                <strong>{{ bill ? formatDate(bill.dueDate) : '-' }}</strong>
              </div>
            </div>

            <div class="action-buttons">
              <button class="action-btn primary" (click)="goToStep('bill')">
                <mat-icon>receipt_long</mat-icon>
                Ver Fatura
              </button>
              <button class="action-btn success" (click)="goToStep('pay')" [disabled]="card.spentThisMonth === 0">
                <mat-icon>payments</mat-icon>
                Pagar Fatura
              </button>
              <button class="action-btn danger-outline"
                      (click)="card.status === 'Active' ? blockCard() : unblockCard()">
                <mat-icon>{{ card.status === 'Active' ? 'lock' : 'lock_open' }}</mat-icon>
                {{ card.status === 'Active' ? 'Bloquear' : 'Desbloquear' }}
              </button>
            </div>
          </div>
        </ng-container>

        <!-- STEP: bill (Ver Fatura) -->
        <ng-container *ngIf="step === 'bill'">
          <div class="bill-container fade-in">
            <div class="bill-header">
              <h2>Fatura do Cartao</h2>
              <p class="card-ref">**** {{ card!.last4Digits }} | {{ card!.brand }}</p>
            </div>

            <div class="bill-summary info-card">
              <div class="info-row">
                <span>Total da fatura</span>
                <strong [class.danger]="bill && bill.currentBill > 0">R$ {{ bill?.currentBill | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row">
                <span>Pagamento minimo (10%)</span>
                <strong>R$ {{ bill?.minimumPayment | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row">
                <span>Fechamento</span>
                <strong>{{ formatDate(bill?.closingDate) }}</strong>
              </div>
              <div class="info-row">
                <span>Vencimento</span>
                <strong>{{ formatDate(bill?.dueDate) }}</strong>
              </div>
              <div class="info-row">
                <span>Limite disponivel</span>
                <strong class="success">R$ {{ bill?.availableLimit | number:'1.2-2' }}</strong>
              </div>
            </div>

            <!-- Barra de progresso: total pago / total da fatura -->
            <div class="bill-progress" *ngIf="bill && bill.currentBill > 0">
              <div class="progress-label">
                <span>Pago</span>
                <span>R$ {{ totalPaid | number:'1.2-2' }} de R$ {{ totalCharges | number:'1.2-2' }}</span>
              </div>
              <div class="progress-bar">
                <div class="progress-fill" [style.width.%]="progressPercent"></div>
              </div>
            </div>

            <h3 *ngIf="bill && bill.payments && bill.payments.length > 0" style="padding:0 20px;margin:20px 0 8px">Pagamentos do mes</h3>
            <div class="charges-list" *ngIf="bill && bill.payments && bill.payments.length > 0">
              <div class="charge-item" *ngFor="let p of bill.payments">
                <div class="charge-info">
                  <span class="charge-desc">{{ p.description || 'Pagamento de fatura' }}</span>
                  <span class="charge-date">{{ formatDate(p.date) }}</span>
                </div>
                <span class="charge-amount payment-amount">- R$ {{ p.amount | number:'1.2-2' }}</span>
              </div>
            </div>

            <h3 *ngIf="bill && bill.charges.length > 0" style="padding:0 20px;margin:20px 0 8px">Compras do mes</h3>
            <div class="charges-list" *ngIf="bill">
              <div class="charge-item" *ngFor="let c of bill.charges">
                <div class="charge-info">
                  <span class="charge-desc">{{ c.description || 'Compra' }}</span>
                  <span class="charge-date">{{ formatDate(c.createdAt) }}
                    <span *ngIf="c.installments > 1"> | {{ c.installments }}x R$ {{ c.installmentAmount | number:'1.2-2' }}</span>
                  </span>
                </div>
                <span class="charge-amount">R$ {{ c.amount | number:'1.2-2' }}</span>
              </div>
            </div>

            <div class="bill-actions">
              <button class="btn-primary" (click)="goToStep('pay')" [disabled]="!bill || bill.currentBill === 0">Pagar Fatura</button>
              <button class="btn-secondary" (click)="goToStep('main')">Voltar</button>
            </div>
          </div>
        </ng-container>

        <!-- STEP: pay (Pagar Fatura) -->
        <ng-container *ngIf="step === 'pay'">
          <div class="pay-container fade-in">
            <div class="bill-header">
              <h2>Pagar Fatura</h2>
              <p class="card-ref">**** {{ card!.last4Digits }} | Fatura: R$ {{ card!.spentThisMonth | number:'1.2-2' }}</p>
            </div>

            <div class="pay-options info-card">
              <button class="pay-option" [class.selected]="payOption === 'total'" (click)="selectPayOption('total')">
                <div>
                  <strong>Pagar total</strong>
                  <span>R$ {{ card!.spentThisMonth | number:'1.2-2' }}</span>
                </div>
                <mat-icon>{{ payOption === 'total' ? 'radio_button_checked' : 'radio_button_unchecked' }}</mat-icon>
              </button>
              <button class="pay-option" [class.selected]="payOption === 'minimum'" (click)="selectPayOption('minimum')">
                <div>
                  <strong>Pagamento minimo (10%)</strong>
                  <span>R$ {{ minimumPayment | number:'1.2-2' }}</span>
                </div>
                <mat-icon>{{ payOption === 'minimum' ? 'radio_button_checked' : 'radio_button_unchecked' }}</mat-icon>
              </button>
              <button class="pay-option" [class.selected]="payOption === 'custom'" (click)="selectPayOption('custom')">
                <div>
                  <strong>Outro valor</strong>
                  <span>Escolha quanto pagar</span>
                </div>
                <mat-icon>{{ payOption === 'custom' ? 'radio_button_checked' : 'radio_button_unchecked' }}</mat-icon>
              </button>
            </div>

            <div class="custom-amount" *ngIf="payOption === 'custom'">
              <label>Valor a pagar</label>
              <input type="number" [(ngModel)]="customAmount" [max]="card!.spentThisMonth" min="0.01" step="0.01"
                     placeholder="0,00">
            </div>

            <div class="pay-summary info-card" *ngIf="payAmount > 0">
              <div class="info-row">
                <span>Valor a pagar</span>
                <strong>R$ {{ payAmount | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row">
                <span>Seu saldo</span>
                <strong>R$ {{ accountBalance | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row" *ngIf="payAmount > accountBalance">
                <span class="danger">Saldo insuficiente!</span>
                <strong class="danger">-</strong>
              </div>
            </div>

            <div class="bill-actions">
              <button class="btn-primary" (click)="confirmPayBill()"
                      [disabled]="paying || payAmount <= 0 || payAmount > accountBalance || payAmount > card!.spentThisMonth">
                {{ paying ? 'Processando...' : 'Confirmar Pagamento' }}
              </button>
              <button class="btn-secondary" (click)="goToStep('main')">Voltar</button>
            </div>
          </div>
        </ng-container>

        <!-- STEP: success -->
        <ng-container *ngIf="step === 'success'">
          <div class="success-container fade-in">
            <div class="success-icon">
              <mat-icon>check_circle</mat-icon>
            </div>
            <h2>Pagamento realizado!</h2>
            <div class="info-card">
              <div class="info-row">
                <span>Valor pago</span>
                <strong>R$ {{ lastPayResult?.amountPaid | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row">
                <span>Fatura restante</span>
                <strong>R$ {{ lastPayResult?.remainingBill | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row">
                <span>Limite disponivel</span>
                <strong class="success">R$ {{ lastPayResult?.availableLimit | number:'1.2-2' }}</strong>
              </div>
              <div class="info-row">
                <span>Saldo da conta</span>
                <strong>R$ {{ lastPayResult?.accountBalance | number:'1.2-2' }}</strong>
              </div>
            </div>
            <div class="bill-actions">
              <button class="btn-primary" (click)="goToStep('main'); loadCard()">Voltar ao Cartao</button>
            </div>
          </div>
        </ng-container>
      </ng-container>
    </div>

    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .cards-container { min-height: 100vh; background: var(--krt-bg); padding-bottom: 80px; }
    .page-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 20px; background: white; border-bottom: 1px solid var(--krt-divider);
    }
    .page-header h1 { font-size: 1.1rem; font-weight: 700; margin: 0; }

    .loading-state, .empty-state {
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      padding: 60px 20px; gap: 16px; text-align: center;
    }
    .loading-state p, .empty-state p { color: var(--krt-text-secondary); }
    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

    .card-display { padding: 24px 20px; max-width: 500px; margin: 0 auto; }
    .credit-card {
      background: var(--krt-gradient-card); border-radius: 20px;
      padding: 28px 24px; color: white; position: relative;
      box-shadow: 0 12px 40px rgba(0,51,153,0.3);
      aspect-ratio: 1.6;
      display: flex; flex-direction: column; justify-content: space-between;
    }
    .card-top { display: flex; justify-content: space-between; align-items: center; }
    .card-brand { font-weight: 800; font-size: 1.1rem; letter-spacing: 1px; }
    .card-number { margin: auto 0; }
    .card-number span { font-size: 1.3rem; letter-spacing: 3px; font-weight: 500; }
    .card-bottom { display: flex; justify-content: space-between; }
    .card-label { display: block; font-size: 0.6rem; color: rgba(255,255,255,0.6); letter-spacing: 1px; }
    .card-value { font-size: 0.85rem; font-weight: 600; }

    .show-number-btn {
      display: flex; align-items: center; gap: 8px; justify-content: center;
      width: 100%; margin-top: 16px; padding: 12px;
      background: none; border: none; cursor: pointer;
      color: var(--krt-primary); font-weight: 600; font-size: 0.88rem;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .card-info, .bill-container, .pay-container, .success-container {
      padding: 0 20px; max-width: 500px; margin: 0 auto;
    }
    .info-card {
      background: white; border-radius: var(--krt-radius); padding: 4px 20px;
      box-shadow: var(--krt-shadow-sm);
    }
    .info-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 16px 0; border-bottom: 1px solid var(--krt-divider);
    }
    .info-row:last-child { border-bottom: none; }
    .info-row span { font-size: 0.88rem; color: var(--krt-text-secondary); }
    .info-row strong { font-size: 0.95rem; }
    .success { color: var(--krt-success); }
    .danger { color: var(--krt-danger); }

    .action-buttons { display: flex; flex-direction: column; gap: 10px; margin-top: 16px; }
    .action-btn {
      display: flex; align-items: center; gap: 8px; justify-content: center;
      width: 100%; padding: 14px;
      border-radius: var(--krt-radius-sm); cursor: pointer;
      font-weight: 600; font-size: 0.9rem;
      font-family: 'Plus Jakarta Sans', sans-serif;
      transition: all 0.2s; border: 1.5px solid transparent;
    }
    .action-btn:disabled { opacity: 0.4; cursor: not-allowed; }
    .action-btn.primary {
      background: var(--krt-primary); color: white; border-color: var(--krt-primary);
    }
    .action-btn.primary:hover:not(:disabled) { opacity: 0.9; }
    .action-btn.success {
      background: var(--krt-success); color: white; border-color: var(--krt-success);
    }
    .action-btn.success:hover:not(:disabled) { opacity: 0.9; }
    .action-btn.danger-outline {
      background: white; color: var(--krt-danger); border-color: var(--krt-danger);
    }
    .action-btn.danger-outline:hover { background: #FFF5F5; }

    .bill-header { padding: 24px 0 16px; }
    .bill-header h2 { font-size: 1.2rem; font-weight: 700; margin: 0 0 4px; }
    .card-ref { font-size: 0.85rem; color: var(--krt-text-secondary); margin: 0; }

    .charges-list { padding: 0 20px; }
    .charge-item {
      display: flex; justify-content: space-between; align-items: center;
      padding: 14px 0; border-bottom: 1px solid var(--krt-divider);
    }
    .charge-item:last-child { border-bottom: none; }
    .charge-info { display: flex; flex-direction: column; gap: 2px; }
    .charge-desc { font-size: 0.9rem; font-weight: 600; }
    .charge-date { font-size: 0.78rem; color: var(--krt-text-secondary); }
    .charge-amount { font-weight: 700; font-size: 0.95rem; color: var(--krt-danger); }
    .payment-amount { color: var(--krt-success); }

    .bill-actions { padding: 24px 0; display: flex; flex-direction: column; gap: 10px; }
    .btn-primary {
      width: 100%; padding: 16px; border: none; border-radius: var(--krt-radius-sm);
      background: var(--krt-primary); color: white; font-weight: 700; font-size: 0.95rem;
      cursor: pointer; font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .btn-primary:disabled { opacity: 0.4; cursor: not-allowed; }
    .btn-secondary {
      width: 100%; padding: 14px; border: 1.5px solid var(--krt-divider);
      border-radius: var(--krt-radius-sm); background: white; color: var(--krt-text-secondary);
      font-weight: 600; font-size: 0.9rem; cursor: pointer;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .pay-options { padding: 0; overflow: hidden; }
    .pay-option {
      display: flex; justify-content: space-between; align-items: center;
      width: 100%; padding: 16px 20px; border: none; background: white; cursor: pointer;
      border-bottom: 1px solid var(--krt-divider); text-align: left;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .pay-option:last-child { border-bottom: none; }
    .pay-option div { display: flex; flex-direction: column; gap: 2px; }
    .pay-option strong { font-size: 0.9rem; }
    .pay-option span { font-size: 0.8rem; color: var(--krt-text-secondary); }
    .pay-option.selected { background: #F0F7FF; }
    .pay-option mat-icon { color: var(--krt-primary); }

    .custom-amount { padding: 16px 0; }
    .custom-amount label { display: block; font-size: 0.85rem; font-weight: 600; margin-bottom: 8px; }
    .custom-amount input {
      width: 100%; padding: 14px 16px; border: 1.5px solid var(--krt-divider);
      border-radius: var(--krt-radius-sm); font-size: 1.1rem; font-weight: 600;
      font-family: 'Plus Jakarta Sans', sans-serif; box-sizing: border-box;
    }
    .custom-amount input:focus { border-color: var(--krt-primary); outline: none; }

    .bill-progress { padding: 16px 0; }
    .progress-label {
      display: flex; justify-content: space-between; align-items: center;
      margin-bottom: 8px; font-size: 0.82rem; color: var(--krt-text-secondary);
    }
    .progress-bar {
      width: 100%; height: 8px; background: var(--krt-divider);
      border-radius: 4px; overflow: hidden;
    }
    .progress-fill {
      height: 100%; background: var(--krt-success);
      border-radius: 4px; transition: width 0.5s ease;
    }

    .pay-summary { margin-top: 16px; }

    .success-container { text-align: center; padding-top: 40px; }
    .success-icon mat-icon { font-size: 64px; width: 64px; height: 64px; color: var(--krt-success); }
    .success-container h2 { margin: 16px 0 24px; }
    .success-container .info-card { text-align: left; }

    .fade-in { animation: fadeIn 0.3s ease; }
    @keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: none; } }
  `]
})
export class CardsPageComponent implements OnInit, OnDestroy {
  card: VirtualCard | null = null;
  bill: CardBill | null = null;
  loading = true;
  showCardNumber = false;
  step: 'main' | 'bill' | 'pay' | 'success' = 'main';

  // Pay bill state
  payOption: 'total' | 'minimum' | 'custom' = 'total';
  customAmount = 0;
  paying = false;
  lastPayResult: any = null;

  accountId = '';
  accountBalance = 0;

  private pollSub: Subscription | null = null;
  private visibilityHandler = () => this.onVisibilityChange();

  constructor(public router: Router, private cardService: CardService) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('krt_account_id') || '';
    this.accountBalance = parseFloat(localStorage.getItem('krt_account_balance') || '0');
    if (this.accountId) {
      this.loadCard();
      this.startPolling();
    } else {
      this.loading = false;
    }
    document.addEventListener('visibilitychange', this.visibilityHandler);
  }

  ngOnDestroy(): void {
    this.stopPolling();
    document.removeEventListener('visibilitychange', this.visibilityHandler);
  }

  private startPolling(): void {
    this.stopPolling();
    this.pollSub = interval(30000).subscribe(() => {
      if (document.hidden) return;
      this.refreshData();
    });
  }

  private stopPolling(): void {
    if (this.pollSub) {
      this.pollSub.unsubscribe();
      this.pollSub = null;
    }
  }

  private onVisibilityChange(): void {
    if (!document.hidden && this.accountId) {
      this.refreshData();
    }
  }

  private refreshData(): void {
    if (!this.card) return;
    // Refresh card data silently (no loading spinner)
    this.cardService.getCard(this.card.id).subscribe({
      next: (c) => { this.card = c; },
      error: () => {}
    });
    // Refresh bill if currently viewing it
    if (this.step === 'bill' && this.card) {
      this.cardService.getBill(this.card.id).subscribe({
        next: (b) => { this.bill = b; },
        error: () => {}
      });
    }
  }

  loadCard(): void {
    this.loading = true;
    this.cardService.getCards(this.accountId).subscribe({
      next: (cards) => {
        if (cards.length > 0) {
          this.cardService.getCard(cards[0].id).subscribe({
            next: (c) => {
              this.card = c;
              this.loading = false;
              // Carregar fatura para exibir vencimento na tela principal
              this.cardService.getBill(c.id).subscribe({
                next: (b) => this.bill = b,
                error: () => {}
              });
            },
            error: () => { this.loading = false; }
          });
        } else {
          this.card = null;
          this.loading = false;
        }
      },
      error: () => { this.loading = false; }
    });
  }

  createCard(): void {
    const name = localStorage.getItem('krt_account_name') || 'Usuario';
    this.cardService.createCard(this.accountId, name, 'Visa').subscribe({
      next: () => this.loadCard(),
      error: () => alert('Erro ao criar cartao')
    });
  }

  goToStep(s: 'main' | 'bill' | 'pay' | 'success'): void {
    this.step = s;
    if (s === 'bill' && this.card) {
      this.cardService.getBill(this.card.id).subscribe({
        next: (b) => this.bill = b,
        error: () => {}
      });
    }
    if (s === 'pay') {
      this.payOption = 'total';
      this.customAmount = 0;
    }
  }

  get totalCharges(): number {
    if (!this.bill) return 0;
    return this.bill.charges.reduce((sum, c) => sum + c.amount, 0);
  }

  get totalPaid(): number {
    if (!this.bill) return 0;
    return this.bill.payments.reduce((sum, p) => sum + p.amount, 0);
  }

  get progressPercent(): number {
    const total = this.totalCharges;
    if (total === 0) return 0;
    return Math.min(100, (this.totalPaid / total) * 100);
  }

  get minimumPayment(): number {
    return this.card ? Math.round(this.card.spentThisMonth * 0.10 * 100) / 100 : 0;
  }

  get payAmount(): number {
    if (!this.card) return 0;
    if (this.payOption === 'total') return this.card.spentThisMonth;
    if (this.payOption === 'minimum') return this.minimumPayment;
    return this.customAmount || 0;
  }

  selectPayOption(opt: 'total' | 'minimum' | 'custom'): void {
    this.payOption = opt;
  }

  confirmPayBill(): void {
    if (!this.card || this.payAmount <= 0) return;
    this.paying = true;
    this.cardService.payBill(this.card.id, this.payAmount).subscribe({
      next: (res) => {
        this.paying = false;
        this.lastPayResult = res;
        // Update localStorage balance
        localStorage.setItem('krt_account_balance', res.accountBalance.toString());
        this.accountBalance = res.accountBalance;
        this.step = 'success';
      },
      error: (err) => {
        this.paying = false;
        alert(err.error?.error || 'Erro ao pagar fatura');
      }
    });
  }

  blockCard(): void {
    if (!this.card) return;
    this.cardService.blockCard(this.card.id).subscribe(() => this.loadCard());
  }

  unblockCard(): void {
    if (!this.card) return;
    this.cardService.unblockCard(this.card.id).subscribe(() => this.loadCard());
  }

  formatCardNumber(n: string): string {
    const digits = n.replace(/\D/g, '');
    if (digits.length < 16) return n;
    return `${digits.slice(0,4)} ${digits.slice(4,8)} ${digits.slice(8,12)} ${digits.slice(12,16)}`;
  }

  formatDate(d: string | undefined): string {
    if (!d) return '-';
    const date = new Date(d);
    return date.toLocaleDateString('pt-BR');
  }
}
