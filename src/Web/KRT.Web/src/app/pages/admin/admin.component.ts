import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit {
  dashboard: any = null;
  pendingAccounts: any[] = [];
  fraudAlerts: any[] = [];
  metrics: any[] = [];
  activeSection = 'dashboard';
  loading = true;
  currentTime = '';
  activityFeed: any[] = [];
  sidebarCollapsed = false;

  constructor(private http: HttpClient, public router: Router) {}

  ngOnInit(): void {
    this.loadAll();
    this.updateTime();
    setInterval(() => this.updateTime(), 1000);
    // Simular feed de atividade
    setInterval(() => this.addActivity(), 4000);
  }

  loadAll(): void {
    this.loading = true;
    this.http.get<any>(`${environment.apiUrl}/admin/dashboard`).subscribe(d => {
      this.dashboard = d;
      this.loading = false;
    });
    this.http.get<any>(`${environment.apiUrl}/admin/accounts/pending`).subscribe(d => this.pendingAccounts = d.accounts || []);
    this.http.get<any>(`${environment.apiUrl}/admin/fraud/alerts`).subscribe(d => this.fraudAlerts = d.alerts || []);
    this.http.get<any>(`${environment.apiUrl}/admin/metrics?days=14`).subscribe(d => this.metrics = d.daily || []);
  }

  updateTime(): void {
    const now = new Date();
    this.currentTime = now.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
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
    return { critical: 'sev-critical', high: 'sev-high', medium: 'sev-medium', low: 'sev-low' }[s] || '';
  }

  getSeverityLabel(s: string): string {
    return { critical: 'CRITICO', high: 'ALTO', medium: 'MEDIO', low: 'BAIXO' }[s] || s;
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

