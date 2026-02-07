import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, AccountResponse, BalanceResponse, CreateAccountRequest } from '../models/account.model';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private baseUrl = `${environment.apiUrl}/accounts`;
  constructor(private http: HttpClient) {}

  create(data: CreateAccountRequest): Observable<ApiResponse<{ id: string }>> {
    return this.http.post<ApiResponse<{ id: string }>>(this.baseUrl, data);
  }

  getById(id: string): Observable<AccountResponse> {
    return this.http.get<ApiResponse<AccountResponse>>(`${this.baseUrl}/${id}`)
      .pipe(map(res => res.data));
  }

  getBalance(id: string): Observable<BalanceResponse> {
    return this.http.get<ApiResponse<BalanceResponse>>(`${this.baseUrl}/${id}/balance`)
      .pipe(map(res => res.data));
  }

  getAll(): Observable<AccountResponse[]> {
    return this.http.get<ApiResponse<AccountResponse[]>>(this.baseUrl)
      .pipe(map(res => res.data));
  }

  activate(id: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/activate`, {});
  }

  block(id: string, reason: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/block`, { reason });
  }
}
