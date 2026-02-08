import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AccountDto {
  id: string;
  customerName: string;
  document: string;
  email: string;
  balance: number;
  status: string;
  type: string;
}

export interface BalanceDto {
  accountId: string;
  availableAmount: number;
}

@Injectable({ providedIn: 'root' })
export class AccountService {
  private baseUrl = `${environment.apiUrl}/accounts`;

  constructor(private http: HttpClient) {}

  /** GET /api/v1/accounts/{id} */
  getById(id: string): Observable<AccountDto> {
    return this.http.get<AccountDto>(`${this.baseUrl}/${id}`);
  }

  /** GET /api/v1/accounts/by-document/{doc} */
  getByDocument(document: string): Observable<AccountDto> {
    return this.http.get<AccountDto>(`${this.baseUrl}/by-document/${document}`);
  }

  /** GET /api/v1/accounts/{id}/balance */
  getBalance(id: string): Observable<BalanceDto> {
    return this.http.get<BalanceDto>(`${this.baseUrl}/${id}/balance`);
  }

  /** POST /api/v1/accounts/{id}/credit */
  credit(id: string, amount: number, reason: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/credit`, { amount, reason });
  }

  /** POST /api/v1/accounts/{id}/debit */
  debit(id: string, amount: number, reason: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/debit`, { amount, reason });
  }
}