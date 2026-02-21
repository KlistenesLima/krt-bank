import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  template: `
    <div class="register-bg">
      <div class="bg-shapes">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
      </div>

      <div class="register-wrapper fade-in">
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
            <p class="tagline">Abra sua conta digital</p>
            <div class="features">
              <div class="feature-item">
                <mat-icon>check_circle</mat-icon>
                <span>Conta 100% gratuita</span>
              </div>
              <div class="feature-item">
                <mat-icon>check_circle</mat-icon>
                <span>PIX ilimitado 24h</span>
              </div>
              <div class="feature-item">
                <mat-icon>check_circle</mat-icon>
                <span>Cartao virtual gratis</span>
              </div>
              <div class="feature-item">
                <mat-icon>check_circle</mat-icon>
                <span>Investimentos integrados</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Right panel -->
        <div class="form-panel">
          <!-- STEP 1: Registration Form -->
          <div *ngIf="step === 1" class="step-content fade-in">
            <div class="form-header">
              <h2>Criar sua conta</h2>
              <p>Preencha seus dados para comecar</p>
            </div>

            <div *ngIf="errorMsg" class="error-msg slide-down">
              <mat-icon>error_outline</mat-icon>
              {{ errorMsg }}
            </div>

            <form (ngSubmit)="onRegister()">
              <div class="field">
                <label>Nome completo</label>
                <div class="input-wrap" [class.focused]="nameFocused" [class.filled]="fullName">
                  <mat-icon class="field-icon">person</mat-icon>
                  <input type="text" [(ngModel)]="fullName" name="fullName"
                         placeholder="Seu nome completo" [disabled]="isLoading"
                         (focus)="nameFocused=true" (blur)="nameFocused=false" autocomplete="name">
                </div>
                <span class="field-hint error" *ngIf="fullName && fullName.length < 3">Minimo 3 caracteres</span>
              </div>

              <div class="field">
                <label>Email</label>
                <div class="input-wrap" [class.focused]="emailFocused" [class.filled]="email">
                  <mat-icon class="field-icon">email</mat-icon>
                  <input type="email" [(ngModel)]="email" name="email"
                         placeholder="seu@email.com" [disabled]="isLoading"
                         (focus)="emailFocused=true" (blur)="emailFocused=false" autocomplete="email">
                </div>
                <span class="field-hint error" *ngIf="email && !isEmailValid()">Email invalido</span>
              </div>

              <div class="field">
                <label>CPF</label>
                <div class="input-wrap" [class.focused]="cpfFocused" [class.filled]="cpf">
                  <mat-icon class="field-icon">badge</mat-icon>
                  <input type="text" [(ngModel)]="cpf" name="cpf"
                         placeholder="000.000.000-00" maxlength="14" [disabled]="isLoading"
                         (input)="maskCpf($event)"
                         (focus)="cpfFocused=true" (blur)="cpfFocused=false" autocomplete="off">
                </div>
                <span class="field-hint error" *ngIf="cpf && getCpfDigits().length < 11">CPF incompleto</span>
              </div>

              <div class="field">
                <label>Senha</label>
                <div class="input-wrap" [class.focused]="passFocused" [class.filled]="password">
                  <mat-icon class="field-icon">lock</mat-icon>
                  <input [type]="showPass ? 'text' : 'password'" [(ngModel)]="password" name="password"
                         placeholder="Minimo 8 caracteres" [disabled]="isLoading"
                         (focus)="passFocused=true" (blur)="passFocused=false" autocomplete="new-password">
                  <mat-icon class="toggle-pass" (click)="showPass = !showPass">
                    {{ showPass ? 'visibility_off' : 'visibility' }}
                  </mat-icon>
                </div>
                <div class="strength-bar" *ngIf="password">
                  <div class="strength-fill" [style.width]="getStrengthPercent() + '%'"
                       [class.weak]="passwordStrength === 'fraca'"
                       [class.medium]="passwordStrength === 'media'"
                       [class.strong]="passwordStrength === 'forte'"></div>
                </div>
                <span class="field-hint" [class.weak]="passwordStrength === 'fraca'"
                      [class.medium]="passwordStrength === 'media'"
                      [class.strong]="passwordStrength === 'forte'"
                      *ngIf="password">
                  Senha {{ passwordStrength }}
                </span>
              </div>

              <div class="field">
                <label>Confirmar senha</label>
                <div class="input-wrap" [class.focused]="confirmFocused" [class.filled]="confirmPassword">
                  <mat-icon class="field-icon">lock_outline</mat-icon>
                  <input [type]="showConfirm ? 'text' : 'password'" [(ngModel)]="confirmPassword" name="confirmPassword"
                         placeholder="Repita sua senha" [disabled]="isLoading"
                         (focus)="confirmFocused=true" (blur)="confirmFocused=false" autocomplete="new-password">
                  <mat-icon class="toggle-pass" (click)="showConfirm = !showConfirm">
                    {{ showConfirm ? 'visibility_off' : 'visibility' }}
                  </mat-icon>
                </div>
                <span class="field-hint error" *ngIf="confirmPassword && password !== confirmPassword">As senhas nao coincidem</span>
              </div>

              <button type="submit" class="btn-primary" [disabled]="isLoading || !isFormValid()"
                      [class.loading]="isLoading">
                <span *ngIf="!isLoading">Criar Conta</span>
                <div class="spinner" *ngIf="isLoading">
                  <div class="dot dot1"></div>
                  <div class="dot dot2"></div>
                  <div class="dot dot3"></div>
                </div>
              </button>
            </form>

            <p class="link-text">
              Ja tem conta? <a (click)="goToLogin()">Faca login</a>
            </p>
          </div>

          <!-- STEP 2: Email Verification (OTP) -->
          <div *ngIf="step === 2" class="step-content fade-in">
            <div class="form-header center">
              <div class="otp-icon">
                <mat-icon>mark_email_read</mat-icon>
              </div>
              <h2>Verificar Email</h2>
              <p>Enviamos um codigo de verificacao para <strong>{{ email }}</strong></p>
            </div>

            <div *ngIf="errorMsg" class="error-msg slide-down">
              <mat-icon>error_outline</mat-icon>
              {{ errorMsg }}
            </div>

            <div class="otp-container">
              <input *ngFor="let d of otpDigits; let i = index"
                     type="text" maxlength="1" class="otp-input"
                     [id]="'otp-' + i"
                     [value]="otpDigits[i]"
                     (input)="onOtpInput($event, i)"
                     (keydown)="onOtpKeydown($event, i)"
                     (paste)="onOtpPaste($event)"
                     [disabled]="isLoading"
                     inputmode="numeric">
            </div>

            <div class="timer-row">
              <span class="timer" *ngIf="timerSeconds > 0">
                <mat-icon>schedule</mat-icon>
                {{ formatTimer() }}
              </span>
              <a class="resend-link" *ngIf="timerSeconds <= 0" (click)="resendCode()">
                Reenviar codigo
              </a>
            </div>

            <button class="btn-primary" (click)="onVerify()" [disabled]="isLoading || getOtpCode().length < 6"
                    [class.loading]="isLoading">
              <span *ngIf="!isLoading">Verificar</span>
              <div class="spinner" *ngIf="isLoading">
                <div class="dot dot1"></div>
                <div class="dot dot2"></div>
                <div class="dot dot3"></div>
              </div>
            </button>
          </div>

          <!-- STEP 3: Success -->
          <div *ngIf="step === 3" class="step-content fade-in">
            <div class="success-container">
              <div class="success-icon">
                <mat-icon>check_circle</mat-icon>
              </div>
              <h2>Cadastro recebido com sucesso!</h2>
              <p>Seu acesso sera analisado pelo administrador. Voce recebera um email quando for aprovado.</p>
              <button class="btn-primary" (click)="goToLogin()">
                Voltar ao Login
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .register-bg {
      min-height: 100vh; display: flex; align-items: center; justify-content: center;
      background: linear-gradient(135deg, #0a1628 0%, #0d2137 30%, #0a2a4a 60%, #071e3d 100%);
      padding: 20px; position: relative; overflow: hidden;
    }
    .bg-shapes { position: absolute; inset: 0; pointer-events: none; overflow: hidden; }
    .shape {
      position: absolute; border-radius: 50%;
      background: radial-gradient(circle, rgba(0,100,255,0.15), transparent 70%);
      animation: float 20s infinite ease-in-out;
    }
    .shape-1 { width: 600px; height: 600px; top: -200px; right: -150px; }
    .shape-2 { width: 400px; height: 400px; bottom: -100px; left: -100px; animation-delay: -5s;
      background: radial-gradient(circle, rgba(0,200,150,0.1), transparent 70%); }
    .shape-3 { width: 300px; height: 300px; top: 50%; left: 30%; animation-delay: -10s;
      background: radial-gradient(circle, rgba(100,100,255,0.08), transparent 70%); }
    @keyframes float {
      0%, 100% { transform: translate(0, 0) scale(1); }
      25% { transform: translate(30px, -40px) scale(1.05); }
      50% { transform: translate(-20px, 20px) scale(0.95); }
      75% { transform: translate(40px, 30px) scale(1.02); }
    }

    .register-wrapper {
      display: flex; width: 100%; max-width: 920px;
      border-radius: 28px; overflow: hidden;
      box-shadow: 0 32px 64px rgba(0,0,0,0.4), 0 0 0 1px rgba(255,255,255,0.05);
      backdrop-filter: blur(20px); position: relative; z-index: 1;
    }

    /* Brand Panel */
    .brand-panel {
      flex: 0 0 340px; padding: 48px 36px;
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
      margin-bottom: 20px; border: 1px solid rgba(255,255,255,0.2);
    }
    .brand-content h1 { font-size: 2rem; font-weight: 800; margin: 0 0 4px; letter-spacing: -0.5px; }
    .tagline { opacity: 0.7; font-size: 0.95rem; margin: 0 0 36px; }
    .features { display: flex; flex-direction: column; gap: 14px; }
    .feature-item { display: flex; align-items: center; gap: 10px; font-size: 0.88rem; opacity: 0.85; }
    .feature-item mat-icon { font-size: 20px; width: 20px; height: 20px; color: #00D4AA; }

    /* Form Panel */
    .form-panel {
      flex: 1; padding: 36px 40px; background: #ffffff;
      display: flex; flex-direction: column; justify-content: center;
      overflow-y: auto; max-height: 95vh;
    }
    .step-content { animation: fadeIn 0.4s ease; }
    .form-header h2 { font-size: 1.5rem; font-weight: 800; color: #1A1A2E; margin: 0 0 6px; }
    .form-header p { color: #9CA3AF; font-size: 0.9rem; margin: 0 0 24px; }
    .form-header.center { text-align: center; }
    .form-header.center p { margin-bottom: 28px; }

    /* Fields */
    .field { margin-bottom: 16px; }
    .field label {
      display: block; font-size: 0.78rem; font-weight: 700;
      color: #6B7280; margin-bottom: 6px; text-transform: uppercase; letter-spacing: 0.5px;
    }
    .input-wrap {
      display: flex; align-items: center; gap: 12px;
      border: 2px solid #E5E7EB; border-radius: 14px;
      padding: 0 16px; height: 50px; transition: all 0.3s ease; background: #F9FAFB;
    }
    .input-wrap.focused { border-color: #0047BB; background: #fff; box-shadow: 0 0 0 4px rgba(0,71,187,0.08); }
    .input-wrap.filled { border-color: #CBD5E1; background: #fff; }
    .input-wrap input {
      flex: 1; border: none; outline: none; background: transparent;
      font-size: 0.95rem; font-family: 'Plus Jakarta Sans', sans-serif; color: #1A1A2E;
    }
    .input-wrap input::placeholder { color: #C4C9D4; }
    .field-icon { color: #9CA3AF; font-size: 20px; width: 20px; height: 20px; }
    .input-wrap.focused .field-icon { color: #0047BB; }
    .toggle-pass { cursor: pointer; color: #9CA3AF; font-size: 20px; width: 20px; height: 20px; transition: color 0.2s; }
    .toggle-pass:hover { color: #6B7280; }

    .field-hint {
      display: block; font-size: 0.75rem; margin-top: 4px; font-weight: 600;
    }
    .field-hint.error { color: #E53935; }
    .field-hint.weak { color: #E53935; }
    .field-hint.medium { color: #FF6B35; }
    .field-hint.strong { color: #00C853; }

    /* Password strength bar */
    .strength-bar {
      height: 4px; border-radius: 2px; background: #E5E7EB; margin-top: 8px; overflow: hidden;
    }
    .strength-fill {
      height: 100%; border-radius: 2px; transition: all 0.3s ease;
    }
    .strength-fill.weak { background: #E53935; }
    .strength-fill.medium { background: #FF6B35; }
    .strength-fill.strong { background: #00C853; }

    /* Primary button */
    .btn-primary {
      width: 100%; height: 52px; border: none; border-radius: 14px;
      background: linear-gradient(135deg, #0047BB 0%, #0035a0 100%);
      color: white; font-size: 1rem; font-weight: 700; cursor: pointer; margin-top: 8px;
      display: flex; align-items: center; justify-content: center;
      box-shadow: 0 8px 24px rgba(0,71,187,0.25); transition: all 0.3s ease;
      font-family: 'Plus Jakarta Sans', sans-serif; letter-spacing: 0.3px;
      position: relative; overflow: hidden;
    }
    .btn-primary::before {
      content: ''; position: absolute; inset: 0;
      background: linear-gradient(135deg, transparent 0%, rgba(255,255,255,0.1) 50%, transparent 100%);
      transform: translateX(-100%); transition: transform 0.5s;
    }
    .btn-primary:hover:not(:disabled)::before { transform: translateX(100%); }
    .btn-primary:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 12px 32px rgba(0,71,187,0.35); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

    .spinner { display: flex; gap: 6px; }
    .dot { width: 8px; height: 8px; border-radius: 50%; background: white; animation: bounce 1.4s infinite ease-in-out both; }
    .dot1 { animation-delay: -0.32s; }
    .dot2 { animation-delay: -0.16s; }
    @keyframes bounce {
      0%, 80%, 100% { transform: scale(0); opacity: 0.5; }
      40% { transform: scale(1); opacity: 1; }
    }

    /* Error message */
    .error-msg {
      background: #FEF2F2; color: #DC2626; padding: 12px 16px;
      border-radius: 12px; margin-bottom: 16px; font-size: 0.85rem;
      font-weight: 500; display: flex; align-items: center; gap: 8px;
      border: 1px solid #FEE2E2;
    }
    .error-msg mat-icon { font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; }

    /* Link text */
    .link-text {
      text-align: center; margin-top: 20px; color: #9CA3AF; font-size: 0.88rem;
    }
    .link-text a { color: #0047BB; font-weight: 700; cursor: pointer; text-decoration: none; }
    .link-text a:hover { text-decoration: underline; }

    /* OTP */
    .otp-icon {
      width: 72px; height: 72px; border-radius: 20px;
      background: linear-gradient(135deg, rgba(0,71,187,0.1), rgba(0,71,187,0.05));
      display: flex; align-items: center; justify-content: center;
      margin: 0 auto 20px;
    }
    .otp-icon mat-icon { font-size: 36px; width: 36px; height: 36px; color: #0047BB; }

    .otp-container {
      display: flex; gap: 10px; justify-content: center; margin-bottom: 20px;
    }
    .otp-input {
      width: 52px; height: 60px; text-align: center; font-size: 1.4rem; font-weight: 800;
      border: 2px solid #E5E7EB; border-radius: 14px; background: #F9FAFB;
      color: #1A1A2E; font-family: 'Plus Jakarta Sans', sans-serif;
      transition: all 0.2s; outline: none;
    }
    .otp-input:focus { border-color: #0047BB; background: #fff; box-shadow: 0 0 0 4px rgba(0,71,187,0.08); }
    .otp-input:disabled { opacity: 0.5; }

    .timer-row { text-align: center; margin-bottom: 20px; }
    .timer {
      color: #9CA3AF; font-size: 0.85rem; display: inline-flex; align-items: center; gap: 4px;
    }
    .timer mat-icon { font-size: 16px; width: 16px; height: 16px; }
    .resend-link { color: #0047BB; font-weight: 700; font-size: 0.88rem; cursor: pointer; text-decoration: none; }
    .resend-link:hover { text-decoration: underline; }

    /* Success */
    .success-container { text-align: center; padding: 40px 0; }
    .success-icon {
      width: 88px; height: 88px; border-radius: 50%;
      background: linear-gradient(135deg, rgba(0,200,83,0.15), rgba(0,200,83,0.05));
      display: flex; align-items: center; justify-content: center;
      margin: 0 auto 24px;
    }
    .success-icon mat-icon { font-size: 48px; width: 48px; height: 48px; color: #00C853; }
    .success-container h2 { font-size: 1.4rem; font-weight: 800; color: #1A1A2E; margin: 0 0 12px; }
    .success-container p { color: #6B7280; font-size: 0.92rem; margin: 0 0 32px; line-height: 1.6; }

    .slide-down { animation: slideDown 0.3s ease; }
    @keyframes slideDown { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }
    .fade-in { animation: fadeIn 0.5s ease; }
    @keyframes fadeIn { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }

    /* Responsive */
    @media (max-width: 768px) {
      .register-wrapper { flex-direction: column; max-width: 460px; }
      .brand-panel { flex: 0 0 auto; padding: 28px 24px; }
      .features { display: none; }
      .brand-content { text-align: center; }
      .brand-content h1 { font-size: 1.6rem; }
      .tagline { margin-bottom: 0; }
      .logo-icon { margin: 0 auto 16px; }
      .form-panel { padding: 28px 24px; max-height: none; }
      .form-header h2 { font-size: 1.3rem; }
      .otp-input { width: 44px; height: 52px; font-size: 1.2rem; }
    }
    @media (max-width: 400px) {
      .register-bg { padding: 12px; }
      .form-panel { padding: 20px 16px; }
      .brand-panel { padding: 20px 16px; }
      .otp-container { gap: 6px; }
      .otp-input { width: 40px; height: 48px; font-size: 1.1rem; border-radius: 10px; }
    }
  `]
})
export class RegisterComponent {
  step = 1;
  fullName = '';
  email = '';
  cpf = '';
  password = '';
  confirmPassword = '';
  isLoading = false;
  errorMsg = '';
  showPass = false;
  showConfirm = false;
  passwordStrength = '';

  nameFocused = false;
  emailFocused = false;
  cpfFocused = false;
  passFocused = false;
  confirmFocused = false;

  otpDigits = ['', '', '', '', '', ''];
  timerSeconds = 1800; // 30 minutes
  private timerInterval: any;

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

  getCpfDigits(): string {
    return this.cpf.replace(/\D/g, '');
  }

  isEmailValid(): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email);
  }

  checkPasswordStrength(): void {
    const p = this.password;
    if (!p) { this.passwordStrength = ''; return; }
    let score = 0;
    if (p.length >= 8) score++;
    if (/[A-Z]/.test(p)) score++;
    if (/[0-9]/.test(p)) score++;
    if (/[^A-Za-z0-9]/.test(p)) score++;
    if (score <= 1) this.passwordStrength = 'fraca';
    else if (score <= 2) this.passwordStrength = 'media';
    else this.passwordStrength = 'forte';
  }

  getStrengthPercent(): number {
    this.checkPasswordStrength();
    if (this.passwordStrength === 'fraca') return 33;
    if (this.passwordStrength === 'media') return 66;
    if (this.passwordStrength === 'forte') return 100;
    return 0;
  }

  isFormValid(): boolean {
    return this.fullName.length >= 3
      && this.isEmailValid()
      && this.getCpfDigits().length === 11
      && this.password.length >= 8
      && /[A-Z]/.test(this.password)
      && /[0-9]/.test(this.password)
      && this.password === this.confirmPassword;
  }

  onRegister() {
    if (!this.isFormValid()) return;
    this.isLoading = true;
    this.errorMsg = '';
    this.auth.register({
      fullName: this.fullName,
      email: this.email,
      document: this.getCpfDigits(),
      password: this.password
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.step = 2;
        this.startTimer();
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMsg = err.error?.message || err.error?.error || 'Erro ao criar conta. Tente novamente.';
      }
    });
  }

  // OTP handlers
  onOtpInput(event: any, index: number) {
    const val = event.target.value.replace(/\D/g, '');
    this.otpDigits[index] = val ? val[0] : '';
    event.target.value = this.otpDigits[index];
    if (val && index < 5) {
      const next = document.getElementById('otp-' + (index + 1)) as HTMLInputElement;
      if (next) next.focus();
    }
  }

  onOtpKeydown(event: KeyboardEvent, index: number) {
    if (event.key === 'Backspace' && !this.otpDigits[index] && index > 0) {
      const prev = document.getElementById('otp-' + (index - 1)) as HTMLInputElement;
      if (prev) { prev.focus(); this.otpDigits[index - 1] = ''; prev.value = ''; }
    }
  }

  onOtpPaste(event: ClipboardEvent) {
    event.preventDefault();
    const paste = (event.clipboardData?.getData('text') || '').replace(/\D/g, '').slice(0, 6);
    for (let i = 0; i < 6; i++) {
      this.otpDigits[i] = paste[i] || '';
      const el = document.getElementById('otp-' + i) as HTMLInputElement;
      if (el) el.value = this.otpDigits[i];
    }
    if (paste.length > 0) {
      const focus = Math.min(paste.length, 5);
      const el = document.getElementById('otp-' + focus) as HTMLInputElement;
      if (el) el.focus();
    }
  }

  getOtpCode(): string {
    return this.otpDigits.join('');
  }

  onVerify() {
    const code = this.getOtpCode();
    if (code.length < 6) return;
    this.isLoading = true;
    this.errorMsg = '';
    this.auth.confirmEmail(this.email, code).subscribe({
      next: () => {
        this.isLoading = false;
        this.stopTimer();
        this.step = 3;
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMsg = err.error?.message || err.error?.error || 'Codigo invalido. Tente novamente.';
      }
    });
  }

  startTimer() {
    this.timerSeconds = 1800;
    this.timerInterval = setInterval(() => {
      this.timerSeconds--;
      if (this.timerSeconds <= 0) this.stopTimer();
    }, 1000);
  }

  stopTimer() {
    if (this.timerInterval) { clearInterval(this.timerInterval); this.timerInterval = null; }
  }

  formatTimer(): string {
    const m = Math.floor(this.timerSeconds / 60);
    const s = this.timerSeconds % 60;
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  }

  resendCode() {
    this.isLoading = true;
    this.errorMsg = '';
    this.auth.register({
      fullName: this.fullName,
      email: this.email,
      document: this.getCpfDigits(),
      password: this.password
    }).subscribe({
      next: () => { this.isLoading = false; this.startTimer(); },
      error: () => { this.isLoading = false; this.errorMsg = 'Erro ao reenviar codigo.'; }
    });
  }

  goToLogin() { this.router.navigate(['/login']); }
}
