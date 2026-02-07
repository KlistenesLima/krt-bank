import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/v1/auth';

  constructor(private http: HttpClient, private router: Router) {}

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  login(cpf: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, { cpf, password }).pipe(
      tap((res: any) => {
        if (res.success) {
          localStorage.setItem('krt_token', res.accessToken);
          localStorage.setItem('krt_refresh_token', res.refreshToken || '');
          localStorage.setItem('krt_account_id', res.account.id);
          localStorage.setItem('krt_account_name', res.account.name);
          localStorage.setItem('krt_account_doc', res.account.document);
          localStorage.setItem('krt_account_email', res.account.email);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('krt_token');
    localStorage.removeItem('krt_refresh_token');
    localStorage.removeItem('krt_account_id');
    localStorage.removeItem('krt_account_name');
    localStorage.removeItem('krt_account_doc');
    localStorage.removeItem('krt_account_email');
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('krt_token');
  }

  getToken(): string | null {
    return localStorage.getItem('krt_token');
  }

  getAccountId(): string | null {
    return localStorage.getItem('krt_account_id');
  }

  getAccountName(): string | null {
    return localStorage.getItem('krt_account_name');
  }

  get accountId(): string | null {
    return this.getAccountId();
  }

  get currentSession(): any {
    return {
      name: localStorage.getItem('krt_account_name') || 'Usuario',
      document: localStorage.getItem('krt_account_doc') || '',
      email: localStorage.getItem('krt_account_email') || ''
    };
  }
}
