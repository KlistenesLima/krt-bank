import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface VirtualCard {
  id: string;
  accountId: string;
  cardNumber: string;
  maskedNumber: string;
  cardholderName: string;
  expiration: string;
  cvv: string;
  cvvValid: boolean;
  cvvExpiresAt: string;
  last4Digits: string;
  brand: string;
  status: string;
  spendingLimit: number;
  spentThisMonth: number;
  remainingLimit: number;
  settings: {
    isContactless: boolean;
    isOnlinePurchase: boolean;
    isInternational: boolean;
  };
}

@Injectable({ providedIn: 'root' })
export class CardService {
  private baseUrl = `${environment.apiUrl}/cards`;

  constructor(private http: HttpClient) {}

  getCards(accountId: string): Observable<VirtualCard[]> {
    return this.http.get<VirtualCard[]>(`${this.baseUrl}/account/${accountId}`);
  }

  getCard(cardId: string): Observable<VirtualCard> {
    return this.http.get<VirtualCard>(`${this.baseUrl}/${cardId}`);
  }

  createCard(accountId: string, holderName: string, brand: string = 'Visa'): Observable<VirtualCard> {
    return this.http.post<VirtualCard>(this.baseUrl, { accountId, holderName, brand });
  }

  blockCard(cardId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${cardId}/block`, {});
  }

  unblockCard(cardId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${cardId}/unblock`, {});
  }

  cancelCard(cardId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${cardId}`);
  }

  rotateCvv(cardId: string): Observable<{ cvv: string; expiresAt: string }> {
    return this.http.post<any>(`${this.baseUrl}/${cardId}/rotate-cvv`, {});
  }

  updateSettings(cardId: string, settings: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/${cardId}/settings`, settings);
  }
}