import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-insurance',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './insurance.component.html',
  styleUrls: ['./insurance.component.scss']
})
export class InsuranceComponent implements OnInit {
  accountId = '';
  plans: any[] = [];
  policies: any[] = [];
  activeTab: 'plans' | 'my' = 'plans';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    this.loadPlans();
    this.loadPolicies();
  }

  loadPlans(): void { this.http.get<any[]>(`${environment.apiUrl}/insurance/plans`).subscribe(d => this.plans = d); }
  loadPolicies(): void { this.http.get<any[]>(`${environment.apiUrl}/insurance/${this.accountId}/policies`).subscribe(d => this.policies = d); }

  subscribe(plan: any): void {
    this.http.post(`${environment.apiUrl}/insurance/${this.accountId}/subscribe`, {
      planId: plan.id, planName: plan.name, monthlyPrice: plan.monthlyPrice, coverage: plan.coverage
    }).subscribe({ next: () => { alert('Seguro contratado!'); this.loadPolicies(); }, error: (e) => alert(e.error?.error || 'Erro') });
  }

  cancelPolicy(id: string): void {
    if (!confirm('Cancelar seguro?')) return;
    this.http.post(`${environment.apiUrl}/insurance/${this.accountId}/cancel/${id}`, {}).subscribe(() => this.loadPolicies());
  }
}