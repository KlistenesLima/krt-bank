import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

export interface UserSession {
  accountId: string;
  customerName: string;
  document: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly STORAGE_KEY = 'krt_session';
  private sessionSubject = new BehaviorSubject<UserSession | null>(this.loadSession());

  session$ = this.sessionSubject.asObservable();

  constructor(private router: Router) {}

  get currentSession(): UserSession | null {
    return this.sessionSubject.value;
  }

  get accountId(): string | null {
    return this.currentSession?.accountId ?? null;
  }

  get isLoggedIn(): boolean {
    return !!this.currentSession;
  }

  login(session: UserSession): void {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(session));
    // Manter compatibilidade com código que usa krt_account_id
    localStorage.setItem('krt_account_id', session.accountId);
    this.sessionSubject.next(session);
  }

  logout(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    localStorage.removeItem('krt_account_id');
    this.sessionSubject.next(null);
    this.router.navigate(['/login']);
  }

  private loadSession(): UserSession | null {
    try {
      const raw = localStorage.getItem(this.STORAGE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
