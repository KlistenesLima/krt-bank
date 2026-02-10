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
        <div class="tx-card" *ngFor="let tx of transactions"
             [class.expanded]="expandedId === tx.transactionId">

          <!-- Linha principal -->
          <div class="tx-row" (click)="toggleExpand(tx)">
            <div class="tx-icon" [ngClass]="getTxClass(tx)">
              <mat-icon>{{ getTxIcon(tx) }}</mat-icon>
            </div>
            <div class="tx-info">
              <strong>{{ getTxLabel(tx) }}</strong>
              <span class="tx-date">{{ tx.createdAt | date:'dd/MM/yyyy HH:mm' }}</span>
            </div>
            <div class="tx-right">
              <span class="tx-amount" [ngClass]="isIncoming(tx) ? 'incoming' : 'outgoing'">
                {{ isIncoming(tx) ? '+' : '-' }} R$ {{ tx.amount | number:'1.2-2' }}
              </span>
              <mat-icon class="expand-icon">{{ expandedId === tx.transactionId ? 'expand_less' : 'expand_more' }}</mat-icon>
            </div>
          </div>

          <!-- Detalhes expandidos -->
          <div class="tx-details" *ngIf="expandedId === tx.transactionId">
            <div class="detail-divider"></div>

            <div class="detail-row">
              <span class="detail-label">Status</span>
              <span class="detail-value status-badge" [ngClass]="'badge-' + tx.status.toLowerCase()">
                {{ getStatusLabel(tx.status) }}
              </span>
            </div>

            <div class="detail-row">
              <span class="detail-label">Tipo</span>
              <span class="detail-value">{{ isIncoming(tx) ? 'Recebido' : 'Enviado' }}</span>
            </div>

            <div class="detail-row">
              <span class="detail-label">Valor</span>
              <span class="detail-value bold">R$ {{ tx.amount | number:'1.2-2' }}</span>
            </div>

            <div class="detail-row" *ngIf="tx.description">
              <span class="detail-label">Descricao</span>
              <span class="detail-value">{{ tx.description }}</span>
            </div>

            <div class="detail-row">
              <span class="detail-label">Data</span>
              <span class="detail-value">{{ tx.createdAt | date:'dd/MM/yyyy HH:mm:ss' }}</span>
            </div>

            <div class="detail-row" *ngIf="tx.completedAt">
              <span class="detail-label">Concluido em</span>
              <span class="detail-value">{{ tx.completedAt | date:'dd/MM/yyyy HH:mm:ss' }}</span>
            </div>

            <div class="detail-row" *ngIf="tx.fraudScore !== null && tx.fraudScore !== undefined">
              <span class="detail-label">Score anti-fraude</span>
              <span class="detail-value">{{ tx.fraudScore }}</span>
            </div>

            <div class="detail-row">
              <span class="detail-label">ID</span>
              <span class="detail-value tx-id">{{ tx.transactionId }}</span>
            </div>

            <button class="btn-receipt" (click)="downloadReceipt(tx.transactionId); $event.stopPropagation()"
                    [disabled]="downloadingId === tx.transactionId">
              <mat-icon>{{ downloadingId === tx.transactionId ? 'hourglass_top' : 'download' }}</mat-icon>
              {{ downloadingId === tx.transactionId ? 'Gerando...' : 'Baixar Comprovante' }}
            </button>
          </div>
        </div>

        <button mat-button color="primary" (click)="loadMore()" *ngIf="hasMore"
                [disabled]="loadingMore" class="load-more-btn">
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

    /* Card da transacao */
    .tx-card {
      background: white; border-radius: 14px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06);
      overflow: hidden; transition: all 0.3s ease;
    }
    .tx-card.expanded {
      box-shadow: 0 4px 20px rgba(0,71,187,0.12);
      border: 1px solid rgba(0,71,187,0.15);
    }

    /* Linha principal */
    .tx-row {
      display: flex; align-items: center; gap: 12px;
      padding: 14px 16px; cursor: pointer;
      transition: background 0.15s;
    }
    .tx-row:hover { background: #FAFBFC; }

    .tx-icon {
      width: 40px; height: 40px; border-radius: 12px;
      display: flex; align-items: center; justify-content: center;
      flex-shrink: 0;
    }
    .tx-icon.sent { background: #ffe0e0; color: #e53935; }
    .tx-icon.received { background: #e0f7f0; color: #00c853; }
    .tx-icon mat-icon { font-size: 20px; width: 20px; height: 20px; }

    .tx-info { flex: 1; display: flex; flex-direction: column; }
    .tx-info strong { font-size: 0.9rem; color: #1A1A2E; }
    .tx-date { font-size: 0.75rem; color: #9CA3AF; }

    .tx-right { display: flex; align-items: center; gap: 4px; }
    .tx-amount { font-weight: 700; font-size: 0.95rem; white-space: nowrap; }
    .tx-amount.incoming { color: #00c853; }
    .tx-amount.outgoing { color: #e53935; }
    .expand-icon { color: #9CA3AF; font-size: 20px; width: 20px; height: 20px; transition: transform 0.3s; }

    /* Detalhes expandidos */
    .tx-details {
      padding: 0 16px 16px;
      animation: slideDown 0.25s ease;
    }
    @keyframes slideDown {
      from { opacity: 0; transform: translateY(-8px); }
      to { opacity: 1; transform: translateY(0); }
    }

    .detail-divider {
      height: 1px; background: #F0F0F0;
      margin-bottom: 14px;
    }

    .detail-row {
      display: flex; justify-content: space-between;
      align-items: center; padding: 6px 0;
    }
    .detail-label { font-size: 0.82rem; color: #9CA3AF; }
    .detail-value { font-size: 0.85rem; color: #1A1A2E; font-weight: 500; }
    .detail-value.bold { font-weight: 700; font-size: 0.95rem; }
    .tx-id { font-size: 0.7rem; font-family: monospace; color: #6B7280; max-width: 180px; overflow: hidden; text-overflow: ellipsis; }

    /* Badge de status */
    .status-badge {
      padding: 3px 10px; border-radius: 20px;
      font-size: 0.75rem; font-weight: 700;
    }
    .badge-completed { background: #E8F5E9; color: #2E7D32; }
    .badge-pendinganalysis, .badge-pending, .badge-approved, .badge-sourcedebited { background: #FFF3E0; color: #E65100; }
    .badge-rejected, .badge-failed { background: #FFEBEE; color: #C62828; }
    .badge-compensated { background: #F3E5F5; color: #6A1B9A; }
    .badge-underreview { background: #E3F2FD; color: #1565C0; }

    /* Botao comprovante */
    .btn-receipt {
      display: flex; align-items: center; justify-content: center; gap: 8px;
      width: 100%; height: 44px; margin-top: 14px;
      border: 2px solid #0047BB; border-radius: 12px;
      background: rgba(0,71,187,0.05); color: #0047BB;
      font-size: 0.85rem; font-weight: 700; cursor: pointer;
      font-family: 'Plus Jakarta Sans', sans-serif;
      transition: all 0.2s;
    }
    .btn-receipt:hover { background: rgba(0,71,187,0.12); }
    .btn-receipt:disabled { opacity: 0.6; cursor: wait; }
    .btn-receipt mat-icon { font-size: 18px; width: 18px; height: 18px; }

    .load-more-btn { margin-top: 12px; }
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
  expandedId: string | null = null;
  downloadingId: string | null = null;

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

  toggleExpand(tx: PixTransaction) {
    if (this.expandedId === tx.transactionId) {
      this.expandedId = null;
    } else {
      this.expandedId = tx.transactionId;
    }
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

  downloadReceipt(txId: string) {
    this.downloadingId = txId;
    const token = localStorage.getItem('krt_token');
    const url = 'http://localhost:5000/api/v1/pix/receipt/' + txId;
    fetch(url, { headers: { Authorization: 'Bearer ' + (token || '') } })
      .then(r => {
        if (!r.ok) throw new Error('Erro');
        return r.blob();
      })
      .then(blob => {
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = 'comprovante-pix-' + txId.substring(0, 8) + '.pdf';
        a.click();
        URL.revokeObjectURL(a.href);
        this.downloadingId = null;
      })
      .catch(() => {
        alert('Comprovante indisponivel para esta transacao.');
        this.downloadingId = null;
      });
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}
