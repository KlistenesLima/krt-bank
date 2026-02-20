import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-marketplace',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './marketplace.component.html',
  styleUrls: ['./marketplace.component.scss']
})
export class MarketplaceComponent implements OnInit {
  accountId = '';
  offers: any[] = [];
  points: any = null;
  history: any[] = [];
  activeTab: 'offers' | 'points' = 'offers';
  filterCategory = '';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    this.loadOffers(); this.loadPoints(); this.loadHistory();
  }

  loadOffers(): void { this.http.get<any[]>(`${environment.apiUrl}/marketplace/offers`).subscribe(d => this.offers = d); }
  loadPoints(): void { this.http.get<any>(`${environment.apiUrl}/marketplace/${this.accountId}/points`).subscribe(d => this.points = d); }
  loadHistory(): void { this.http.get<any[]>(`${environment.apiUrl}/marketplace/${this.accountId}/history`).subscribe(d => this.history = d); }

  redeemOffer(offer: any): void {
    const cost = offer.type === 'pontos' ? parseInt(offer.value) || 0 : 0;
    this.http.post<any>(`${environment.apiUrl}/marketplace/${this.accountId}/redeem`, {
      offerId: offer.id, offerName: offer.partner + ' - ' + offer.value, pointsCost: cost
    }).subscribe({ next: (data) => { alert(`${data.message}\nCodigo: ${data.code}`); this.loadPoints(); }, error: (e) => alert(e.error?.error || 'Erro') });
  }

  get filteredOffers(): any[] {
    return this.filterCategory ? this.offers.filter(o => o.category === this.filterCategory) : this.offers;
  }

  get categories(): string[] { return [...new Set(this.offers.map((o: any) => o.category))]; }
}