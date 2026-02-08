import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-recharge',
  template: `
    <div class="recharge-container page-with-nav">
      <header class="page-header">
        <button class="back-btn" (click)="router.navigate(['/dashboard'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Recarga de Celular</h1>
        <div style="width:40px"></div>
      </header>

      <div class="content fade-in" *ngIf="!done">
        <div class="section-label">Qual número deseja recarregar?</div>

        <div class="field">
          <label>Número com DDD</label>
          <div class="input-wrap">
            <input type="text" [(ngModel)]="phone" placeholder="(00) 00000-0000"
                   (input)="maskPhone($event)" maxlength="15" inputmode="numeric" autocomplete="off">
            <mat-icon>smartphone</mat-icon>
          </div>
        </div>

        <div class="section-label" style="margin-top:28px">Valor da recarga</div>

        <div class="values-grid">
          <button class="value-card" *ngFor="let v of values" [class.selected]="selected === v.amount" (click)="selected = v.amount">
            <span class="val-amount">{{ formatCurrency(v.amount) }}</span>
            <span class="val-bonus" *ngIf="v.bonus">{{ v.bonus }}</span>
          </button>
        </div>

        <div class="balance-hint">
          Saldo disponível: {{ formatCurrency(getBalance()) }}
        </div>

        <button class="btn-primary" [disabled]="!isValid() || isLoading" (click)="confirm()">
          <span *ngIf="!isLoading">CONFIRMAR RECARGA</span>
          <mat-spinner diameter="22" *ngIf="isLoading"></mat-spinner>
        </button>
      </div>

      <!-- Sucesso -->
      <div class="content success fade-in" *ngIf="done">
        <div class="success-check">
          <mat-icon>check</mat-icon>
        </div>
        <h2>Recarga realizada!</h2>
        <p class="success-amount">{{ formatCurrency(selected) }}</p>
        <p class="success-dest">Para {{ phone }}</p>
        <button class="btn-primary" style="margin-top:40px" (click)="router.navigate(['/dashboard'])">
          VOLTAR AO INÍCIO
        </button>
        <button class="btn-text" (click)="reset()">Fazer outra recarga</button>
      </div>
    </div>

    <app-bottom-nav *ngIf="!done"></app-bottom-nav>
  `,
  styles: [`
    .recharge-container { min-height: 100vh; background: var(--krt-bg); }

    .page-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 20px; background: #fff; border-bottom: 1px solid #F0F0F0;
    }
    .page-header h1 { font-size: 1.05rem; font-weight: 700; margin: 0; color: #1A1A2E; }
    .back-btn { background: none; border: none; cursor: pointer; padding: 4px; color: #1A1A2E; display: flex; }

    .content { padding: 24px 20px; max-width: 480px; margin: 0 auto; }

    .section-label { font-size: 1rem; font-weight: 700; color: #1A1A2E; margin-bottom: 16px; }

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
      font-size: 1rem; font-family: 'Plus Jakarta Sans', sans-serif; color: #1A1A2E;
    }
    .input-wrap input::placeholder { color: #B0B8C4; }
    .input-wrap mat-icon { color: #9CA3AF; font-size: 22px; width: 22px; height: 22px; }

    /* Value cards */
    .values-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 16px; }
    .value-card {
      background: #fff; border: 2px solid #E5E7EB; border-radius: 16px;
      padding: 20px 16px; text-align: center; cursor: pointer;
      transition: all 0.2s; display: flex; flex-direction: column; gap: 4px;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .value-card:hover { border-color: #B0C4DE; }
    .value-card.selected {
      border-color: #0047BB; background: rgba(0,71,187,0.04);
      box-shadow: 0 0 0 1px #0047BB;
    }
    .val-amount { font-size: 1.1rem; font-weight: 700; color: #1A1A2E; }
    .value-card.selected .val-amount { color: #0047BB; }
    .val-bonus {
      font-size: 0.75rem; font-weight: 600; color: #00C853;
      background: rgba(0,200,83,0.08); padding: 2px 10px;
      border-radius: 20px; display: inline-block; margin: 2px auto 0;
    }

    .balance-hint { font-size: 0.82rem; color: #9CA3AF; margin-bottom: 4px; }

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

    .btn-text {
      display: block; margin: 16px auto 0; background: none; border: none;
      color: #0047BB; font-weight: 600; font-size: 0.9rem; cursor: pointer;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }

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
export class RechargeComponent {
  phone = '';
  selected = 0;
  isLoading = false;
  done = false;

  values = [
    { amount: 15, bonus: null },
    { amount: 20, bonus: null },
    { amount: 30, bonus: '+1GB Bônus' },
    { amount: 50, bonus: '+2GB Bônus' },
  ];

  constructor(public router: Router) {}

  maskPhone(event: any) {
    let v = event.target.value.replace(/\D/g, '');
    if (v.length > 11) v = v.slice(0, 11);
    if (v.length > 6) v = v.replace(/(\d{2})(\d{5})(\d{1,4})/, '($1) $2-$3');
    else if (v.length > 2) v = v.replace(/(\d{2})(\d{1,5})/, '($1) $2');
    else if (v.length > 0) v = v.replace(/(\d{1,2})/, '($1');
    this.phone = v; event.target.value = v;
  }

  getBalance(): number { return parseFloat(localStorage.getItem('krt_account_balance') || '0'); }

  formatCurrency(v: number): string {
    return 'R$ ' + v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  isValid(): boolean {
    return this.phone.replace(/\D/g, '').length >= 10 && this.selected > 0 && this.selected <= this.getBalance();
  }

  confirm() {
    this.isLoading = true;
    setTimeout(() => {
      const newBal = Math.max(0, this.getBalance() - this.selected);
      localStorage.setItem('krt_account_balance', String(newBal));
      const txs = JSON.parse(localStorage.getItem('krt_transactions') || '[]');
      txs.unshift({ id: Date.now().toString(), amount: this.selected, type: 'DEBIT', description: 'Recarga ' + this.phone, createdAt: new Date().toISOString() });
      localStorage.setItem('krt_transactions', JSON.stringify(txs.slice(0, 20)));
      this.isLoading = false;
      this.done = true;
    }, 1500);
  }

  reset() { this.phone = ''; this.selected = 0; this.done = false; }
}
