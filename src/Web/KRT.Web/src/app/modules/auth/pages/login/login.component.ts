import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../../../../core/services/account.service';
import { AuthService } from '../../../../core/services/auth.service';
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

        <div *ngIf="errorMsg" class="error-banner">
          <mat-icon>error_outline</mat-icon> {{ errorMsg }}
        </div>
        
        <form (ngSubmit)="login()">
            <mat-form-field appearance="fill" class="custom-field">
              <mat-label>ID da Conta</mat-label>
              <input matInput placeholder="Cole o ID retornado no cadastro" [(ngModel)]="accountId" name="accountId" [disabled]="isLoading">
              <mat-icon matSuffix>badge</mat-icon>
            </mat-form-field>

            <button mat-raised-button color="primary" class="full-width-btn" type="submit" [disabled]="isLoading || !accountId">
              <span *ngIf="!isLoading">ACESSAR MINHA CONTA</span>
              <mat-spinner diameter="24" *ngIf="isLoading" color="accent"></mat-spinner>
            </button>
        </form>
        
        <div class="actions">
          <span class="divider-text">Novo por aqui?</span>
          <button mat-stroked-button color="primary" (click)="goToRegister()" [disabled]="isLoading">
            Abrir Conta Gratuita
          </button>
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
    .full-width-btn { width: 100%; margin-top: 10px; height: 50px; font-size: 1rem; display: flex; justify-content: center; align-items: center; }
    .custom-field { width: 100%; margin-bottom: 10px; }
    .actions { margin-top: 30px; display: flex; flex-direction: column; gap: 10px; }
    .divider-text { color: #666; font-size: 0.9rem; }
    button[mat-stroked-button] { width: 100%; height: 45px; border: 2px solid var(--primary); }
    ::ng-deep .mat-mdc-progress-spinner circle { stroke: white !important; }
  `]
})
export class LoginComponent {
  accountId = '';
  isLoading = false;
  errorMsg = '';

  constructor(
    private router: Router,
    private accountService: AccountService,
    private authService: AuthService,
    private notify: NotificationService
  ) {
    // Se já logado, ir pro dashboard
    if (this.authService.isLoggedIn) this.router.navigate(['/dashboard']);
  }

  login() {
    this.isLoading = true;
    this.errorMsg = '';

    this.accountService.getById(this.accountId.trim()).subscribe({
      next: (account) => {
        this.authService.login({
          accountId: account.id,
          customerName: account.customerName,
          document: account.document,
          email: account.email
        });
        this.notify.success('Bem-vindo, ' + account.customerName + '!');
        this.router.navigate(['/dashboard']);
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMsg = err.status === 404
          ? 'Conta não encontrada. Verifique o ID.'
          : 'Erro ao conectar. Verifique se o backend está rodando.';
      }
    });
  }

  goToRegister() { this.router.navigate(['/register']); }
}
