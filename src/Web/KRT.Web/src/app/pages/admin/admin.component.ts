import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
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
  activeTab: 'overview' | 'accounts' | 'fraud' = 'overview';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadDashboard();
    this.loadPending();
    this.loadFraudAlerts();
  }

  loadDashboard(): void {
    this.http.get<any>(`${environment.apiUrl}/admin/dashboard`).subscribe(d => this.dashboard = d);
  }

  loadPending(): void {
    this.http.get<any>(`${environment.apiUrl}/admin/accounts/pending`).subscribe(d => this.pendingAccounts = d.accounts);
  }

  loadFraudAlerts(): void {
    this.http.get<any>(`${environment.apiUrl}/admin/fraud/alerts`).subscribe(d => this.fraudAlerts = d.alerts);
  }

  reviewAccount(id: string, approved: boolean): void {
    this.http.post(`${environment.apiUrl}/admin/accounts/${id}/review`, { approved, notes: approved ? 'Aprovado pelo admin' : 'Documentacao insuficiente' })
      .subscribe(() => this.loadPending());
  }

  fraudAction(id: string, action: string): void {
    this.http.post(`${environment.apiUrl}/admin/fraud/alerts/${id}/action`, { action }).subscribe(() => this.loadFraudAlerts());
  }

  getSeverityClass(s: string): string { return { 'critical': 'sev-critical', 'high': 'sev-high', 'medium': 'sev-medium', 'low': 'sev-low' }[s] || ''; }
}