import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { AccountService } from '../../../../core/services/account.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-login',
  template: `
    <div class="full-screen-center">
      <div class="auth-card fade-in">
        
        <div class="brand-header">
           <div class="logo-icon">
             <mat-icon>account_balance_wallet</mat-icon>
           </div>
           <h2>KRT Bank</h2>
           <p>Banking Reimagined</p>
        </div>

        <!-- Se já logou no Keycloak mas não tem conta vinculada -->
        <div *ngIf="isKeycloakLoggedIn && !hasAccount" class="link-section">
          <p>Olá, <strong>{{ keycloakName }}</strong>! Vincule sua conta bancária:</p>
          
          <mat-form-field appearance="fill" class="custom-field">
            <mat-label>ID da Conta Bancária</mat-label>
            <input matInput placeholder="Cole o ID retornado no cadastro" [(ngModel)]="accountId" name="accountId">
            <mat-icon matSuffix>account_balance</mat-icon>
          </mat-form-field>

          <button mat-raised-button color="primary" class="full-width-btn" 
                  (click)="linkAccount()" [disabled]="isLoading || !accountId">
            <span *ngIf="!isLoading">VINCULAR CONTA</span>
            <mat-spinner diameter="24" *ngIf="isLoading"></mat-spinner>
          </button>
        </div>

        <!-- Se já tem conta vinculada, redireciona -->
        <div *ngIf="isKeycloakLoggedIn && hasAccount" class="link-section">
          <mat-spinner diameter="40"></mat-spinner>
          <p>Carregando dashboard...</p>
        </div>

        <!-- Se não logou no Keycloak -->
        <div *ngIf="!isKeycloakLoggedIn">
          <div *ngIf="errorMsg" class="error-banner">
            <mat-icon>error_outline</mat-icon> {{ errorMsg }}
          </div>

          <button mat-raised-button color="primary" class="full-width-btn" (click)="loginKeycloak()">
            <mat-icon>login</mat-icon> ENTRAR COM KEYCLOAK
          </button>
          
          <div class="actions">
            <span class="divider-text">Novo por aqui?</span>
            <button mat-stroked-button color="primary" (click)="goToRegister()">
              Abrir Conta Gratuita
            </button>
          </div>
        </div>

      </div>
    </div>
  `,
  styles: [`
    .full-screen-center {
        min-height: 100vh; display: flex; align-items: center; justify-content: center;
        background: linear-gradient(135deg, #0047BB 0%, #002a70 100%);
        padding: 20px;
    }
    .auth-card { 
      background: white; width: 100%; max-width: 400px; 
      padding: 40px 30px; border-radius: 24px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.2); text-align: center;
    }
    .logo-icon { 
      background: rgba(0, 71, 187, 0.1); color: var(--primary);
      width: 64px; height: 64px; border-radius: 20px; 
      margin: 0 auto 16px; display: flex; align-items: center; justify-content: center;
    }
    .brand-header h2 { color: #002a70; margin: 0; }
    .brand-header p { color: #666; margin-bottom: 20px; }
    .error-banner { 
      background: #ffebee; color: #c62828; padding: 10px 15px; border-radius: 8px; 
      margin-bottom: 15px; display: flex; align-items: center; gap: 8px; font-size: 0.9rem;
    }
    .full-width-btn { width: 100%; margin-top: 10px; height: 50px; font-size: 1rem; display: flex; justify-content: center; align-items: center; gap: 8px; }
    .custom-field { width: 100%; margin-bottom: 10px; }
    .actions { margin-top: 30px; display: flex; flex-direction: column; gap: 10px; }
    .divider-text { color: #666; font-size: 0.9rem; }
    button[mat-stroked-button] { width: 100%; height: 45px; border: 2px solid var(--primary); }
    .link-section { margin-top: 10px; }
    .link-section p { color: #555; margin-bottom: 15px; }
    ::ng-deep .mat-mdc-progress-spinner circle { stroke: white !important; }
  `]
})
export class LoginComponent implements OnInit {
  accountId = '';
  isLoading = false;
  errorMsg = '';
  isKeycloakLoggedIn = false;
  hasAccount = false;
  keycloakName = '';

  constructor(
    private router: Router,
    private authService: AuthService,
    private accountService: AccountService,
    private notify: NotificationService
  ) {}

  async ngOnInit() {
    this.isKeycloakLoggedIn = this.authService.isLoggedIn;

    if (this.isKeycloakLoggedIn) {
      // Já logou no Keycloak
      const profile = await this.authService.loadProfile();
      this.keycloakName = profile?.firstName || profile?.username || 'Usuário';

      // Verifica se já tem conta vinculada
      const session = this.authService.currentSession;
      if (session?.accountId) {
        this.hasAccount = true;
        // Redireciona direto
        setTimeout(() => this.router.navigate(['/dashboard']), 500);
      }
    }
  }

  loginKeycloak() {
    this.authService.login();
  }

  linkAccount() {
    this.isLoading = true;
    this.errorMsg = '';

    this.accountService.getById(this.accountId.trim()).subscribe({
      next: (account) => {
        this.authService.saveSession({
          accountId: account.id,
          customerName: account.customerName,
          document: account.document,
          email: account.email,
          keycloakId: undefined
        });
        this.notify.success('Conta vinculada! Bem-vindo, ' + account.customerName);
        this.router.navigate(['/dashboard']);
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMsg = err.status === 404
          ? 'Conta não encontrada. Verifique o ID.'
          : 'Erro ao conectar. Backend rodando?';
      }
    });
  }

  goToRegister() { this.router.navigate(['/register']); }
}
