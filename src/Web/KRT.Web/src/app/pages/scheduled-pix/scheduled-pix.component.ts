import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

interface ScheduledItem {
  id: string; destinationName: string; pixKey: string; amount: number;
  scheduledDate: string; frequency: string; frequencyCode: string;
  isRecurring: boolean; status: string; statusCode: string;
  nextExecutionDate: string; executionCount: number; maxExecutions: number;
}

@Component({
  selector: 'app-scheduled-pix',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './scheduled-pix.component.html',
  styleUrls: ['./scheduled-pix.component.scss']
})
export class ScheduledPixComponent implements OnInit {
  accountId = '';
  scheduledList: ScheduledItem[] = [];
  loading = false;
  showForm = false;
  statusFilter = '';

  // Form
  form = {
    destinationAccountId: '00000000-0000-0000-0000-000000000002',
    pixKey: '',
    destinationName: '',
    amount: 0,
    description: '',
    scheduledDate: '',
    frequency: 'Once',
    maxExecutions: 12
  };

  frequencies = [
    { value: 'Once', label: 'Unico' },
    { value: 'Weekly', label: 'Semanal' },
    { value: 'BiWeekly', label: 'Quinzenal' },
    { value: 'Monthly', label: 'Mensal' },
    { value: 'Yearly', label: 'Anual' }
  ];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.form.scheduledDate = tomorrow.toISOString().slice(0, 16);
    this.loadScheduled();
  }

  loadScheduled(): void {
    this.loading = true;
    let url = `${environment.apiUrl}/pix/scheduled/account/${this.accountId}`;
    if (this.statusFilter) url += `?status=${this.statusFilter}`;
    this.http.get<ScheduledItem[]>(url).subscribe({
      next: (data) => { this.scheduledList = data; this.loading = false; },
      error: () => this.loading = false
    });
  }

  createScheduled(): void {
    const body = {
      accountId: this.accountId,
      ...this.form,
      maxExecutions: this.form.frequency !== 'Once' ? this.form.maxExecutions : null
    };
    this.http.post(`${environment.apiUrl}/pix/scheduled`, body).subscribe({
      next: () => { this.showForm = false; this.loadScheduled(); },
      error: (err) => alert(err.error?.error || 'Erro ao agendar')
    });
  }

  execute(id: string): void {
    this.http.post(`${environment.apiUrl}/pix/scheduled/${id}/execute`, {}).subscribe({
      next: () => this.loadScheduled(),
      error: (err) => alert(err.error?.error || 'Erro')
    });
  }

  cancel(id: string): void {
    if (!confirm('Cancelar este agendamento?')) return;
    this.http.post(`${environment.apiUrl}/pix/scheduled/${id}/cancel`, {}).subscribe(() => this.loadScheduled());
  }

  pause(id: string): void {
    this.http.post(`${environment.apiUrl}/pix/scheduled/${id}/pause`, {}).subscribe(() => this.loadScheduled());
  }

  resume(id: string): void {
    this.http.post(`${environment.apiUrl}/pix/scheduled/${id}/resume`, {}).subscribe(() => this.loadScheduled());
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Pending': 'pending', 'Agendado': 'pending',
      'Executed': 'executed', 'Executado': 'executed',
      'Failed': 'failed', 'Falhou': 'failed',
      'Cancelled': 'cancelled', 'Cancelado': 'cancelled',
      'Paused': 'paused', 'Pausado': 'paused'
    };
    return map[status] || '';
  }
}