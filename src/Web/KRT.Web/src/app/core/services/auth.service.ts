import { Injectable } from '@angular/core';
import { KeycloakService } from 'keycloak-angular';
import { KeycloakProfile } from 'keycloak-js';
import { Router } from '@angular/router';

export interface UserSession {
  accountId: string;
  customerName: string;
  document: string;
  email: string;
  keycloakId?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly STORAGE_KEY = 'krt_session';
  private profile: KeycloakProfile | null = null;

  constructor(
    private keycloak: KeycloakService,
    private router: Router
  ) {}

  /** Verifica se está logado no Keycloak */
  get isLoggedIn(): boolean {
    try {
      return this.keycloak.isLoggedIn();
    } catch {
      return !!this.currentSession;
    }
  }

  /** Dados da sessão (armazenados após vincular conta) */
  get currentSession(): UserSession | null {
    try {
      const raw = localStorage.getItem(this.STORAGE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch { return null; }
  }

  get accountId(): string | null {
    return this.currentSession?.accountId ?? null;
  }

  /** Pega o token JWT do Keycloak para API calls */
  async getToken(): Promise<string> {
    try {
      return await this.keycloak.getToken();
    } catch { return ''; }
  }

  /** Pega o perfil do Keycloak */
  async loadProfile(): Promise<KeycloakProfile | null> {
    try {
      if (!this.keycloak.isLoggedIn()) return null;
      this.profile = await this.keycloak.loadUserProfile();
      return this.profile;
    } catch { return null; }
  }

  /** Redireciona para tela de login do Keycloak */
  login(): void {
    this.keycloak.login({
      redirectUri: window.location.origin + '/dashboard'
    });
  }

  /** Vincula conta bancária à sessão (após criar ou buscar conta) */
  saveSession(session: UserSession): void {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(session));
    localStorage.setItem('krt_account_id', session.accountId);
  }

  /** Logout: limpa Keycloak + sessão local */
  logout(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    localStorage.removeItem('krt_account_id');
    try {
      this.keycloak.logout(window.location.origin + '/login');
    } catch {
      this.router.navigate(['/login']);
    }
  }

  /** Registrar novo usuário no Keycloak */
  register(): void {
    this.keycloak.register({
      redirectUri: window.location.origin + '/register'
    });
  }

  /** Roles do Keycloak */
  hasRole(role: string): boolean {
    try {
      return this.keycloak.isUserInRole(role);
    } catch { return false; }
  }
}
