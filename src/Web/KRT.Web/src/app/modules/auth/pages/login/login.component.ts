import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  template: `
    <div class="login-bg">
      <!-- Animated floating shapes -->
      <div class="bg-shapes">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
        <div class="shape shape-4"></div>
        <div class="shape shape-5"></div>
      </div>

      <div class="login-wrapper fade-in">
        <!-- Left panel - branding -->
        <div class="brand-panel">
          <div class="brand-content">
            <div class="logo-icon">
              <svg viewBox="0 0 32 32" width="48" height="48">
                <rect width="32" height="32" rx="8" fill="white"/>
                <path d="M16 5L6 11v2h20v-2L16 5z" fill="#0047BB"/>
                <rect x="8" y="14" width="3" height="8" rx="0.5" fill="#0047BB"/>
                <rect x="14.5" y="14" width="3" height="8" rx="0.5" fill="#0047BB"/>
                <rect x="21" y="14" width="3" height="8" rx="0.5" fill="#0047BB"/>
                <rect x="6" y="23" width="20" height="3" rx="1" fill="#0047BB"/>
              </svg>
            </div>
            <h1>KRT Bank</h1>
            <p class="tagline">Banking Reimagined</p>
            <div class="features">
              <div class="feature-item">
                <mat-icon>bolt</mat-icon>
                <span>PIX instantaneo 24h</span>
              </div>
              <div class="feature-item">
                <mat-icon>shield</mat-icon>
                <span>Anti-fraude inteligente</span>
              </div>
              <div class="feature-item">
                <mat-icon>trending_up</mat-icon>
                <span>Investimentos e metas</span>
              </div>
              <div class="feature-item">
                <mat-icon>credit_card</mat-icon>
                <span>Cartao virtual gratuito</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Right panel - form -->
        <div class="form-panel">
          <div class="form-header">
            <h2>Bem-vindo de volta</h2>
            <p>Acesse sua conta para continuar</p>
          </div>

          <div *ngIf="errorMsg" class="error-msg slide-down">
            <mat-icon>error_outline</mat-icon>
            {{ errorMsg }}
          </div>

          <!-- Status messages -->
          <div *ngIf="statusMsg" class="status-msg slide-down" [class]="'status-' + statusType">
            <mat-icon>{{ statusIcon }}</mat-icon>
            <div>
              <span>{{ statusMsg }}</span>
              <a *ngIf="statusType === 'email'" class="status-link" (click)="goToRegister()">Reenviar codigo</a>
            </div>
          </div>

          <form (ngSubmit)="login()">
            <div class="field">
              <label>Email ou CPF</label>
              <div class="input-wrap" [class.focused]="identifierFocused" [class.filled]="identifier">
                <mat-icon class="field-icon">{{ isIdentifierCpf() ? 'badge' : 'email' }}</mat-icon>
                <input type="text" [(ngModel)]="identifier" name="identifier" placeholder="Email ou CPF"
                       [maxlength]="isIdentifierCpf() ? 14 : 100" (input)="onIdentifierInput($event)" [disabled]="isLoading"
                       (focus)="identifierFocused=true" (blur)="identifierFocused=false" autocomplete="off">
              </div>
            </div>

            <div class="field">
              <label>Senha</label>
              <div class="input-wrap" [class.focused]="passFocused" [class.filled]="password">
                <mat-icon class="field-icon">lock</mat-icon>
                <input [type]="showPass ? 'text' : 'password'" [(ngModel)]="password" name="password"
                       placeholder="Digite sua senha" [disabled]="isLoading"
                       (focus)="passFocused=true" (blur)="passFocused=false" autocomplete="off">
                <mat-icon class="toggle-pass" (click)="showPass = !showPass">
                  {{ showPass ? 'visibility_off' : 'visibility' }}
                </mat-icon>
              </div>
            </div>

            <a class="forgot-link" (click)="goToForgotPassword()">Esqueci minha senha</a>

            <button type="submit" class="btn-primary" [disabled]="isLoading || !identifier || !password"
                    [class.loading]="isLoading">
              <span *ngIf="!isLoading">Entrar</span>
              <div class="spinner" *ngIf="isLoading">
                <div class="dot dot1"></div>
                <div class="dot dot2"></div>
                <div class="dot dot3"></div>
              </div>
            </button>
          </form>

          <div class="divider">
            <span>ou</span>
          </div>

          <button class="btn-register" (click)="goToRegister()" [disabled]="isLoading">
            <mat-icon>person_add</mat-icon>
            Criar conta gratuita
          </button>

          <p class="footer-text">
            Protegido com criptografia de ponta a ponta
            <mat-icon class="lock-icon">verified_user</mat-icon>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    /* === BACKGROUND === */
    .login-bg {
      min-height: 100vh; display: flex; align-items: center; justify-content: center;
      background: linear-gradient(135deg, #0a1628 0%, #0d2137 30%, #0a2a4a 60%, #071e3d 100%);
      padding: 20px; position: relative; overflow: hidden;
    }

    /* Floating shapes */
    .bg-shapes { position: absolute; inset: 0; pointer-events: none; overflow: hidden; }
    .shape {
      position: absolute; border-radius: 50%;
      background: radial-gradient(circle, rgba(0,100,255,0.15), transparent 70%);
      animation: float 20s infinite ease-in-out;
    }
    .shape-1 { width: 600px; height: 600px; top: -200px; right: -150px; animation-delay: 0s; }
    .shape-2 { width: 400px; height: 400px; bottom: -100px; left: -100px; animation-delay: -5s;
      background: radial-gradient(circle, rgba(0,200,150,0.1), transparent 70%); }
    .shape-3 { width: 300px; height: 300px; top: 50%; left: 30%; animation-delay: -10s;
      background: radial-gradient(circle, rgba(100,100,255,0.08), transparent 70%); }
    .shape-4 { width: 200px; height: 200px; top: 20%; left: 10%; animation-delay: -15s;
      background: radial-gradient(circle, rgba(0,150,255,0.12), transparent 70%); }
    .shape-5 { width: 350px; height: 350px; bottom: 10%; right: 20%; animation-delay: -7s;
      background: radial-gradient(circle, rgba(50,100,255,0.1), transparent 70%); }

    @keyframes float {
      0%, 100% { transform: translate(0, 0) scale(1); }
      25% { transform: translate(30px, -40px) scale(1.05); }
      50% { transform: translate(-20px, 20px) scale(0.95); }
      75% { transform: translate(40px, 30px) scale(1.02); }
    }

    /* === WRAPPER === */
    .login-wrapper {
      display: flex; width: 100%; max-width: 900px;
      border-radius: 28px; overflow: hidden;
      box-shadow: 0 32px 64px rgba(0,0,0,0.4), 0 0 0 1px rgba(255,255,255,0.05);
      backdrop-filter: blur(20px);
      position: relative; z-index: 1;
    }

    /* === BRAND PANEL (left) === */
    .brand-panel {
      flex: 1; padding: 48px 40px;
      background: linear-gradient(135deg, #0047BB 0%, #002a70 50%, #001a4d 100%);
      display: flex; align-items: center; justify-content: center;
      position: relative; overflow: hidden;
    }
    .brand-panel::after {
      content: ''; position: absolute; inset: 0;
      background: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.03'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
    }
    .brand-content { position: relative; z-index: 1; color: white; }
    .logo-icon {
      width: 72px; height: 72px; border-radius: 20px;
      background: rgba(255,255,255,0.15); backdrop-filter: blur(10px);
      display: flex; align-items: center; justify-content: center;
      margin-bottom: 20px;
      border: 1px solid rgba(255,255,255,0.2);
    }
    .brand-content h1 {
      font-size: 2rem; font-weight: 800; margin: 0 0 4px;
      letter-spacing: -0.5px;
    }
    .tagline { opacity: 0.7; font-size: 0.95rem; margin: 0 0 36px; }

    .features { display: flex; flex-direction: column; gap: 16px; }
    .feature-item {
      display: flex; align-items: center; gap: 12px;
      font-size: 0.9rem; opacity: 0.85;
    }
    .feature-item mat-icon {
      font-size: 20px; width: 20px; height: 20px;
      color: rgba(255,255,255,0.9);
    }

    /* === FORM PANEL (right) === */
    .form-panel {
      flex: 1; padding: 48px 40px; background: #ffffff;
      display: flex; flex-direction: column; justify-content: center;
    }
    .form-header h2 {
      font-size: 1.6rem; font-weight: 800; color: #1A1A2E; margin: 0 0 6px;
    }
    .form-header p {
      color: #9CA3AF; font-size: 0.92rem; margin: 0 0 32px;
    }

    /* Fields */
    .field { margin-bottom: 22px; }
    .field label {
      display: block; font-size: 0.8rem; font-weight: 700;
      color: #6B7280; margin-bottom: 8px; text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .input-wrap {
      display: flex; align-items: center; gap: 12px;
      border: 2px solid #E5E7EB; border-radius: 14px;
      padding: 0 16px; height: 54px;
      transition: all 0.3s ease;
      background: #F9FAFB;
    }
    .input-wrap.focused {
      border-color: #0047BB;
      background: #fff;
      box-shadow: 0 0 0 4px rgba(0,71,187,0.08);
    }
    .input-wrap.filled { border-color: #CBD5E1; background: #fff; }
    .input-wrap input {
      flex: 1; border: none; outline: none; background: transparent;
      font-size: 1rem; font-family: 'Plus Jakarta Sans', sans-serif;
      color: #1A1A2E;
    }
    .input-wrap input::placeholder { color: #C4C9D4; }
    .field-icon { color: #9CA3AF; font-size: 22px; width: 22px; height: 22px; }
    .input-wrap.focused .field-icon { color: #0047BB; }
    .toggle-pass { cursor: pointer; color: #9CA3AF; font-size: 22px; width: 22px; height: 22px;
      transition: color 0.2s; }
    .toggle-pass:hover { color: #6B7280; }

    /* Forgot password link */
    .forgot-link {
      display: block; text-align: right; font-size: 0.82rem;
      color: #0047BB; font-weight: 700; cursor: pointer;
      margin: -12px 0 8px; text-decoration: none;
    }
    .forgot-link:hover { text-decoration: underline; }

    /* Primary button */
    .btn-primary {
      width: 100%; height: 54px; border: none; border-radius: 14px;
      background: linear-gradient(135deg, #0047BB 0%, #0035a0 100%);
      color: white; font-size: 1rem; font-weight: 700;
      cursor: pointer; margin-top: 8px;
      display: flex; align-items: center; justify-content: center;
      box-shadow: 0 8px 24px rgba(0,71,187,0.25);
      transition: all 0.3s ease;
      font-family: 'Plus Jakarta Sans', sans-serif;
      letter-spacing: 0.3px; position: relative; overflow: hidden;
    }
    .btn-primary::before {
      content: ''; position: absolute; inset: 0;
      background: linear-gradient(135deg, transparent 0%, rgba(255,255,255,0.1) 50%, transparent 100%);
      transform: translateX(-100%); transition: transform 0.5s;
    }
    .btn-primary:hover:not(:disabled)::before { transform: translateX(100%); }
    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 12px 32px rgba(0,71,187,0.35);
    }
    .btn-primary:active:not(:disabled) { transform: translateY(0); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

    /* Loading dots */
    .spinner { display: flex; gap: 6px; }
    .dot {
      width: 8px; height: 8px; border-radius: 50%; background: white;
      animation: bounce 1.4s infinite ease-in-out both;
    }
    .dot1 { animation-delay: -0.32s; }
    .dot2 { animation-delay: -0.16s; }
    @keyframes bounce {
      0%, 80%, 100% { transform: scale(0); opacity: 0.5; }
      40% { transform: scale(1); opacity: 1; }
    }

    /* Divider */
    .divider {
      text-align: center; margin: 24px 0; position: relative;
    }
    .divider span {
      background: #fff; padding: 0 16px;
      color: #C4C9D4; font-size: 0.82rem; font-weight: 600;
      position: relative; z-index: 1; text-transform: uppercase;
      letter-spacing: 1px;
    }
    .divider::before {
      content: ''; position: absolute; top: 50%; left: 0; right: 0;
      height: 1px; background: #E5E7EB;
    }

    /* Register button */
    .btn-register {
      width: 100%; height: 50px; border: 2px solid #E5E7EB;
      border-radius: 14px; background: transparent;
      color: #4B5563; font-size: 0.92rem; font-weight: 600;
      cursor: pointer; transition: all 0.25s;
      display: flex; align-items: center; justify-content: center; gap: 8px;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .btn-register mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .btn-register:hover:not(:disabled) {
      border-color: #0047BB; color: #0047BB; background: rgba(0,71,187,0.03);
    }

    /* Footer */
    .footer-text {
      text-align: center; margin-top: 28px; color: #C4C9D4;
      font-size: 0.78rem; display: flex; align-items: center;
      justify-content: center; gap: 4px;
    }
    .lock-icon { font-size: 14px !important; width: 14px !important; height: 14px !important; color: #22C55E; }

    /* Error */
    .error-msg {
      background: #FEF2F2; color: #DC2626; padding: 14px 16px;
      border-radius: 12px; margin-bottom: 20px; font-size: 0.88rem;
      font-weight: 500; display: flex; align-items: center; gap: 8px;
      border: 1px solid #FEE2E2;
    }
    .error-msg mat-icon { font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; }

    /* Status messages */
    .status-msg {
      padding: 14px 16px; border-radius: 12px; margin-bottom: 20px;
      font-size: 0.88rem; font-weight: 500;
      display: flex; align-items: flex-start; gap: 8px;
    }
    .status-msg mat-icon { font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; margin-top: 1px; }
    .status-email { background: #E3F2FD; color: #1565C0; border: 1px solid #BBDEFB; }
    .status-pending { background: #FFF3E0; color: #E65100; border: 1px solid #FFE0B2; }
    .status-inactive { background: #F5F5F5; color: #616161; border: 1px solid #E0E0E0; }
    .status-rejected { background: #FFEBEE; color: #C62828; border: 1px solid #FFCDD2; }
    .status-link {
      display: block; margin-top: 6px; font-weight: 700; cursor: pointer;
      text-decoration: underline; color: inherit;
    }

    .slide-down {
      animation: slideDown 0.3s ease;
    }
    @keyframes slideDown {
      from { opacity: 0; transform: translateY(-10px); }
      to { opacity: 1; transform: translateY(0); }
    }

    /* === RESPONSIVE === */
    @media (max-width: 768px) {
      .login-wrapper { flex-direction: column; max-width: 440px; }
      .brand-panel {
        padding: 32px 28px;
      }
      .features { display: none; }
      .brand-content { text-align: center; }
      .brand-content h1 { font-size: 1.6rem; }
      .tagline { margin-bottom: 0; }
      .logo-icon { margin: 0 auto 16px; }
      .form-panel { padding: 32px 28px; }
      .form-header h2 { font-size: 1.3rem; }
    }

    @media (max-width: 400px) {
      .login-bg { padding: 12px; }
      .form-panel { padding: 24px 20px; }
      .brand-panel { padding: 24px 20px; }
    }

    /* Fade in */
    .fade-in {
      animation: fadeIn 0.6s ease;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(16px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class LoginComponent {
  identifier = '';
  password = '';
  isLoading = false;
  showPass = false;
  errorMsg = '';
  statusMsg = '';
  statusType = '';
  statusIcon = '';
  identifierFocused = false;
  passFocused = false;

  constructor(private auth: AuthService, private router: Router) {}

  isIdentifierCpf(): boolean {
    return /^\d/.test(this.identifier.replace(/\D/g, ''));
  }

  onIdentifierInput(event: any) {
    const raw = event.target.value;
    // Auto-detect: if starts with digits, apply CPF mask
    if (/^\d/.test(raw.replace(/[.\-]/g, '')) && !raw.includes('@')) {
      let v = raw.replace(/\D/g, '');
      if (v.length > 11) v = v.slice(0, 11);
      if (v.length > 9) v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{1,2})/, '$1.$2.$3-$4');
      else if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d{1,3})/, '$1.$2.$3');
      else if (v.length > 3) v = v.replace(/(\d{3})(\d{1,3})/, '$1.$2');
      this.identifier = v;
      event.target.value = v;
    }
  }

  login() {
    this.isLoading = true;
    this.errorMsg = '';
    this.statusMsg = '';
    this.auth.login(this.identifier, this.password).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res.success) {
          this.router.navigate(['/dashboard']);
        } else {
          this.handleLoginResponse(res);
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        this.handleLoginError(err);
      }
    });
  }

  private handleLoginResponse(res: any) {
    const status = res.status || res.userStatus || '';
    const error = res.error || res.message || '';

    if (status === 'PendingEmailConfirmation') {
      this.statusMsg = 'Confirme seu email antes de fazer login.';
      this.statusType = 'email';
      this.statusIcon = 'mark_email_unread';
    } else if (status === 'PendingApproval') {
      this.statusMsg = 'Seu cadastro esta em analise. Aguarde aprovacao.';
      this.statusType = 'pending';
      this.statusIcon = 'hourglass_empty';
    } else if (status === 'Inactive') {
      this.statusMsg = 'Sua conta foi desativada. Entre em contato com o administrador.';
      this.statusType = 'inactive';
      this.statusIcon = 'block';
    } else if (status === 'Rejected') {
      this.statusMsg = 'Seu cadastro nao foi aprovado.';
      this.statusType = 'rejected';
      this.statusIcon = 'cancel';
    } else {
      this.errorMsg = error || 'Erro ao fazer login';
    }
  }

  private handleLoginError(err: any) {
    const status = err.error?.status || err.error?.userStatus || '';
    const error = err.error?.error || err.error?.message || '';

    if (status === 'PendingEmailConfirmation') {
      this.statusMsg = 'Confirme seu email antes de fazer login.';
      this.statusType = 'email';
      this.statusIcon = 'mark_email_unread';
    } else if (status === 'PendingApproval') {
      this.statusMsg = 'Seu cadastro esta em analise. Aguarde aprovacao.';
      this.statusType = 'pending';
      this.statusIcon = 'hourglass_empty';
    } else if (status === 'Inactive') {
      this.statusMsg = 'Sua conta foi desativada. Entre em contato com o administrador.';
      this.statusType = 'inactive';
      this.statusIcon = 'block';
    } else if (status === 'Rejected') {
      this.statusMsg = 'Seu cadastro nao foi aprovado.';
      this.statusType = 'rejected';
      this.statusIcon = 'cancel';
    } else {
      this.errorMsg = error || 'Email/CPF ou senha incorretos.';
    }
  }

  goToRegister() { this.router.navigate(['/register']); }
  goToForgotPassword() { this.router.navigate(['/forgot-password']); }
}
