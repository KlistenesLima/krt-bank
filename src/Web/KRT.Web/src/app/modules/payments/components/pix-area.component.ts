import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { AccountService, AccountDto } from '../../../core/services/account.service';
import { PaymentService, PixTransferRequest } from '../../../core/services/payment.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-pix-area',
  template: `
    <div class="pix-container">
      <h2>Enviar Pix</h2>

      <!-- Step 1: Buscar destinatario -->
      <div class="form-section" *ngIf="step === 1">
        <label>CPF do destinatario</label>
        <input type="text" [(ngModel)]="destCpf" placeholder="000.000.000-00"
               maxlength="14" (input)="formatCpf()">
        <button mat-raised-button color="primary" (click)="searchDestination()"
                [disabled]="searching || destCpf.length < 11">
          {{ searching ? 'Buscando...' : 'Buscar' }}
        </button>

        <div class="dest-card" *ngIf="destAccount">
          <mat-icon>person</mat-icon>
          <div>
            <strong>{{ destAccount.customerName }}</strong>
            <span>CPF: {{ maskCpf(destAccount.document) }}</span>
          </div>
        </div>
        <div class="error-msg" *ngIf="destError">{{ destError }}</div>
        <button mat-raised-button color="accent" (click)="step = 2" *ngIf="destAccount">
          Continuar
        </button>
      </div>

      <!-- Step 2: Valor e descricao -->
      <div class="form-section" *ngIf="step === 2">
        <div class="dest-summary">
          Para: <strong>{{ destAccount?.customerName }}</strong>
        </div>
        <label>Valor (R$)</label>
        <input type="number" [(ngModel)]="amount" placeholder="0.00" min="0.01" step="0.01">
        <label>Descricao (opcional)</label>
        <input type="text" [(ngModel)]="description" placeholder="Pagamento, transferencia...">
        <div class="actions">
          <button mat-button (click)="step = 1">Voltar</button>
          <button mat-raised-button color="primary" (click)="confirmPix()"
                  [disabled]="!amount || amount <= 0 || sending">
            {{ sending ? 'Enviando...' : 'Confirmar Pix' }}
          </button>
        </div>
      </div>

      <!-- Step 3: Resultado -->
      <div class="form-section result" *ngIf="step === 3">
        <mat-icon class="success-icon">check_circle</mat-icon>
        <h3>Pix Enviado!</h3>
        <p>R$ {{ amount | number:'1.2-2' }} para {{ destAccount?.customerName }}</p>
        <p class="status-info">Status: {{ pixStatus }}</p>
        <p class="tx-id" *ngIf="transactionId">ID: {{ transactionId }}</p>
        <button mat-raised-button color="primary" (click)="reset()">Novo Pix</button>
        <button mat-button (click)="goToDashboard()">Voltar ao inicio</button>
      </div>
    </div>
  `,
  styles: [`
    .pix-container {
      padding: 20px;
      max-width: 400px;
      margin: 0 auto;
    }
    h2 { color: var(--krt-primary); margin-bottom: 20px; }
    .form-section { display: flex; flex-direction: column; gap: 12px; }
    label { font-weight: 600; font-size: 0.85rem; color: #555; }
    input {
      padding: 12px 16px;
      border: 2px solid #e0e0e0;
      border-radius: 12px;
      font-size: 1rem;
      outline: none;
      transition: border-color 0.2s;
    }
    input:focus { border-color: var(--krt-primary); }
    .dest-card {
      display: flex; align-items: center; gap: 12px;
      padding: 16px; background: #f0faf7; border-radius: 12px;
      border: 1px solid #00d4aa33;
    }
    .dest-card mat-icon { color: var(--krt-primary); font-size: 32px; width: 32px; height: 32px; }
    .dest-card strong { display: block; font-size: 1rem; }
    .dest-card span { font-size: 0.8rem; color: #777; }
    .dest-summary {
      padding: 12px 16px; background: #f5f5f5; border-radius: 8px;
      font-size: 0.9rem;
    }
    .error-msg { color: #e53935; font-size: 0.85rem; }
    .actions { display: flex; gap: 12px; justify-content: flex-end; }
    .result { align-items: center; text-align: center; }
    .success-icon { font-size: 64px; width: 64px; height: 64px; color: #00d4aa; }
    .status-info { color: #777; font-size: 0.85rem; }
    .tx-id { font-family: monospace; font-size: 0.75rem; color: #999; word-break: break-all; }
  `]
})
export class PixAreaComponent {
  step = 1;
  destCpf = '';
  destAccount: AccountDto | null = null;
  destError = '';
  searching = false;
  amount = 0;
  description = '';
  sending = false;
  transactionId = '';
  pixStatus = '';

  constructor(
    private auth: AuthService,
    private accountService: AccountService,
    private paymentService: PaymentService,
    private notification: NotificationService,
    private router: Router
  ) {}

  formatCpf() {
    let v = this.destCpf.replace(/\D/g, '');
    if (v.length > 11) v = v.substring(0, 11);
    if (v.length > 9) v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{1,2})/, '$1.$2.$3-$4');
    else if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d{1,3})/, '$1.$2.$3');
    else if (v.length > 3) v = v.replace(/(\d{3})(\d{1,3})/, '$1.$2');
    this.destCpf = v;
  }

  maskCpf(cpf: string): string {
    if (!cpf || cpf.length < 11) return cpf;
    return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.***.***-$4');
  }

  searchDestination() {
    this.searching = true;
    this.destError = '';
    this.destAccount = null;

    const cleanCpf = this.destCpf.replace(/\D/g, '');

    if (cleanCpf === localStorage.getItem('krt_account_doc')) {
      this.destError = 'Voce nao pode enviar Pix para si mesmo';
      this.searching = false;
      return;
    }

    this.accountService.getByDocument(cleanCpf).subscribe({
      next: (account) => {
        this.destAccount = account;
        this.searching = false;
      },
      error: (err) => {
        this.destError = err.status === 404
          ? 'Conta nao encontrada com esse CPF'
          : 'Erro ao buscar conta. Tente novamente.';
        this.searching = false;
      }
    });
  }

  confirmPix() {
    if (!this.destAccount || !this.amount || this.amount <= 0) return;
    this.sending = true;

    const request: PixTransferRequest = {
      sourceAccountId: this.auth.getAccountId() || '',
      destinationAccountId: this.destAccount.id,
      pixKey: this.destAccount.document,
      amount: this.amount,
      description: this.description || 'Pix',
      idempotencyKey: crypto.randomUUID()
    };

    this.paymentService.sendPix(request).subscribe({
      next: (res) => {
        this.transactionId = res.transactionId;
        this.pixStatus = res.status || 'PendingAnalysis';
        this.step = 3;
        this.sending = false;
        this.notification.success('Pix enviado para analise anti-fraude!');
      },
      error: (err) => {
        this.sending = false;
        const msg = err.error?.error || err.error?.message || 'Erro ao enviar Pix';
        this.notification.error(msg);
      }
    });
  }

  reset() {
    this.step = 1;
    this.destCpf = '';
    this.destAccount = null;
    this.destError = '';
    this.amount = 0;
    this.description = '';
    this.transactionId = '';
    this.pixStatus = '';
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}