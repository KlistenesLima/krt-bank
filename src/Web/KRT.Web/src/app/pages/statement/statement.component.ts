import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-statement',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './statement.component.html',
  styleUrls: ['./statement.component.scss']
})
export class StatementComponent implements OnInit {
  accountId = '';
  transactions: any[] = [];
  loading = false;
  summary: any = null;

  // Filtros
  startDate = '';
  endDate = '';
  typeFilter = '';
  categoryFilter = '';
  minAmount: number | null = null;
  maxAmount: number | null = null;
  searchTerm = '';

  // Paginacao
  page = 1;
  pageSize = 20;
  totalItems = 0;
  totalPages = 0;

  // Ordenacao
  sortBy = 'date';
  sortOrder = 'desc';

  types = ['PIX_SENT', 'PIX_RECEIVED', 'TED_SENT', 'TED_RECEIVED', 'BOLETO', 'CARD_PURCHASE', 'REFUND'];
  categories = ['Alimentacao', 'Transporte', 'Moradia', 'Lazer', 'Saude', 'Educacao', 'Servicos', 'Outros'];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    this.loadStatement();
  }

  loadStatement(): void {
    this.loading = true;
    let params = new HttpParams()
      .set('page', this.page.toString())
      .set('pageSize', this.pageSize.toString())
      .set('sortBy', this.sortBy)
      .set('sortOrder', this.sortOrder);

    if (this.startDate) params = params.set('startDate', this.startDate);
    if (this.endDate) params = params.set('endDate', this.endDate);
    if (this.typeFilter) params = params.set('type', this.typeFilter);
    if (this.categoryFilter) params = params.set('category', this.categoryFilter);
    if (this.minAmount) params = params.set('minAmount', this.minAmount.toString());
    if (this.maxAmount) params = params.set('maxAmount', this.maxAmount.toString());
    if (this.searchTerm) params = params.set('search', this.searchTerm);

    this.http.get<any>(`${environment.apiUrl}/statement/${this.accountId}`, { params })
      .subscribe({
        next: (data) => {
          this.transactions = data.items;
          this.totalItems = data.totalItems;
          this.totalPages = data.totalPages;
          this.summary = data.summary;
          this.loading = false;
        },
        error: () => this.loading = false
      });
  }

  applyFilters(): void {
    this.page = 1;
    this.loadStatement();
  }

  clearFilters(): void {
    this.startDate = '';
    this.endDate = '';
    this.typeFilter = '';
    this.categoryFilter = '';
    this.minAmount = null;
    this.maxAmount = null;
    this.searchTerm = '';
    this.page = 1;
    this.loadStatement();
  }

  sort(column: string): void {
    if (this.sortBy === column) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortOrder = 'desc';
    }
    this.loadStatement();
  }

  goToPage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.page = p;
      this.loadStatement();
    }
  }

  getPages(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.page - 2);
    const end = Math.min(this.totalPages, this.page + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  exportCsv(): void {
    let params = new HttpParams();
    if (this.startDate) params = params.set('startDate', this.startDate);
    if (this.endDate) params = params.set('endDate', this.endDate);
    if (this.typeFilter) params = params.set('type', this.typeFilter);
    if (this.categoryFilter) params = params.set('category', this.categoryFilter);

    this.http.get(`${environment.apiUrl}/statement/${this.accountId}/export/csv`, { params, responseType: 'blob' })
      .subscribe(blob => this.downloadFile(blob, 'extrato.csv'));
  }

  exportPdf(): void {
    let params = new HttpParams();
    if (this.startDate) params = params.set('startDate', this.startDate);
    if (this.endDate) params = params.set('endDate', this.endDate);
    if (this.typeFilter) params = params.set('type', this.typeFilter);
    if (this.categoryFilter) params = params.set('category', this.categoryFilter);

    this.http.get(`${environment.apiUrl}/statement/${this.accountId}/export/pdf`, { params, responseType: 'blob' })
      .subscribe(blob => this.downloadFile(blob, 'extrato.pdf'));
  }

  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  getTypeLabel(type: string): string {
    const map: Record<string, string> = {
      'PIX_SENT': 'Pix Enviado', 'PIX_RECEIVED': 'Pix Recebido',
      'TED_SENT': 'TED Enviada', 'TED_RECEIVED': 'TED Recebida',
      'BOLETO': 'Boleto', 'CARD_PURCHASE': 'Cartao', 'REFUND': 'Estorno'
    };
    return map[type] || type;
  }

  getTypeIcon(type: string): string {
    const map: Record<string, string> = {
      'PIX_SENT': '⚡↑', 'PIX_RECEIVED': '⚡↓',
      'TED_SENT': '🏦↑', 'TED_RECEIVED': '🏦↓',
      'BOLETO': '📄', 'CARD_PURCHASE': '💳', 'REFUND': '↩️'
    };
    return map[type] || '💰';
  }
}