import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-boleto',
  template: `
    <div class="boleto-container page-with-nav">
      <header class="page-header">
        <button class="back-btn" (click)="goBack()">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Pagar Boleto</h1>
        <div style="width:40px"></div>
      </header>

      <!-- Step 1: Enter code -->
      <div class="boleto-content fade-in" *ngIf="step === 'input'">
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

        <div class="error-msg" *ngIf="errorMsg">
          <mat-icon>error_outline</mat-icon>
          <span>{{ errorMsg }}</span>
        </div>

        <button class="btn-primary" [disabled]="!isCodeValid() || isLoading" (click)="searchBoleto()">
          <span *ngIf="!isLoading">BUSCAR BOLETO</span>
          <mat-spinner diameter="22" *ngIf="isLoading"></mat-spinner>
        </button>
      </div>

      <!-- Step 2: Confirm payment -->
      <div class="boleto-content fade-in" *ngIf="step === 'confirm'">
        <div class="info-card">
          <div class="info-row">
            <span>Beneficiário</span>
            <strong>AUREA Maison Joalheria</strong>
          </div>
          <div class="info-row">
            <span>Descrição</span>
            <strong>{{ chargeData?.description || 'Boleto bancário' }}</strong>
          </div>
          <div class="info-row">
            <span>Valor</span>
            <strong class="amount-highlight">{{ formatCurrency(chargeData?.amount || 0) }}</strong>
          </div>
          <div class="info-row">
            <span>Vencimento</span>
            <strong>{{ formatDate(chargeData?.dueDate) }}</strong>
          </div>
          <div class="info-row digitable" *ngIf="chargeData?.digitableLine">
            <span>Linha digitavel</span>
            <strong style="font-size:0.72rem;word-break:break-all;text-align:right;max-width:60%">{{ chargeData?.digitableLine }}</strong>
          </div>
          <div class="info-row">
            <span>Seu saldo</span>
            <strong>{{ formatCurrency(getBalance()) }}</strong>
          </div>
        </div>

        <div class="error-msg" *ngIf="errorMsg">
          <mat-icon>error_outline</mat-icon>
          <span>{{ errorMsg }}</span>
        </div>

        <button class="btn-primary" [disabled]="isLoading" (click)="confirmPayment()">
          <span *ngIf="!isLoading">CONFIRMAR PAGAMENTO</span>
          <mat-spinner diameter="22" *ngIf="isLoading"></mat-spinner>
        </button>
        <button class="btn-secondary" (click)="step = 'input'; errorMsg = ''" [disabled]="isLoading">
          VOLTAR
        </button>
      </div>

      <!-- Step 3: Success -->
      <div class="boleto-content success fade-in" *ngIf="step === 'success'">
        <div class="success-check">
          <mat-icon>check</mat-icon>
        </div>
        <h2>Boleto pago!</h2>
        <p class="success-amount">{{ formatCurrency(chargeData?.amount || 0) }}</p>
        <p class="success-dest">AUREA Maison Joalheria</p>
        <p class="success-balance" *ngIf="newBalance !== null">Novo saldo: {{ formatCurrency(newBalance!) }}</p>
        <button class="btn-primary" style="margin-top:40px" (click)="router.navigate(['/dashboard'])">
          VOLTAR AO INÍCIO
        </button>
      </div>
    </div>

    <app-bottom-nav *ngIf="step !== 'success'"></app-bottom-nav>
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
    .amount-highlight { color: #0047BB !important; font-size: 1rem !important; }

    .error-msg {
      display: flex; align-items: center; gap: 8px; padding: 12px 16px;
      background: #FFF0F0; border-radius: 12px; margin-bottom: 16px;
    }
    .error-msg mat-icon { color: #E53935; font-size: 20px; width: 20px; height: 20px; }
    .error-msg span { color: #C62828; font-size: 0.85rem; }

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

    .btn-secondary {
      width: 100%; height: 48px; border: 2px solid #E5E7EB; border-radius: 14px;
      background: transparent; color: #555; font-size: 0.9rem; font-weight: 600;
      cursor: pointer; display: flex; align-items: center; justify-content: center;
      transition: all 0.2s; font-family: 'Plus Jakarta Sans', sans-serif; margin-top: 12px;
    }
    .btn-secondary:hover:not(:disabled) { border-color: #0047BB; color: #0047BB; }
    .btn-secondary:disabled { opacity: 0.5; cursor: not-allowed; }

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
    .success-balance { color: #00C853; font-size: 0.95rem; font-weight: 600; margin-top: 8px; }

    ::ng-deep .mat-mdc-progress-spinner circle { stroke: white !important; }
  `]
})
export class BoletoComponent {
  code = '';
  isLoading = false;
  errorMsg = '';
  step: 'input' | 'confirm' | 'success' = 'input';
  chargeData: any = null;
  newBalance: number | null = null;

  private readonly API = 'http://localhost:5000/api/v1/boletos/charges';

  constructor(public router: Router, private http: HttpClient) {}

  goBack() {
    if (this.step === 'confirm') {
      this.step = 'input';
      this.errorMsg = '';
    } else {
      this.router.navigate(['/dashboard']);
    }
  }

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

  getBalance(): number { return parseFloat(localStorage.getItem('krt_account_balance') || '0'); }

  formatCurrency(v: number): string {
    return 'R$ ' + v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  formatDate(d: string | null): string {
    if (!d) return '-';
    return new Date(d).toLocaleDateString('pt-BR');
  }

  isCodeValid(): boolean { return this.code.replace(/\D/g, '').length === 47; }

  searchBoleto() {
    this.isLoading = true;
    this.errorMsg = '';
    const digitableLine = this.code.replace(/\D/g, '');

    const token = localStorage.getItem('krt_token') || '';
    const hdrs = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.http.post<any>(`${this.API}/find-by-digitable-line`, { digitableLine }, { headers: hdrs })
      .subscribe({
        next: (res) => {
          this.chargeData = res;
          this.step = 'confirm';
          this.isLoading = false;
        },
        error: (err) => {
          this.errorMsg = err.error?.error || 'Boleto não encontrado. Verifique o código digitado.';
          this.isLoading = false;
        }
      });
  }

  confirmPayment() {
    if (!this.chargeData?.chargeId) return;
    this.isLoading = true;
    this.errorMsg = '';

    const token = localStorage.getItem('krt_token') || '';
    const accountId = localStorage.getItem('krt_account_id') || '';
    const hdrs = new HttpHeaders({ Authorization: `Bearer ${token}` });

    const body = accountId ? { payerAccountId: accountId } : {};

    this.http.post<any>(`${this.API}/${this.chargeData.chargeId}/simulate-payment`, body, { headers: hdrs })
      .subscribe({
        next: (res) => {
          if (res.newBalance !== undefined && res.newBalance !== null) {
            this.newBalance = res.newBalance;
            localStorage.setItem('krt_account_balance', String(res.newBalance));
          }
          this.step = 'success';
          this.isLoading = false;
        },
        error: (err) => {
          this.errorMsg = err.error?.error || 'Erro ao processar pagamento. Tente novamente.';
          this.isLoading = false;
        }
      });
  }
}
