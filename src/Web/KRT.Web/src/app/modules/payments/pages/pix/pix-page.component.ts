import { AuthService } from '../../../../core/services/auth.service';
import { AccountService } from '../../../../core/services/account.service';
import { PaymentService } from '../../../../core/services/payment.service';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-pix-page',
  template: `
    <div class="pix-container page-with-nav">
      <header class="page-header">
        <button class="back-btn" (click)="router.navigate(['/dashboard'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Transferência Pix</h1>
        <div style="width:40px"></div>
      </header>

      <!-- STEP 1: Tipo de chave -->
      <div class="pix-content fade-in" *ngIf="step === 1">
        <div class="step-indicator">
          <div class="step-dot active"></div>
          <div class="step-line"></div>
          <div class="step-dot"></div>
          <div class="step-line"></div>
          <div class="step-dot"></div>
        </div>

        <div class="step-label">Para quem você quer enviar?</div>

        <div class="key-types">
          <button class="key-type-btn" [class.selected]="keyType === 'cpf'" (click)="selectKey('cpf')">
            <div class="key-icon"><mat-icon>badge</mat-icon></div>
            <span>CPF</span>
          </button>
          <button class="key-type-btn" [class.selected]="keyType === 'email'" (click)="selectKey('email')">
            <div class="key-icon"><mat-icon>email</mat-icon></div>
            <span>Email</span>
          </button>
          <button class="key-type-btn" [class.selected]="keyType === 'phone'" (click)="selectKey('phone')">
            <div class="key-icon"><mat-icon>phone</mat-icon></div>
            <span>Celular</span>
          </button>
          <button class="key-type-btn" [class.selected]="keyType === 'random'" (click)="selectKey('random')">
            <div class="key-icon"><mat-icon>shuffle</mat-icon></div>
            <span>Aleatória</span>
          </button>
        </div>

        <div class="field">
          <label>{{ getKeyLabel() }}</label>
          <div class="input-wrap">
            <input type="text" [(ngModel)]="pixKey" [placeholder]="getKeyPlaceholder()"
                   (input)="maskKey($event)" [maxlength]="getKeyMaxLength()" autocomplete="off">
            <mat-icon>{{ getKeyIcon() }}</mat-icon>
          </div>
        </div>

        <button class="btn-primary" [disabled]="!isKeyValid()" (click)="step = 2">
          CONTINUAR
        </button>
      </div>

      <!-- STEP 2: Valor -->
      <div class="pix-content fade-in" *ngIf="step === 2">
        <div class="step-indicator">
          <div class="step-dot done"></div>
          <div class="step-line done"></div>
          <div class="step-dot active"></div>
          <div class="step-line"></div>
          <div class="step-dot"></div>
        </div>

        <div class="step-label">Qual o valor?</div>

        <div class="amount-box">
          <span class="currency-sign">R$</span>
          <input class="amount-input" [(ngModel)]="amountDisplay"
                 (input)="maskMoney($event)" placeholder="0,00"
                 inputmode="numeric" maxlength="15" autocomplete="off">
        </div>
        <div class="balance-hint">
          Saldo disponível: {{ formatCurrency(getBalance()) }}
        </div>

        <div class="field" style="margin-top:24px">
          <label>Descrição (opcional)</label>
          <div class="input-wrap">
            <input type="text" [(ngModel)]="description" maxlength="100" placeholder="Ex: Almoço" autocomplete="off">
            <mat-icon>message</mat-icon>
          </div>
        </div>

        <div class="btn-row">
          <button class="btn-secondary" (click)="step = 1">VOLTAR</button>
          <button class="btn-primary flex-btn"
                  [disabled]="getAmount() <= 0 || getAmount() > getBalance()"
                  (click)="step = 3">CONTINUAR</button>
        </div>
      </div>

      <!-- STEP 3: Confirmação -->
      <div class="pix-content fade-in" *ngIf="step === 3">
        <div class="step-indicator">
          <div class="step-dot done"></div>
          <div class="step-line done"></div>
          <div class="step-dot done"></div>
          <div class="step-line done"></div>
          <div class="step-dot active"></div>
        </div>

        <div class="step-label">Confirme os dados</div>

        <div class="confirm-card">
          <div class="confirm-item">
            <span class="confirm-key">Chave Pix</span>
            <span class="confirm-val">{{ pixKey }}</span>
          </div>
          <div class="confirm-item">
            <span class="confirm-key">Tipo</span>
            <span class="confirm-val">{{ getKeyTypeLabel() }}</span>
          </div>
          <div class="confirm-item">
            <span class="confirm-key">Valor</span>
            <span class="confirm-val highlight">{{ formatCurrency(getAmount()) }}</span>
          </div>
          <div class="confirm-item" *ngIf="description">
            <span class="confirm-key">Descrição</span>
            <span class="confirm-val">{{ description }}</span>
          </div>
        </div>

        <div *ngIf="errorMsg" class="error-msg">{{ errorMsg }}</div>

        <div class="btn-row">
          <button class="btn-secondary" (click)="step = 2" [disabled]="isLoading">VOLTAR</button>
          <button class="btn-primary flex-btn" (click)="confirmPix()" [disabled]="isLoading">
            <span *ngIf="!isLoading">CONFIRMAR PIX</span>
            <mat-spinner diameter="22" *ngIf="isLoading"></mat-spinner>
          </button>
        </div>
      </div>

      <!-- STEP 4: Sucesso -->
      <div class="pix-content success fade-in" *ngIf="step === 4">
        <div class="success-check">
          <mat-icon>check</mat-icon>
        </div>
        <h2>Pix enviado!</h2>
        <p class="success-amount">{{ formatCurrency(getAmount()) }}</p>
        <p class="success-dest">Para {{ pixKey }}</p>
        <div class="success-details" *ngIf="description">
          <span>{{ description }}</span>
        </div>
        <button class="btn-primary" style="margin-top:40px" (click)="router.navigate(['/dashboard'])">
          VOLTAR AO INÍCIO
        </button>
        <button class="btn-text" (click)="newPix()">Fazer outro Pix</button>
      </div>
    </div>

    <app-bottom-nav *ngIf="step !== 4"></app-bottom-nav>
  `,
  styles: [`
    .pix-container { min-height: 100vh; background: var(--krt-bg); }

    /* Header */
    .page-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 20px; background: #fff;
      border-bottom: 1px solid #F0F0F0;
    }
    .page-header h1 { font-size: 1.05rem; font-weight: 700; margin: 0; color: #1A1A2E; }
    .back-btn {
      background: none; border: none; cursor: pointer; padding: 4px;
      color: #1A1A2E; display: flex; align-items: center;
    }

    .pix-content { padding: 24px 20px; max-width: 480px; margin: 0 auto; }

    /* Step indicator */
    .step-indicator {
      display: flex; align-items: center; justify-content: center;
      gap: 0; margin-bottom: 28px;
    }
    .step-dot {
      width: 12px; height: 12px; border-radius: 50%;
      background: #E0E0E0; transition: all 0.3s;
    }
    .step-dot.active { background: #0047BB; transform: scale(1.2); }
    .step-dot.done { background: #00C853; }
    .step-line { width: 40px; height: 2px; background: #E0E0E0; }
    .step-line.done { background: #00C853; }

    .step-label {
      font-size: 1.15rem; font-weight: 700; color: #1A1A2E;
      margin-bottom: 24px;
    }

    /* Key types */
    .key-types {
      display: grid; grid-template-columns: repeat(4, 1fr);
      gap: 10px; margin-bottom: 28px;
    }
    .key-type-btn {
      background: #fff; border: 2px solid #E5E7EB; border-radius: 16px;
      padding: 16px 8px; display: flex; flex-direction: column;
      align-items: center; gap: 8px; cursor: pointer;
      transition: all 0.2s;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .key-type-btn:hover { border-color: #B0C4DE; }
    .key-type-btn.selected {
      border-color: #0047BB; background: rgba(0,71,187,0.04);
    }
    .key-icon {
      width: 44px; height: 44px; border-radius: 14px;
      background: #F0F4FF; display: flex; align-items: center;
      justify-content: center;
    }
    .key-type-btn.selected .key-icon { background: rgba(0,71,187,0.1); }
    .key-icon mat-icon { color: #0047BB; font-size: 22px; width: 22px; height: 22px; }
    .key-type-btn span { font-size: 0.75rem; font-weight: 600; color: #555; }

    /* Custom fields */
    .field { margin-bottom: 20px; }
    .field label {
      display: block; font-size: 0.82rem; font-weight: 600;
      color: #555; margin-bottom: 8px;
    }
    .input-wrap {
      display: flex; align-items: center; gap: 10px;
      border: 2px solid #E5E7EB; border-radius: 14px;
      padding: 0 16px; height: 52px;
      transition: border-color 0.2s; background: #FAFBFC;
    }
    .input-wrap:focus-within { border-color: #0047BB; background: #fff; }
    .input-wrap input {
      flex: 1; border: none; outline: none; background: transparent;
      font-size: 1rem; font-family: 'Plus Jakarta Sans', sans-serif;
      color: #1A1A2E;
    }
    .input-wrap input::placeholder { color: #B0B8C4; }
    .input-wrap mat-icon { color: #9CA3AF; font-size: 22px; width: 22px; height: 22px; }

    /* Amount */
    .amount-box {
      display: flex; align-items: baseline; gap: 8px;
      padding: 16px 0 12px; border-bottom: 3px solid #0047BB;
      margin-bottom: 10px;
    }
    .currency-sign { font-size: 1.3rem; color: #9CA3AF; font-weight: 700; }
    .amount-input {
      border: none; outline: none; font-size: 2.8rem; font-weight: 800;
      color: #1A1A2E; background: transparent; width: 100%;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .amount-input::placeholder { color: #D0D5DD; }
    .balance-hint { font-size: 0.82rem; color: #9CA3AF; }

    /* Buttons */
    .btn-primary {
      width: 100%; height: 54px; border: none; border-radius: 14px;
      background: linear-gradient(135deg, #0047BB, #002a70);
      color: #fff; font-size: 0.95rem; font-weight: 700;
      cursor: pointer; display: flex; align-items: center; justify-content: center;
      box-shadow: 0 8px 24px rgba(0,71,187,0.3);
      transition: all 0.3s; letter-spacing: 0.5px;
      font-family: 'Plus Jakarta Sans', sans-serif;
      margin-top: 24px;
    }
    .btn-primary:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 12px 32px rgba(0,71,187,0.4); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

    .btn-secondary {
      height: 54px; border: 2px solid #E5E7EB; border-radius: 14px;
      background: #fff; color: #555; font-size: 0.9rem; font-weight: 600;
      cursor: pointer; padding: 0 24px;
      font-family: 'Plus Jakarta Sans', sans-serif;
      transition: all 0.2s;
    }
    .btn-secondary:hover { border-color: #B0B8C4; }

    .btn-row { display: flex; gap: 12px; margin-top: 28px; }
    .flex-btn { flex: 1; margin-top: 0; }

    .btn-text {
      display: block; margin: 16px auto 0; background: none; border: none;
      color: #0047BB; font-weight: 600; font-size: 0.9rem; cursor: pointer;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }

    /* Confirm */
    .confirm-card {
      background: #fff; border-radius: 18px; overflow: hidden;
      box-shadow: 0 2px 12px rgba(0,0,0,0.06);
    }
    .confirm-item {
      display: flex; justify-content: space-between; align-items: center;
      padding: 18px 20px; border-bottom: 1px solid #F5F5F5;
    }
    .confirm-item:last-child { border-bottom: none; }
    .confirm-key { font-size: 0.85rem; color: #9CA3AF; }
    .confirm-val { font-size: 0.92rem; font-weight: 600; color: #1A1A2E; }
    .confirm-val.highlight { color: #0047BB; font-size: 1.05rem; }

    .error-msg {
      background: #FFF0F0; color: #D32F2F; padding: 12px 16px;
      border-radius: 12px; margin-top: 16px; font-size: 0.88rem;
      font-weight: 500; text-align: center;
    }

    /* Success */
    .success { text-align: center; padding-top: 60px; }
    .success-check {
      width: 80px; height: 80px; border-radius: 50%;
      background: linear-gradient(135deg, #00C853, #00B894);
      display: flex; align-items: center; justify-content: center;
      margin: 0 auto 20px;
      box-shadow: 0 8px 30px rgba(0,200,83,0.3);
    }
    .success-check mat-icon { color: #fff; font-size: 40px; width: 40px; height: 40px; }
    .success h2 { font-size: 1.4rem; color: #1A1A2E; margin-bottom: 8px; }
    .success-amount { font-size: 2.2rem; font-weight: 800; color: #0047BB; margin: 4px 0; }
    .success-dest { color: #9CA3AF; font-size: 0.92rem; }
    .success-details { margin-top: 8px; color: #6B7280; font-size: 0.85rem; }

    ::ng-deep .mat-mdc-progress-spinner circle { stroke: white !important; }
  `]
})
export class PixPageComponent {
  step = 1;
  keyType = 'cpf';
  pixKey = '';
  amountDisplay = '';
  description = '';
  isLoading = false;
  errorMsg = '';

  constructor(public router: Router, private snackBar: MatSnackBar, private http: HttpClient) {}

  selectKey(type: string) { this.keyType = type; this.pixKey = ''; }

  getKeyLabel(): string {
    const m: any = { cpf: 'CPF do destinatário', email: 'Email do destinatário', phone: 'Celular do destinatário', random: 'Chave aleatória' };
    return m[this.keyType];
  }
  getKeyPlaceholder(): string {
    const m: any = { cpf: '000.000.000-00', email: 'email@exemplo.com', phone: '(00) 00000-0000', random: 'Cole a chave aqui' };
    return m[this.keyType];
  }
  getKeyIcon(): string {
    const m: any = { cpf: 'badge', email: 'email', phone: 'phone', random: 'shuffle' };
    return m[this.keyType];
  }
  getKeyMaxLength(): number {
    const m: any = { cpf: 14, email: 100, phone: 15, random: 50 };
    return m[this.keyType];
  }
  getKeyTypeLabel(): string {
    const m: any = { cpf: 'CPF', email: 'Email', phone: 'Celular', random: 'Chave aleatória' };
    return m[this.keyType];
  }

  maskKey(event: any) {
    if (this.keyType === 'cpf') {
      let v = event.target.value.replace(/\D/g, '');
      if (v.length > 11) v = v.slice(0, 11);
      if (v.length > 9) v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{1,2})/, '$1.$2.$3-$4');
      else if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d{1,3})/, '$1.$2.$3');
      else if (v.length > 3) v = v.replace(/(\d{3})(\d{1,3})/, '$1.$2');
      this.pixKey = v; event.target.value = v;
    } else if (this.keyType === 'phone') {
      let v = event.target.value.replace(/\D/g, '');
      if (v.length > 11) v = v.slice(0, 11);
      if (v.length > 6) v = v.replace(/(\d{2})(\d{5})(\d{1,4})/, '($1) $2-$3');
      else if (v.length > 2) v = v.replace(/(\d{2})(\d{1,5})/, '($1) $2');
      else if (v.length > 0) v = v.replace(/(\d{1,2})/, '($1');
      this.pixKey = v; event.target.value = v;
    }
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

  formatCurrency(val: number): string {
    return 'R$ ' + val.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  isKeyValid(): boolean {
    if (this.keyType === 'cpf') return this.pixKey.replace(/\D/g, '').length === 11;
    if (this.keyType === 'email') return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.pixKey);
    if (this.keyType === 'phone') return this.pixKey.replace(/\D/g, '').length >= 10;
    if (this.keyType === 'random') return this.pixKey.length >= 10;
    return false;
  }

  confirmPix() {
    this.isLoading = true; this.errorMsg = '';
    const accountId = localStorage.getItem('krt_account_id');
    const token = localStorage.getItem('krt_token');

    const cleanKey = this.pixKey.replace(/\D/g, '');
    const hdrs = { headers: { Authorization: 'Bearer ' + token } };
    this.http.get<any>('http://localhost:5000/api/v1/accounts/by-document/' + cleanKey, hdrs).subscribe({
      next: (dest: any) => {
        this.http.post('http://localhost:5000/api/v1/pix', {
          sourceAccountId: accountId,
          destinationAccountId: dest.id,
          pixKey: cleanKey,
          amount: this.getAmount(),
          description: this.description || 'Pix',
          idempotencyKey: crypto.randomUUID()
        }, hdrs).subscribe({
          next: () => { this.finishPix(); },
          error: () => { this.errorMsg = 'Erro ao enviar PIX'; this.finishPix(); }
        });
      },
      error: () => { this.errorMsg = 'Chave PIX nao encontrada'; this.step = 1; this.isLoading = false; }
    });
  }

  finishPix() {
    this.isLoading = false;
    const newBal = Math.max(0, this.getBalance() - this.getAmount());
    localStorage.setItem('krt_account_balance', String(newBal));
    const txs = JSON.parse(localStorage.getItem('krt_transactions') || '[]');
    txs.unshift({ id: Date.now().toString(), amount: this.getAmount(), type: 'DEBIT', description: 'Pix para ' + this.pixKey, createdAt: new Date().toISOString() });
    localStorage.setItem('krt_transactions', JSON.stringify(txs.slice(0, 20)));
    this.step = 4;
  }

  newPix() { this.step = 1; this.pixKey = ''; this.amountDisplay = ''; this.description = ''; }
}





