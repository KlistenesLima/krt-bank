import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

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

        <div *ngIf="errorMsg" class="error-box">{{ errorMsg }}</div>

        <form (ngSubmit)="login()">
            <mat-form-field appearance="fill" class="custom-field">
              <mat-label>CPF</mat-label>
              <input matInput placeholder="000.000.000-00" [(ngModel)]="cpf" name="cpf"
                     [disabled]="isLoading" maxlength="14" (input)="maskCpf($event)">
              <mat-icon matSuffix>badge</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="fill" class="custom-field">
              <mat-label>Senha</mat-label>
              <input matInput [type]="showPass ? 'text' : 'password'" [(ngModel)]="password"
                     name="password" [disabled]="isLoading">
              <button mat-icon-button matSuffix type="button" (click)="showPass = !showPass">
                <mat-icon>{{ showPass ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
            </mat-form-field>

            <button mat-raised-button color="primary" class="full-width-btn" type="submit"
                    [disabled]="isLoading || !cpf || !password">
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
        background: linear-gradient(135deg, #0047BB 0%, #002a70 100%); padding: 20px;
    }
    .auth-card {
      background: white; width: 100%; max-width: 400px;
      padding: 40px 30px; border-radius: 24px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.2); text-align: center;
    }
    .logo-icon {
      background: rgba(0, 71, 187, 0.1); color: #0047BB;
      width: 64px; height: 64px; border-radius: 20px;
      margin: 0 auto 16px; display: flex; align-items: center; justify-content: center;
    }
    .brand-header h2 { color: #002a70; margin: 0; }
    .brand-header p { color: #666; margin-bottom: 20px; }
    .full-width-btn { width: 100%; margin-top: 10px; height: 50px; font-size: 1rem; display: flex; justify-content: center; align-items: center; }
    .custom-field { width: 100%; margin-bottom: 10px; }
    .actions { margin-top: 30px; display: flex; flex-direction: column; gap: 10px; }
    .divider-text { color: #666; font-size: 0.9rem; }
    button[mat-stroked-button] { width: 100%; height: 45px; border: 2px solid #0047BB; }
    .error-box { background: #FFEBEE; color: #C62828; padding: 10px 16px; border-radius: 8px; margin-bottom: 16px; font-size: 0.9rem; }
    ::ng-deep .mat-mdc-progress-spinner circle { stroke: white !important; }
  `]
})
export class LoginComponent {
  cpf = '';
  password = '';
  isLoading = false;
  showPass = false;
  errorMsg = '';

  constructor(private auth: AuthService, private router: Router) {}

  maskCpf(event: any) {
    let v = event.target.value.replace(/\D/g, '');
    if (v.length > 11) v = v.slice(0, 11);
    if (v.length > 9) v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{1,2})/, '$1.$2.$3-$4');
    else if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d{1,3})/, '$1.$2.$3');
    else if (v.length > 3) v = v.replace(/(\d{3})(\d{1,3})/, '$1.$2');
    this.cpf = v;
    event.target.value = v;
  }

  login() {
    this.isLoading = true;
    this.errorMsg = '';

    this.auth.login(this.cpf, this.password).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res.success) {
          this.router.navigate(['/dashboard']);
        } else {
          this.errorMsg = res.error || 'Erro ao fazer login';
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMsg = err.error?.error || 'CPF ou senha inválidos';
      }
    });
  }

  goToRegister() { this.router.navigate(['/register']); }
}

