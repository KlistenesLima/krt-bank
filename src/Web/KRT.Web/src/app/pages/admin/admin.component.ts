import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit, OnDestroy {
  dashboard: any = null;
  pendingAccounts: any[] = [];
  fraudAlerts: any[] = [];
  metrics: any[] = [];
  activeSection = 'dashboard';
  loading = true;
  currentTime = '';
  activityFeed: any[] = [];
  sidebarCollapsed = false;
  private charts: Chart[] = [];
  private intervals: any[] = [];

  constructor(private http: HttpClient, public router: Router) {}

  ngOnInit(): void {
    this.loadAll();
    this.updateTime();
    this.intervals.push(setInterval(() => this.updateTime(), 1000));
    this.intervals.push(setInterval(() => this.addActivity(), 4000));
  }

  ngOnDestroy(): void {
    this.destroyCharts();
    this.intervals.forEach(i => clearInterval(i));
  }

  loadAll(): void {
    this.loading = true;
    this.http.get<any>(`${environment.apiUrl}/admin/dashboard`).subscribe({
      next: d => {
        this.dashboard = d;
        this.loading = false;
        setTimeout(() => this.renderCharts(), 150);
      },
      error: () => this.loading = false
    });
    this.http.get<any>(`${environment.apiUrl}/admin/accounts/pending`).subscribe({
      next: d => this.pendingAccounts = d.accounts || [],
      error: () => {}
    });
    this.http.get<any>(`${environment.apiUrl}/admin/fraud/alerts`).subscribe({
      next: d => this.fraudAlerts = d.alerts || [],
      error: () => {}
    });
    this.http.get<any>(`${environment.apiUrl}/admin/metrics?days=14`).subscribe({
      next: d => {
        this.metrics = d.daily || [];
        setTimeout(() => this.renderCharts(), 200);
      },
      error: () => {}
    });
  }

  setSection(section: string): void {
    this.activeSection = section;
    setTimeout(() => this.renderCharts(), 150);
  }

  // ==================== CHARTS ====================
  destroyCharts(): void {
    this.charts.forEach(c => c.destroy());
    this.charts = [];
  }

  renderCharts(): void {
    this.destroyCharts();
    if (this.activeSection === 'dashboard' && this.dashboard) {
      this.createTransactionsLineChart();
      this.createRevenueDonutChart();
    }
    if (this.activeSection === 'transactions' && this.metrics.length) {
      this.createVolumeBarChart();
    }
  }

  createTransactionsLineChart(): void {
    const canvas = document.getElementById('chartTransactions') as HTMLCanvasElement;
    if (!canvas || !this.metrics.length) return;
    const ctx = canvas.getContext('2d')!;
    const gradient = ctx.createLinearGradient(0, 0, 0, 280);
    gradient.addColorStop(0, 'rgba(59, 130, 246, 0.35)');
    gradient.addColorStop(1, 'rgba(59, 130, 246, 0.0)');

    this.charts.push(new Chart(canvas, {
      type: 'line',
      data: {
        labels: this.metrics.map(m => this.getDayLabel(m.date)),
        datasets: [{
          label: 'Transacoes',
          data: this.metrics.map(m => m.transactions),
          borderColor: '#3b82f6',
          backgroundColor: gradient,
          borderWidth: 2.5,
          fill: true,
          tension: 0.4,
          pointBackgroundColor: '#3b82f6',
          pointBorderColor: '#1e293b',
          pointBorderWidth: 2,
          pointRadius: 4,
          pointHoverRadius: 7
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#1e293b',
            titleColor: '#f1f5f9',
            bodyColor: '#94a3b8',
            borderColor: '#334155',
            borderWidth: 1,
            cornerRadius: 8,
            padding: 12
          }
        },
        scales: {
          x: {
            grid: { color: 'rgba(255,255,255,0.04)' },
            ticks: { color: 'rgba(255,255,255,0.4)', font: { size: 10 } }
          },
          y: {
            grid: { color: 'rgba(255,255,255,0.06)' },
            ticks: { color: 'rgba(255,255,255,0.4)', font: { size: 10 } }
          }
        }
      }
    }));
  }

  createRevenueDonutChart(): void {
    const canvas = document.getElementById('chartRevenue') as HTMLCanvasElement;
    if (!canvas || !this.dashboard?.revenue) return;
    const rev = this.dashboard.revenue;

    this.charts.push(new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels: ['PIX', 'Cartao', 'Seguros', 'Emprestimos'],
        datasets: [{
          data: [rev.pixFees, rev.cardFees, rev.insurance, rev.loanInterest],
          backgroundColor: ['#3b82f6', '#8b5cf6', '#10b981', '#f59e0b'],
          borderColor: '#0f172a',
          borderWidth: 3,
          hoverOffset: 8
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '68%',
        plugins: {
          legend: {
            position: 'bottom',
            labels: {
              color: 'rgba(255,255,255,0.7)',
              padding: 16,
              usePointStyle: true,
              pointStyleWidth: 10,
              font: { size: 11 }
            }
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleColor: '#f1f5f9',
            bodyColor: '#94a3b8',
            borderColor: '#334155',
            borderWidth: 1,
            cornerRadius: 8,
            callbacks: {
              label: (ctx: any) => ` R$ ${ctx.parsed.toLocaleString('pt-BR')}`
            }
          }
        }
      }
    }));
  }

  createVolumeBarChart(): void {
    const canvas = document.getElementById('chartVolume') as HTMLCanvasElement;
    if (!canvas || !this.metrics.length) return;
    const ctx = canvas.getContext('2d')!;
    const gradient = ctx.createLinearGradient(0, 0, 0, 350);
    gradient.addColorStop(0, '#8b5cf6');
    gradient.addColorStop(1, 'rgba(139, 92, 246, 0.15)');

    this.charts.push(new Chart(canvas, {
      type: 'bar',
      data: {
        labels: this.metrics.map(m => this.getDayLabel(m.date)),
        datasets: [{
          label: 'Volume (R$)',
          data: this.metrics.map(m => m.volume),
          backgroundColor: gradient,
          borderColor: '#8b5cf6',
          borderWidth: 1,
          borderRadius: 6,
          borderSkipped: false
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#1e293b',
            titleColor: '#f1f5f9',
            bodyColor: '#94a3b8',
            borderColor: '#334155',
            borderWidth: 1,
            cornerRadius: 8,
            callbacks: {
              label: (ctx: any) => ` R$ ${(ctx.parsed.y / 1000).toFixed(0)}K`
            }
          }
        },
        scales: {
          x: {
            grid: { color: 'rgba(255,255,255,0.04)' },
            ticks: { color: 'rgba(255,255,255,0.4)', font: { size: 10 } }
          },
          y: {
            grid: { color: 'rgba(255,255,255,0.06)' },
            ticks: {
              color: 'rgba(255,255,255,0.4)',
              font: { size: 10 },
              callback: (val: any) => 'R$' + (val / 1000) + 'K'
            }
          }
        }
      }
    }));
  }

  // ==================== HELPERS ====================
  updateTime(): void {
    this.currentTime = new Date().toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  }

  addActivity(): void {
    const types = [
      { icon: 'north_east', text: 'PIX enviado R$ {amount}', color: '#e53935' },
      { icon: 'south_west', text: 'PIX recebido R$ {amount}', color: '#00c853' },
      { icon: 'person_add', text: 'Nova conta criada', color: '#2196f3' },
      { icon: 'shield', text: 'Fraude detectada - Score {score}', color: '#ff9800' },
      { icon: 'check_circle', text: 'Transacao aprovada', color: '#4caf50' },
      { icon: 'block', text: 'Transacao bloqueada', color: '#e53935' },
    ];
    const t = types[Math.floor(Math.random() * types.length)];
    const amount = (Math.random() * 5000 + 50).toFixed(2);
    const score = Math.floor(Math.random() * 100);
    this.activityFeed.unshift({
      icon: t.icon,
      text: t.text.replace('{amount}', amount).replace('{score}', score.toString()),
      color: t.color,
      time: new Date().toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
    });
    if (this.activityFeed.length > 20) this.activityFeed.pop();
  }

  getBarHeight(val: number): number {
    if (!this.metrics.length) return 0;
    const max = Math.max(...this.metrics.map((m: any) => m.transactions));
    return max > 0 ? (val / max) * 100 : 0;
  }

  getVolumeBarHeight(val: number): number {
    if (!this.metrics.length) return 0;
    const max = Math.max(...this.metrics.map((m: any) => m.volume));
    return max > 0 ? (val / max) * 100 : 0;
  }

  getDayLabel(dateStr: string): string {
    const d = new Date(dateStr + 'T00:00:00');
    return d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
  }

  reviewAccount(id: string, approved: boolean): void {
    this.http.post(`${environment.apiUrl}/admin/accounts/${id}/review`, {
      approved, notes: approved ? 'Aprovado pelo admin' : 'Documentacao insuficiente'
    }).subscribe(() => {
      this.pendingAccounts = this.pendingAccounts.filter(a => a.id !== id);
    });
  }

  fraudAction(id: string, action: string): void {
    this.http.post(`${environment.apiUrl}/admin/fraud/alerts/${id}/action`, { action })
      .subscribe(() => {
        const alert = this.fraudAlerts.find(a => a.id === id);
        if (alert) {
          alert.status = action === 'block' ? 'Bloqueado' : action === 'approve' ? 'Liberado' : 'Investigando';
        }
      });
  }

  get pendingFraudCount(): number { return this.fraudAlerts.filter(a => a.status === 'Pendente').length; }
  get highSeverityCount(): number { return this.fraudAlerts.filter(a => a.severity === 'critical' || a.severity === 'high').length; }

  getSeverityClass(s: string): string {
    return ({ critical: 'sev-critical', high: 'sev-high', medium: 'sev-medium', low: 'sev-low' } as any)[s] || '';
  }

  getSeverityLabel(s: string): string {
    return ({ critical: 'CRITICO', high: 'ALTO', medium: 'MEDIO', low: 'BAIXO' } as any)[s] || s;
  }

  getTotalVolume(): string {
    if (!this.dashboard) return '0';
    const v = this.dashboard.transactions.totalVolume;
    if (v >= 1000000) return (v / 1000000).toFixed(1) + 'M';
    if (v >= 1000) return (v / 1000).toFixed(0) + 'K';
    return v.toFixed(0);
  }

  logout(): void {
    this.router.navigate(['/dashboard']);
  }
}
