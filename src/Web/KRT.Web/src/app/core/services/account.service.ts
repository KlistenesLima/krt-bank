import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, AccountResponse, BalanceResponse, CreateAccountRequest } from '../models/account.model';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private baseUrl = `${environment.apiUrl}/accounts`;

  constructor(private http: HttpClient) {}

  /** POST /api/v1/accounts — Criar conta */
  create(data: CreateAccountRequest): Observable<ApiResponse<{ id: string }>> {
    return this.http.post<ApiResponse<{ id: string }>>(this.baseUrl, data);
  }

  /** GET /api/v1/accounts/{id} — Dados da conta */
  getById(id: string): Observable<AccountResponse> {
    return this.http.get<ApiResponse<AccountResponse>>(`${this.baseUrl}/${id}`)
      .pipe(map(res => res.data));
  }

  /** GET /api/v1/accounts/{id}/balance — Saldo */
  getBalance(id: string): Observable<BalanceResponse> {
    return this.http.get<ApiResponse<BalanceResponse>>(`${this.baseUrl}/${id}/balance`)
      .pipe(map(res => res.data));
  }

  /** GET /api/v1/accounts — Listar contas */
  getAll(): Observable<AccountResponse[]> {
    return this.http.get<ApiResponse<AccountResponse[]>>(this.baseUrl)
      .pipe(map(res => res.data));
  }

  /** POST /api/v1/accounts/{id}/activate */
  activate(id: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/activate`, {});
  }

  /** POST /api/v1/accounts/{id}/block */
  block(id: string, reason: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/block`, { reason });
  }
}
