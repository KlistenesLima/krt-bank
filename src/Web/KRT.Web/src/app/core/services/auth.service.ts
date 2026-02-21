import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient, private router: Router) {}

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  confirmEmail(email: string, code: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/confirm-email`, { email, code });
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(email: string, code: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, { email, code, newPassword });
  }

  login(identifier: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, { identifier, password }).pipe(
      tap((res: any) => {
        if (res.success) {
          localStorage.setItem('krt_token', res.accessToken);
          localStorage.setItem('krt_refresh_token', res.refreshToken || '');
          localStorage.setItem('krt_account_id', res.account.id);
          localStorage.setItem('krt_account_name', res.account.name);
          localStorage.setItem('krt_account_doc', res.account.document);
          localStorage.setItem('krt_account_email', res.account.email);
          localStorage.setItem('krt_account_balance', res.account.balance?.toString() || '0');
          localStorage.setItem('krt_account_role', res.account?.role || 'User');
          localStorage.setItem('krt_account_status', res.account.status || 'Active');
        }
      })
    );
  }

  logout(): void {
    const keys = [
      'krt_token', 'krt_refresh_token', 'krt_account_id',
      'krt_account_name', 'krt_account_doc', 'krt_account_email',
      'krt_account_balance', 'krt_account_status', 'krt_account_role'
    ];
    keys.forEach(k => localStorage.removeItem(k));
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('krt_token');
  }

  isAuthenticated(): boolean {
    return this.isLoggedIn();
  }

  getToken(): string | null {
    return localStorage.getItem('krt_token');
  }

  getAccountId(): string | null {
    return localStorage.getItem('krt_account_id');
  }

  get accountId(): string | null {
    return this.getAccountId();
  }

  getAccountName(): string | null {
    return localStorage.getItem('krt_account_name');
  }

  getBalance(): number {
    const bal = localStorage.getItem('krt_account_balance');
    return bal ? parseFloat(bal) : 0;
  }

  updateBalance(newBalance: number): void {
    localStorage.setItem('krt_account_balance', newBalance.toString());
  }

  getRole(): string {
    return localStorage.getItem('krt_account_role') || 'User';
  }

  isAdmin(): boolean {
    const role = this.getRole();
    return role === 'Admin' || role === 'Administrador';
  }

  get currentSession(): any {
    return {
      name: localStorage.getItem('krt_account_name') || 'Usuario',
      document: localStorage.getItem('krt_account_doc') || '',
      email: localStorage.getItem('krt_account_email') || '',
      balance: this.getBalance()
    };
  }
}
