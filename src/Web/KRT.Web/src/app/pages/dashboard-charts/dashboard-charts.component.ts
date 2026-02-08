import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard-charts',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-charts.component.html',
  styleUrls: ['./dashboard-charts.component.scss']
})
export class DashboardChartsComponent implements OnInit, AfterViewInit {
  @ViewChild('balanceChart') balanceCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('categoriesChart') categoriesCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('monthlyChart') monthlyCanvas!: ElementRef<HTMLCanvasElement>;

  summary: any = null;
  accountId = '';
  loading = true;

  private balanceChart: Chart | null = null;
  private categoriesChart: Chart | null = null;
  private monthlyChart: Chart | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    this.loadSummary();
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.loadBalanceHistory();
      this.loadCategories();
      this.loadMonthlySummary();
    }, 300);
  }

  loadSummary(): void {
    this.http.get<any>(`${environment.apiUrl}/dashboard/summary/${this.accountId}`)
      .subscribe({
        next: (data) => { this.summary = data; this.loading = false; },
        error: () => this.loading = false
      });
  }

  loadBalanceHistory(): void {
    this.http.get<any>(`${environment.apiUrl}/dashboard/balance-history/${this.accountId}?days=30`)
      .subscribe(data => {
        const labels = data.history.map((h: any) => h.date.substring(5));
        const values = data.history.map((h: any) => h.balance);

        this.balanceChart = new Chart(this.balanceCanvas.nativeElement, {
          type: 'line',
          data: {
            labels,
            datasets: [{
              label: 'Saldo (R$)',
              data: values,
              borderColor: '#1a237e',
              backgroundColor: 'rgba(26,35,126,0.1)',
              fill: true,
              tension: 0.4,
              pointRadius: 1,
              pointHoverRadius: 5
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
              y: { beginAtZero: false, ticks: { callback: (v) => 'R$ ' + Number(v).toLocaleString('pt-BR') } },
              x: { ticks: { maxTicksLimit: 10 } }
            }
          }
        });
      });
  }

  loadCategories(): void {
    this.http.get<any>(`${environment.apiUrl}/dashboard/spending-categories/${this.accountId}`)
      .subscribe(data => {
        this.categoriesChart = new Chart(this.categoriesCanvas.nativeElement, {
          type: 'doughnut',
          data: {
            labels: data.categories.map((c: any) => c.category),
            datasets: [{
              data: data.categories.map((c: any) => c.amount),
              backgroundColor: data.categories.map((c: any) => c.color),
              borderWidth: 2,
              borderColor: '#fff'
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: { position: 'bottom', labels: { padding: 16, usePointStyle: true } }
            },
            cutout: '65%'
          }
        });
      });
  }

  loadMonthlySummary(): void {
    this.http.get<any>(`${environment.apiUrl}/dashboard/monthly-summary/${this.accountId}`)
      .subscribe(data => {
        this.monthlyChart = new Chart(this.monthlyCanvas.nativeElement, {
          type: 'bar',
          data: {
            labels: data.months.map((m: any) => m.month),
            datasets: [
              {
                label: 'Entradas',
                data: data.months.map((m: any) => m.income),
                backgroundColor: 'rgba(46,125,50,0.7)',
                borderRadius: 6
              },
              {
                label: 'Saidas',
                data: data.months.map((m: any) => m.expenses),
                backgroundColor: 'rgba(211,47,47,0.7)',
                borderRadius: 6
              }
            ]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'top' } },
            scales: {
              y: { beginAtZero: true, ticks: { callback: (v) => 'R$ ' + Number(v).toLocaleString('pt-BR') } }
            }
          }
        });
      });
  }
}