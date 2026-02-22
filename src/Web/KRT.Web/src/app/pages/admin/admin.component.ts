import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { environment } from '../../../environments/environment';
import { AccountService, AccountAdminDto, AccountStats } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit, OnDestroy {
  dashboard: any = null;
  pendingAccounts: any[] = [];
  fraudAlerts: any[] = [];
  metrics: any[] = [];
  systemServices: any[] = [];
  systemMetrics: any = null;
  activeSection = 'dashboard';
  loading = true;
  loadError = false;
  currentTime = '';
  activityFeed: any[] = [];
  sidebarCollapsed = false;
  transactionsData: any[] = [];
  transactionsKpis: any = null;
  transactionsPagination: any = null;
  txFilterType = '';
  txPage = 1;
  private charts: Chart[] = [];
  private intervals: any[] = [];

  // === ACCOUNTS MANAGEMENT ===
  allAccounts: AccountAdminDto[] = [];
  filteredAccounts: AccountAdminDto[] = [];
  accountStats: AccountStats = { total: 0, active: 0, inactive: 0, blocked: 0, pending: 0, suspended: 0, closed: 0 };
  accountsTab = 'all';
  accountSearch = '';
  accountsLoading = false;
  // Action dialog
  accountAction: { type: string; account: AccountAdminDto | null; selectedRole: string } = { type: '', account: null, selectedRole: 'Cliente' };

  // Role-based visibility
  isAdmin = false;

  constructor(private http: HttpClient, public router: Router, private accountService: AccountService, private auth: AuthService) {}

  ngOnInit(): void {
    this.isAdmin = this.auth.isAdmin();
    this.loadAll();
    this.updateTime();
    this.intervals.push(setInterval(() => this.updateTime(), 1000));
    this.intervals.push(setInterval(() => this.loadActivity(), 30000));
  }

  ngOnDestroy(): void {
    this.destroyCharts();
    this.intervals.forEach(i => clearInterval(i));
  }

  loadAll(): void {
    this.loading = true;
    this.loadError = false;
    this.http.get<any>(`${environment.apiUrl}/admin/dashboard`).subscribe({
      next: d => {
        this.dashboard = d;
        this.loading = false;
        this.loadError = false;
        setTimeout(() => this.renderCharts(), 150);
      },
      error: () => { this.loading = false; this.loadError = true; }
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
    this.http.get<any>(`${environment.apiUrl}/admin/system`).subscribe({
      next: d => {
        this.systemServices = d.services || [];
        this.systemMetrics = d.metrics || null;
      },
      error: () => {}
    });
    this.loadActivity();
    this.loadTransactions();
  }

  loadActivity(): void {
    this.http.get<any>(`${environment.apiUrl}/admin/activity`).subscribe({
      next: d => {
        this.activityFeed = (d.activities || []).map((a: any) => ({
          ...a,
          text: a.description,
          timeFormatted: new Date(a.time).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
        }));
      },
      error: () => {}
    });
  }

  loadTransactions(): void {
    const params: any = { page: this.txPage, pageSize: 20 };
    if (this.txFilterType) params.type = this.txFilterType;
    this.http.get<any>(`${environment.apiUrl}/admin/transactions`, { params }).subscribe({
      next: d => {
        this.transactionsData = d.transactions || [];
        this.transactionsKpis = d.kpis || null;
        this.transactionsPagination = d.pagination || null;
      },
      error: () => {}
    });
  }

  filterTx(type: string): void {
    this.txFilterType = this.txFilterType === type ? '' : type;
    this.txPage = 1;
    this.loadTransactions();
  }

  changeTxPage(delta: number): void {
    if (!this.transactionsPagination) return;
    const newPage = this.txPage + delta;
    if (newPage < 1 || newPage > this.transactionsPagination.totalPages) return;
    this.txPage = newPage;
    this.loadTransactions();
  }

  setSection(section: string): void {
    this.activeSection = section;
    if (section === 'accounts') this.loadAccounts();
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
          label: 'Transações',
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
    const breakdown = rev.revenueBreakdown || [];
    const labels = breakdown.map((b: any) => b.label);
    const data = breakdown.map((b: any) => b.value);

    this.charts.push(new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels,
        datasets: [{
          data,
          backgroundColor: ['#0047BB', '#7c3aed', '#f59e0b'],
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

  formatTxDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' }) + ' ' +
           d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
  }

  formatCurrency(val: number): string {
    return Math.abs(val).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
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

  // ==================== ACCOUNTS MANAGEMENT ====================

  loadAccounts(): void {
    this.accountsLoading = true;
    this.accountService.getAll().subscribe({
      next: data => { this.allAccounts = data; this.applyAccountFilters(); this.accountsLoading = false; },
      error: () => { this.accountsLoading = false; }
    });
    this.accountService.getStats().subscribe({
      next: stats => this.accountStats = stats,
      error: () => {}
    });
  }

  setAccountsTab(tab: string): void {
    this.accountsTab = tab;
    this.applyAccountFilters();
  }

  applyAccountFilters(): void {
    let list = [...this.allAccounts];
    if (this.accountsTab === 'active') list = list.filter(a => a.status === 'Active');
    else if (this.accountsTab === 'inactive') list = list.filter(a => a.status === 'Inactive');
    else if (this.accountsTab === 'pending') list = list.filter(a => a.status === 'Pending');
    else if (this.accountsTab === 'blocked') list = list.filter(a => a.status === 'Blocked');

    if (this.accountSearch.trim()) {
      const q = this.accountSearch.toLowerCase().trim();
      list = list.filter(a =>
        a.customerName.toLowerCase().includes(q) ||
        a.email.toLowerCase().includes(q) ||
        a.document.includes(q)
      );
    }
    this.filteredAccounts = list;
  }

  onAccountSearch(): void {
    this.applyAccountFilters();
  }

  getStatusLabel(s: string): string {
    const map: any = { Active: 'Ativa', Inactive: 'Inativa', Pending: 'Pendente', Blocked: 'Bloqueada', Closed: 'Encerrada', Suspended: 'Suspensa' };
    return map[s] || s;
  }

  getStatusClass(s: string): string {
    const map: any = { Active: 'status-active', Inactive: 'status-inactive', Pending: 'status-pending', Blocked: 'status-blocked', Closed: 'status-closed', Suspended: 'status-suspended' };
    return map[s] || '';
  }

  getRoleLabel(r: string): string {
    const map: any = { User: 'Cliente', Admin: 'Administrador', Administrador: 'Administrador', Operador: 'Operador', Cliente: 'Cliente' };
    return map[r] || r;
  }

  getRoleClass(r: string): string {
    if (r === 'Admin' || r === 'Administrador') return 'role-admin';
    if (r === 'Operador') return 'role-operador';
    return 'role-cliente';
  }

  formatCpf(doc: string): string {
    if (!doc || doc.length !== 11) return doc;
    return doc.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  // Actions
  openAccountAction(type: string, account: AccountAdminDto): void {
    this.accountAction = { type, account, selectedRole: account.role === 'User' ? 'Cliente' : account.role };
  }

  closeAccountAction(): void {
    this.accountAction = { type: '', account: null, selectedRole: 'Cliente' };
  }

  confirmAccountAction(): void {
    const { type, account, selectedRole } = this.accountAction;
    if (!account) return;

    if (type === 'activate') {
      this.accountService.changeStatus(account.id, true).subscribe({
        next: () => { this.loadAccounts(); this.closeAccountAction(); },
        error: (e) => alert('Erro: ' + (e.error?.error || 'Falha ao ativar'))
      });
    } else if (type === 'deactivate') {
      this.accountService.changeStatus(account.id, false).subscribe({
        next: () => { this.loadAccounts(); this.closeAccountAction(); },
        error: (e) => alert('Erro: ' + (e.error?.error || 'Falha ao inativar'))
      });
    } else if (type === 'role') {
      this.accountService.changeRole(account.id, selectedRole).subscribe({
        next: () => { this.loadAccounts(); this.closeAccountAction(); },
        error: (e) => alert('Erro: ' + (e.error?.error || 'Falha ao mudar role'))
      });
    } else if (type === 'approve') {
      this.http.post(`${environment.apiUrl}/admin/accounts/${account.id}/review`, {
        approved: true, notes: 'Aprovado pelo admin'
      }).subscribe({ next: () => { this.loadAccounts(); this.closeAccountAction(); } });
    } else if (type === 'reject') {
      this.http.post(`${environment.apiUrl}/admin/accounts/${account.id}/review`, {
        approved: false, notes: 'Documentação insuficiente'
      }).subscribe({ next: () => { this.loadAccounts(); this.closeAccountAction(); } });
    }
  }

  reviewAccount(id: string, approved: boolean): void {
    this.http.post(`${environment.apiUrl}/admin/accounts/${id}/review`, {
      approved, notes: approved ? 'Aprovado pelo admin' : 'Documentação insuficiente'
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
    return ({ critical: 'CRÍTICO', high: 'ALTO', medium: 'MÉDIO', low: 'BAIXO' } as any)[s] || s;
  }

  getTotalVolume(): string {
    if (!this.dashboard) return '0';
    const v = this.dashboard.transactions.totalVolume;
    if (v >= 1000000) return (v / 1000000).toFixed(1) + 'M';
    if (v >= 1000) return (v / 1000).toFixed(0) + 'K';
    return v.toFixed(0);
  }

  getServiceStatusClass(status: string): string {
    if (status === 'healthy') return 'online';
    if (status === 'degraded') return 'degraded';
    return 'offline';
  }

  getServiceStatusLabel(status: string): string {
    if (status === 'healthy') return 'Online';
    if (status === 'degraded') return 'Degraded';
    return 'Offline';
  }

  logout(): void {
    this.router.navigate(['/dashboard']);
  }
}
