import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-boletos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './boletos.component.html',
  styleUrls: ['./boletos.component.scss']
})
export class BoletosComponent implements OnInit {
  accountId = '';
  boletos: any[] = [];
  loading = false;
  activeTab: 'list' | 'generate' | 'pay' = 'list';

  genForm = { beneficiaryName: '', beneficiaryCnpj: '', amount: 0, dueDate: '', description: '' };
  payForm = { barcode: '', amount: 0, beneficiaryName: '' };
  generatedBoleto: any = null;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    const nextMonth = new Date(); nextMonth.setMonth(nextMonth.getMonth() + 1);
    this.genForm.dueDate = nextMonth.toISOString().split('T')[0];
    this.loadBoletos();
  }

  loadBoletos(): void {
    this.loading = true;
    this.http.get<any[]>(`${environment.apiUrl}/boletos/account/${this.accountId}`).subscribe({
      next: (data) => { this.boletos = data; this.loading = false; },
      error: () => this.loading = false
    });
  }

  generate(): void {
    const body = { accountId: this.accountId, ...this.genForm };
    this.http.post<any>(`${environment.apiUrl}/boletos/generate`, body).subscribe({
      next: (data) => { this.generatedBoleto = data; this.loadBoletos(); },
      error: (err) => alert(err.error?.error || 'Erro')
    });
  }

  payByBarcode(): void {
    const body = { accountId: this.accountId, ...this.payForm };
    this.http.post(`${environment.apiUrl}/boletos/pay-barcode`, body).subscribe({
      next: () => { alert('Boleto pago!'); this.activeTab = 'list'; this.loadBoletos(); },
      error: (err) => alert(err.error?.error || 'Erro')
    });
  }

  payBoleto(id: string): void {
    this.http.post(`${environment.apiUrl}/boletos/pay/${id}`, {}).subscribe(() => this.loadBoletos());
  }

  cancelBoleto(id: string): void {
    if (!confirm('Cancelar boleto?')) return;
    this.http.post(`${environment.apiUrl}/boletos/cancel/${id}`, {}).subscribe(() => this.loadBoletos());
  }

  copyBarcode(code: string): void {
    navigator.clipboard.writeText(code);
  }

  getStatusClass(status: string): string {
    return { 'Pendente': 'pending', 'Pago': 'paid', 'Vencido': 'overdue', 'Cancelado': 'cancelled' }[status] || '';
  }
}