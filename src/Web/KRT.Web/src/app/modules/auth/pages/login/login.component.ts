import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  template: `
    <div class="login-bg">
      <div class="login-card fade-in">

        <div class="brand">
          <div class="logo">
            <svg viewBox="0 0 32 32" width="40" height="40">
              <rect width="32" height="32" rx="8" fill="#0047BB"/>
              <path d="M16 5L6 11v2h20v-2L16 5z" fill="white"/>
              <rect x="8" y="14" width="3" height="8" rx="0.5" fill="white"/>
              <rect x="14.5" y="14" width="3" height="8" rx="0.5" fill="white"/>
              <rect x="21" y="14" width="3" height="8" rx="0.5" fill="white"/>
              <rect x="6" y="23" width="20" height="3" rx="1" fill="white"/>
            </svg>
          </div>
          <h2>KRT Bank</h2>
          <p>Banking Reimagined</p>
        </div>

        <div *ngIf="errorMsg" class="error-msg">{{ errorMsg }}</div>

        <form (ngSubmit)="login()">
          <div class="field">
            <label>CPF</label>
            <div class="input-wrap">
              <input type="text" [(ngModel)]="cpf" name="cpf" placeholder="000.000.000-00"
                     maxlength="14" (input)="maskCpf($event)" [disabled]="isLoading" autocomplete="off">
              <mat-icon>badge</mat-icon>
            </div>
          </div>

          <div class="field">
            <label>Senha</label>
            <div class="input-wrap">
              <input [type]="showPass ? 'text' : 'password'" [(ngModel)]="password" name="password"
                     placeholder="Digite sua senha" [disabled]="isLoading" autocomplete="off">
              <mat-icon class="clickable" (click)="showPass = !showPass">
                {{ showPass ? 'visibility_off' : 'visibility' }}
              </mat-icon>
            </div>
          </div>

          <button type="submit" class="btn-primary" [disabled]="isLoading || !cpf || !password">
            <span *ngIf="!isLoading">ACESSAR MINHA CONTA</span>
            <mat-spinner diameter="22" *ngIf="isLoading"></mat-spinner>
          </button>
        </form>

        <div class="divider">
          <span>Novo por aqui?</span>
        </div>

        <button class="btn-outline" (click)="goToRegister()" [disabled]="isLoading">
          Abrir Conta Gratuita
        </button>
      </div>
    </div>
  `,
  styles: [`
    .login-bg {
      min-height: 100vh; display: flex; align-items: center; justify-content: center;
      background: linear-gradient(135deg, #0047BB 0%, #002a70 50%, #001a4d 100%);
      padding: 20px;
    }
    .login-card {
      background: #fff; width: 100%; max-width: 400px;
      padding: 40px 32px; border-radius: 24px;
      box-shadow: 0 24px 48px rgba(0,0,0,0.25);
    }

    /* Brand */
    .brand { text-align: center; margin-bottom: 32px; }
    .logo {
      width: 64px; height: 64px; margin: 0 auto 14px;
      background: rgba(0,71,187,0.08); border-radius: 18px;
      display: flex; align-items: center; justify-content: center;
    }
    .brand h2 { color: #002a70; margin: 0; font-size: 1.5rem; font-weight: 800; }
    .brand p { color: #888; margin: 4px 0 0; font-size: 0.88rem; }

    /* Fields */
    .field { margin-bottom: 20px; }
    .field label {
      display: block; font-size: 0.82rem; font-weight: 600;
      color: #555; margin-bottom: 8px;
    }
    .input-wrap {
      display: flex; align-items: center; gap: 10px;
      border: 2px solid #E5E7EB; border-radius: 14px;
      padding: 0 16px; height: 52px;
      transition: border-color 0.2s;
      background: #FAFBFC;
    }
    .input-wrap:focus-within {
      border-color: #0047BB; background: #fff;
    }
    .input-wrap input {
      flex: 1; border: none; outline: none; background: transparent;
      font-size: 1rem; font-family: 'Plus Jakarta Sans', sans-serif;
      color: #1A1A2E;
    }
    .input-wrap input::placeholder { color: #B0B8C4; }
    .input-wrap mat-icon { color: #9CA3AF; font-size: 22px; width: 22px; height: 22px; }
    .input-wrap .clickable { cursor: pointer; }

    /* Buttons */
    .btn-primary {
      width: 100%; height: 54px; border: none; border-radius: 14px;
      background: linear-gradient(135deg, #0047BB, #002a70);
      color: white; font-size: 0.95rem; font-weight: 700;
      cursor: pointer; margin-top: 8px;
      display: flex; align-items: center; justify-content: center;
      box-shadow: 0 8px 24px rgba(0,71,187,0.3);
      transition: all 0.3s; letter-spacing: 0.5px;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .btn-primary:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 12px 32px rgba(0,71,187,0.4);
    }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }

    .divider {
      text-align: center; margin: 28px 0 16px;
      position: relative;
    }
    .divider span {
      background: #fff; padding: 0 12px;
      color: #999; font-size: 0.85rem;
      position: relative; z-index: 1;
    }
    .divider::before {
      content: ''; position: absolute; top: 50%; left: 0; right: 0;
      height: 1px; background: #E5E7EB; z-index: 0;
    }

    .btn-outline {
      width: 100%; height: 50px; border: 2px solid #0047BB;
      border-radius: 14px; background: transparent;
      color: #0047BB; font-size: 0.92rem; font-weight: 700;
      cursor: pointer; transition: all 0.2s;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .btn-outline:hover:not(:disabled) { background: rgba(0,71,187,0.05); }

    /* Error */
    .error-msg {
      background: #FFF0F0; color: #D32F2F; padding: 12px 16px;
      border-radius: 12px; margin-bottom: 20px; font-size: 0.88rem;
      font-weight: 500; text-align: center;
    }

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
