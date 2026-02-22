import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { PaymentService, StatementEntry } from '../../../core/services/payment.service';

@Component({
  selector: 'app-statement-page',
  template: `
    <div class="statement-container">
      <div class="header">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h2>Extrato</h2>
        <button mat-icon-button class="download-btn" (click)="downloadCsv()"
                *ngIf="transactions.length > 0" title="Baixar extrato CSV">
          <mat-icon>download</mat-icon>
        </button>
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
             [class.expanded]="expandedId === tx.id">

          <!-- Linha principal -->
          <div class="tx-row" (click)="toggleExpand(tx)">
            <div class="tx-icon" [ngClass]="getTxClass(tx)">
              <mat-icon>{{ getTxIcon(tx) }}</mat-icon>
            </div>
            <div class="tx-info">
              <strong>{{ getTxLabel(tx) }}</strong>
              <span class="tx-date">{{ tx.date | date:'dd/MM/yyyy HH:mm' }}</span>
            </div>
            <div class="tx-right">
              <span class="tx-amount" [ngClass]="tx.isCredit ? 'incoming' : 'outgoing'">
                {{ tx.isCredit ? '+' : '-' }} R$ {{ tx.amount | number:'1.2-2' }}
              </span>
              <mat-icon class="expand-icon">{{ expandedId === tx.id ? 'expand_less' : 'expand_more' }}</mat-icon>
            </div>
          </div>

          <!-- Detalhes expandidos -->
          <div class="tx-details" *ngIf="expandedId === tx.id">
            <div class="detail-divider"></div>

            <div class="detail-row">
              <span class="detail-label">Tipo</span>
              <span class="detail-value status-badge" [ngClass]="'badge-' + tx.type.toLowerCase()">
                {{ getTypeLabel(tx.type) }}
              </span>
            </div>

            <div class="detail-row">
              <span class="detail-label">Categoria</span>
              <span class="detail-value">{{ tx.category }}</span>
            </div>

            <div class="detail-row">
              <span class="detail-label">Valor</span>
              <span class="detail-value bold">R$ {{ tx.amount | number:'1.2-2' }}</span>
            </div>

            <div class="detail-row" *ngIf="tx.description">
              <span class="detail-label">Descricao</span>
              <span class="detail-value">{{ tx.description }}</span>
            </div>

            <div class="detail-row" *ngIf="tx.counterpartyName">
              <span class="detail-label">Contraparte</span>
              <span class="detail-value">{{ tx.counterpartyName }}</span>
            </div>

            <div class="detail-row">
              <span class="detail-label">Data</span>
              <span class="detail-value">{{ tx.date | date:'dd/MM/yyyy HH:mm:ss' }}</span>
            </div>

            <div class="detail-row">
              <span class="detail-label">ID</span>
              <span class="detail-value tx-id">{{ tx.id }}</span>
            </div>
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
    .header h2 { margin: 0; color: var(--krt-primary); flex: 1; }
    .download-btn { color: var(--krt-primary, #0047BB); }
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
    .tx-icon.pix { background: #e0f7f0; color: #00c853; }
    .tx-icon.boleto { background: #fff3e0; color: #ff6d00; }
    .tx-icon.cartao { background: #e3f2fd; color: #1565c0; }
    .tx-icon.fatura { background: #fce4ec; color: #c62828; }
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

    /* Badge de tipo */
    .status-badge {
      padding: 3px 10px; border-radius: 20px;
      font-size: 0.75rem; font-weight: 700;
    }
    .badge-pix { background: #E8F5E9; color: #2E7D32; }
    .badge-boleto { background: #FFF3E0; color: #E65100; }
    .badge-cartao { background: #E3F2FD; color: #1565C0; }
    .badge-fatura\ cartao, .badge-fatura { background: #FCE4EC; color: #C62828; }
    .badge-pix_sent, .badge-pix_received { background: #E8F5E9; color: #2E7D32; }
    .badge-card_purchase { background: #E3F2FD; color: #1565C0; }
    .badge-transfer_in, .badge-salary { background: #E8F5E9; color: #2E7D32; }

    .load-more-btn { margin-top: 12px; }
  `]
})
export class StatementPageComponent implements OnInit {
  transactions: StatementEntry[] = [];
  loading = true;
  loadingMore = false;
  hasMore = true;
  page = 1;
  pageSize = 20;
  accountId = '';
  expandedId: string | null = null;

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

    this.paymentService.getStatement(this.accountId, this.page, this.pageSize).subscribe({
      next: (res) => {
        if (this.page === 1) {
          this.transactions = res.items;
        } else {
          this.transactions = [...this.transactions, ...res.items];
        }
        this.hasMore = this.page < res.totalPages;
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

  toggleExpand(tx: StatementEntry) {
    this.expandedId = this.expandedId === tx.id ? null : tx.id;
  }

  getTxClass(tx: StatementEntry): string {
    const t = tx.type.toLowerCase();
    if (t.includes('pix')) return 'pix';
    if (t.includes('boleto')) return 'boleto';
    if (t.includes('cartao') || t.includes('card')) return 'cartao';
    if (t.includes('fatura')) return 'fatura';
    return tx.isCredit ? 'received' : 'sent';
  }

  getTxIcon(tx: StatementEntry): string {
    const t = tx.type.toLowerCase();
    if (t.includes('pix')) return 'flash_on';
    if (t.includes('boleto')) return 'receipt_long';
    if (t.includes('cartao') || t.includes('card')) return 'credit_card';
    if (t.includes('fatura')) return 'payment';
    return tx.isCredit ? 'call_received' : 'call_made';
  }

  getTxLabel(tx: StatementEntry): string {
    if (tx.description) return tx.description;
    return tx.isCredit ? 'Credito' : 'Debito';
  }

  getTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      'PIX': 'Pix',
      'Pix': 'Pix',
      'Boleto': 'Boleto',
      'Cartao': 'Cartao',
      'Fatura Cartao': 'Fatura',
      'PIX_SENT': 'Pix Enviado',
      'PIX_RECEIVED': 'Pix Recebido',
      'BOLETO': 'Boleto',
      'CARD_PURCHASE': 'Cartao',
      'TRANSFER_IN': 'Transferencia',
      'SALARY': 'Salario'
    };
    return labels[type] || type;
  }

  downloadCsv() {
    const header = 'Data,Tipo,Descricao,Contraparte,Valor\n';
    const rows = this.transactions.map(tx => {
      const date = new Date(tx.date).toLocaleDateString('pt-BR') + ' ' + new Date(tx.date).toLocaleTimeString('pt-BR');
      const amount = (tx.isCredit ? '+' : '-') + tx.amount.toFixed(2);
      const desc = (tx.description || '').replace(/,/g, ';');
      const counter = (tx.counterpartyName || '').replace(/,/g, ';');
      return `${date},${tx.type},${desc},${counter},${amount}`;
    }).join('\n');
    const csv = '\uFEFF' + header + rows;
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'extrato-krt-bank.csv';
    a.click();
    URL.revokeObjectURL(url);
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}
