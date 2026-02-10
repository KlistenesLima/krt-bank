import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { PaymentService, PixTransaction } from '../../../core/services/payment.service';

@Component({
  selector: 'app-statement-page',
  template: `
    <div class="statement-container">
      <div class="header">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h2>Extrato</h2>
      </div>

      <div class="loading" *ngIf="loading">
        <mat-spinner diameter="32"></mat-spinner>
        <span>Carregando extrato...</span>
      </div>

      <div class="empty" *ngIf="!loading && transactions.length === 0">
        <mat-icon>receipt_long</mat-icon>
        <p>Nenhuma transacao encontrada</p>
      </div>

      <div class="tx-list" *ngIf="!loading && transactions.length > 0">
        <div class="tx-item" *ngFor="let tx of transactions" (click)="viewDetail(tx)">
          <div class="tx-icon" [ngClass]="getTxClass(tx)">
            <mat-icon>{{ getTxIcon(tx) }}</mat-icon>
          </div>
          <div class="tx-info">
            <strong>{{ getTxLabel(tx) }}</strong>
            <span class="tx-date">{{ tx.createdAt | date:'dd/MM/yyyy HH:mm' }}</span>
            <span class="tx-status" [ngClass]="'status-' + tx.status.toLowerCase()">
              {{ getStatusLabel(tx.status) }}
            </span>
          </div>
          <div class="tx-amount" [ngClass]="isIncoming(tx) ? 'incoming' : 'outgoing'">
            {{ isIncoming(tx) ? '+' : '-' }} R$ {{ tx.amount | number:'1.2-2' }}
          </div>
        </div>

        <button mat-button color="primary" (click)="loadMore()" *ngIf="hasMore"
                [disabled]="loadingMore">
          {{ loadingMore ? 'Carregando...' : 'Carregar mais' }}
        </button>
      </div>

      <app-bottom-nav></app-bottom-nav>
    </div>
  `,
  styles: [`
    .statement-container { padding: 16px 16px 80px; max-width: 500px; margin: 0 auto; }
    .header { display: flex; align-items: center; gap: 8px; margin-bottom: 20px; }
    .header h2 { margin: 0; color: var(--krt-primary); }
    .loading { display: flex; align-items: center; gap: 12px; justify-content: center; padding: 40px; }
    .empty { text-align: center; padding: 60px 20px; color: #999; }
    .empty mat-icon { font-size: 48px; width: 48px; height: 48px; margin-bottom: 12px; }
    .tx-list { display: flex; flex-direction: column; gap: 8px; }
    .tx-item {
      display: flex; align-items: center; gap: 12px;
      padding: 14px 16px; background: white; border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06); cursor: pointer;
      transition: transform 0.15s;
    }
    .tx-item:hover { transform: translateX(4px); }
      .tx-download-btn { background: none; border: none; cursor: pointer; padding: 6px; border-radius: 8px; display: flex; align-items: center; transition: all 0.2s; }
      .tx-download-btn:hover { background: rgba(0,71,187,0.08); }
      .tx-download-btn mat-icon { font-size: 22px; width: 22px; height: 22px; color: #0047BB; }
    .tx-icon {
      width: 40px; height: 40px; border-radius: 12px;
      display: flex; align-items: center; justify-content: center;
    }
    .tx-icon.sent { background: #ffe0e0; color: #e53935; }
    .tx-icon.received { background: #e0f7f0; color: #00c853; }
    .tx-icon mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .tx-info { flex: 1; display: flex; flex-direction: column; }
    .tx-info strong { font-size: 0.9rem; }
    .tx-date { font-size: 0.75rem; color: #999; }
    .tx-status { font-size: 0.7rem; font-weight: 600; }
    .status-completed { color: #00c853; }
    .status-pendinganalysis, .status-pending, .status-approved { color: #ff9800; }
    .status-rejected, .status-failed { color: #e53935; }
    .status-compensated { color: #9c27b0; }
    .status-underreview { color: #2196f3; }
    .tx-amount { font-weight: 700; font-size: 0.95rem; white-space: nowrap; }
    .tx-amount.incoming { color: #00c853; }
    .tx-amount.outgoing { color: #e53935; }
  `]
})
export class StatementPageComponent implements OnInit {
  transactions: PixTransaction[] = [];
  loading = true;
  loadingMore = false;
  hasMore = true;
  page = 1;
  pageSize = 20;
  accountId = '';

  constructor(
    private auth: AuthService,
    private paymentService: PaymentService,
    private router: Router
  ) {}

  ngOnInit() {
    this.accountId = this.auth.getAccountId() || '';
    if (this.accountId) {
      this.loadTransactions();
    } else {
      this.loading = false;
    }
  }

  loadTransactions() {
    this.loading = this.page === 1;
    this.loadingMore = this.page > 1;

    this.paymentService.getHistory(this.accountId, this.page, this.pageSize).subscribe({
      next: (txs) => {
        if (this.page === 1) {
          this.transactions = txs;
        } else {
          this.transactions = [...this.transactions, ...txs];
        }
        this.hasMore = txs.length === this.pageSize;
        this.loading = false;
        this.loadingMore = false;
      },
      error: (err) => {
        console.error('Erro ao carregar extrato:', err);
        this.loading = false;
        this.loadingMore = false;
      }
    });
  }

  loadMore() {
    this.page++;
    this.loadTransactions();
  }

  isIncoming(tx: PixTransaction): boolean {
    return tx.destinationAccountId === this.accountId;
  }

  getTxClass(tx: PixTransaction): string {
    return this.isIncoming(tx) ? 'received' : 'sent';
  }

  getTxIcon(tx: PixTransaction): string {
    return this.isIncoming(tx) ? 'call_received' : 'call_made';
  }

  getTxLabel(tx: PixTransaction): string {
    if (this.isIncoming(tx)) return 'Pix Recebido';
    return tx.description || 'Pix Enviado';
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      'Completed': 'Concluido',
      'PendingAnalysis': 'Em analise',
      'Pending': 'Processando',
      'Approved': 'Aprovado',
      'Rejected': 'Rejeitado',
      'Failed': 'Falhou',
      'Compensated': 'Estornado',
      'UnderReview': 'Em revisao',
      'SourceDebited': 'Processando'
    };
    return labels[status] || status;
  }

  viewDetail(tx: PixTransaction) {
    // Futuro: navegar para /receipt/:id
    console.log('Detalhe da transacao:', tx.transactionId);
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}

