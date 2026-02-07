import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-boleto',
  template: `
    <div class="boleto-container page-with-nav">
      <header class="page-header">
        <button class="back-btn" (click)="router.navigate(['/dashboard'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Pagar Boleto</h1>
        <div style="width:40px"></div>
      </header>

      <div class="boleto-content fade-in" *ngIf="!paid">
        <div class="barcode-area">
          <mat-icon>qr_code_scanner</mat-icon>
          <p>Escaneie ou digite o código de barras</p>
        </div>

        <div class="field">
          <label>Código do boleto</label>
          <div class="input-wrap">
            <input type="text" [(ngModel)]="code" placeholder="00000.00000 00000.000000 00000.000000 0 00000000000000"
                   (input)="maskBoleto($event)" maxlength="54" autocomplete="off">
            <mat-icon>qr_code</mat-icon>
          </div>
          <span class="hint" *ngIf="code && code.length < 47">Digite os 47 dígitos do boleto</span>
        </div>

        <div class="field" *ngIf="isCodeValid()">
          <label>Valor do boleto</label>
          <div class="input-wrap">
            <input type="text" [(ngModel)]="amountDisplay" (input)="maskMoney($event)"
                   placeholder="0,00" inputmode="numeric" maxlength="15" autocomplete="off">
            <span class="currency-prefix">R$</span>
          </div>
        </div>

        <div class="info-card" *ngIf="isCodeValid()">
          <div class="info-row">
            <span>Beneficiário</span>
            <strong>Empresa Exemplo LTDA</strong>
          </div>
          <div class="info-row">
            <span>Vencimento</span>
            <strong>{{ getVencimento() }}</strong>
          </div>
          <div class="info-row">
            <span>Seu saldo</span>
            <strong>{{ formatCurrency(getBalance()) }}</strong>
          </div>
        </div>

        <button class="btn-primary" [disabled]="!isCodeValid() || getAmount() <= 0 || isLoading" (click)="pay()">
          <span *ngIf="!isLoading">PAGAR BOLETO</span>
          <mat-spinner diameter="22" *ngIf="isLoading"></mat-spinner>
        </button>
      </div>

      <!-- Sucesso -->
      <div class="boleto-content success fade-in" *ngIf="paid">
        <div class="success-check">
          <mat-icon>check</mat-icon>
        </div>
        <h2>Boleto pago!</h2>
        <p class="success-amount">{{ formatCurrency(getAmount()) }}</p>
        <p class="success-dest">Empresa Exemplo LTDA</p>
        <button class="btn-primary" style="margin-top:40px" (click)="router.navigate(['/dashboard'])">
          VOLTAR AO INÍCIO
        </button>
      </div>
    </div>

    <app-bottom-nav *ngIf="!paid"></app-bottom-nav>
  `,
  styles: [`
    .boleto-container { min-height: 100vh; background: var(--krt-bg); }
    .page-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 20px; background: #fff; border-bottom: 1px solid #F0F0F0;
    }
    .page-header h1 { font-size: 1.05rem; font-weight: 700; margin: 0; }
    .back-btn { background: none; border: none; cursor: pointer; padding: 4px; color: #1A1A2E; display: flex; }

    .boleto-content { padding: 24px 20px; max-width: 480px; margin: 0 auto; }

    .barcode-area {
      background: #1A1A2E; border-radius: 18px; padding: 32px 20px;
      text-align: center; margin-bottom: 28px;
    }
    .barcode-area mat-icon { font-size: 48px; width: 48px; height: 48px; color: rgba(255,255,255,0.6); }
    .barcode-area p { color: rgba(255,255,255,0.7); font-size: 0.88rem; margin: 8px 0 0; }

    .field { margin-bottom: 20px; }
    .field label { display: block; font-size: 0.82rem; font-weight: 600; color: #555; margin-bottom: 8px; }
    .input-wrap {
      display: flex; align-items: center; gap: 10px;
      border: 2px solid #E5E7EB; border-radius: 14px;
      padding: 0 16px; height: 52px;
      transition: border-color 0.2s; background: #FAFBFC;
    }
    .input-wrap:focus-within { border-color: #0047BB; background: #fff; }
    .input-wrap input {
      flex: 1; border: none; outline: none; background: transparent;
      font-size: 0.95rem; font-family: 'Plus Jakarta Sans', sans-serif; color: #1A1A2E;
    }
    .input-wrap input::placeholder { color: #B0B8C4; }
    .input-wrap mat-icon { color: #9CA3AF; font-size: 22px; width: 22px; height: 22px; }
    .currency-prefix { color: #9CA3AF; font-weight: 600; font-size: 0.95rem; }
    .hint { font-size: 0.78rem; color: #9CA3AF; margin-top: 4px; display: block; }

    .info-card {
      background: #fff; border-radius: 16px; overflow: hidden;
      box-shadow: 0 2px 12px rgba(0,0,0,0.06); margin-bottom: 8px;
    }
    .info-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 16px 20px; border-bottom: 1px solid #F5F5F5;
    }
    .info-row:last-child { border-bottom: none; }
    .info-row span { font-size: 0.85rem; color: #9CA3AF; }
    .info-row strong { font-size: 0.9rem; color: #1A1A2E; }

    .btn-primary {
      width: 100%; height: 54px; border: none; border-radius: 14px;
      background: linear-gradient(135deg, #0047BB, #002a70);
      color: #fff; font-size: 0.95rem; font-weight: 700;
      cursor: pointer; display: flex; align-items: center; justify-content: center;
      box-shadow: 0 8px 24px rgba(0,71,187,0.3);
      transition: all 0.3s; font-family: 'Plus Jakarta Sans', sans-serif; margin-top: 24px;
    }
    .btn-primary:hover:not(:disabled) { transform: translateY(-1px); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

    .success { text-align: center; padding-top: 60px; }
    .success-check {
      width: 80px; height: 80px; border-radius: 50%;
      background: linear-gradient(135deg, #00C853, #00B894);
      display: flex; align-items: center; justify-content: center;
      margin: 0 auto 20px; box-shadow: 0 8px 30px rgba(0,200,83,0.3);
    }
    .success-check mat-icon { color: #fff; font-size: 40px; width: 40px; height: 40px; }
    .success h2 { font-size: 1.4rem; color: #1A1A2E; margin-bottom: 8px; }
    .success-amount { font-size: 2.2rem; font-weight: 800; color: #0047BB; margin: 4px 0; }
    .success-dest { color: #9CA3AF; font-size: 0.92rem; }

    ::ng-deep .mat-mdc-progress-spinner circle { stroke: white !important; }
  `]
})
export class BoletoComponent {
  code = '';
  amountDisplay = '';
  isLoading = false;
  paid = false;

  constructor(public router: Router) {}

  maskBoleto(event: any) {
    let v = event.target.value.replace(/\D/g, '');
    if (v.length > 47) v = v.slice(0, 47);
    let formatted = '';
    if (v.length > 0) formatted = v.slice(0, Math.min(5, v.length));
    if (v.length > 5) formatted += '.' + v.slice(5, Math.min(10, v.length));
    if (v.length > 10) formatted += ' ' + v.slice(10, Math.min(15, v.length));
    if (v.length > 15) formatted += '.' + v.slice(15, Math.min(21, v.length));
    if (v.length > 21) formatted += ' ' + v.slice(21, Math.min(26, v.length));
    if (v.length > 26) formatted += '.' + v.slice(26, Math.min(32, v.length));
    if (v.length > 32) formatted += ' ' + v.slice(32, Math.min(33, v.length));
    if (v.length > 33) formatted += ' ' + v.slice(33);
    this.code = formatted;
    event.target.value = formatted;
  }

  maskMoney(event: any) {
    let v = event.target.value.replace(/\D/g, '');
    if (!v) { this.amountDisplay = ''; event.target.value = ''; return; }
    const num = parseInt(v) / 100;
    this.amountDisplay = num.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    event.target.value = this.amountDisplay;
  }

  getAmount(): number {
    if (!this.amountDisplay) return 0;
    return parseFloat(this.amountDisplay.replace(/\./g, '').replace(',', '.')) || 0;
  }

  getBalance(): number { return parseFloat(localStorage.getItem('krt_account_balance') || '0'); }

  formatCurrency(v: number): string {
    return 'R$ ' + v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  isCodeValid(): boolean { return this.code.replace(/\D/g, '').length === 47; }

  getVencimento(): string {
    const d = new Date(); d.setDate(d.getDate() + 5);
    return d.toLocaleDateString('pt-BR');
  }

  pay() {
    this.isLoading = true;
    setTimeout(() => {
      const newBal = Math.max(0, this.getBalance() - this.getAmount());
      localStorage.setItem('krt_account_balance', String(newBal));
      const txs = JSON.parse(localStorage.getItem('krt_transactions') || '[]');
      txs.unshift({ id: Date.now().toString(), amount: this.getAmount(), type: 'DEBIT', description: 'Boleto - Empresa Exemplo', createdAt: new Date().toISOString() });
      localStorage.setItem('krt_transactions', JSON.stringify(txs.slice(0, 20)));
      this.isLoading = false;
      this.paid = true;
    }, 2000);
  }
}
