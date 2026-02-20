import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-create-account',
  template: `
    <div class="register-bg">
      <div class="bg-shapes">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
      </div>

      <div class="register-wrapper fade-in">
        <!-- Left branding -->
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
            <p class="tagline">Abra sua conta em minutos</p>

            <!-- Stepper visual -->
            <div class="stepper">
              <div class="step" [class.active]="step >= 1" [class.done]="step > 1">
                <div class="step-circle">
                  <mat-icon *ngIf="step > 1">check</mat-icon>
                  <span *ngIf="step <= 1">1</span>
                </div>
                <span class="step-label">Dados pessoais</span>
              </div>
              <div class="step-line" [class.active]="step > 1"></div>
              <div class="step" [class.active]="step >= 2" [class.done]="step > 2">
                <div class="step-circle">
                  <mat-icon *ngIf="step > 2">check</mat-icon>
                  <span *ngIf="step <= 2">2</span>
                </div>
                <span class="step-label">Seguranca</span>
              </div>
              <div class="step-line" [class.active]="step > 2"></div>
              <div class="step" [class.active]="step >= 3">
                <div class="step-circle"><span>3</span></div>
                <span class="step-label">Confirmacao</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Right form -->
        <div class="form-panel">
          <button class="back-btn" (click)="handleBack()">
            <mat-icon>arrow_back</mat-icon>
            {{ step > 1 ? 'Voltar' : 'Login' }}
          </button>

          <div *ngIf="successMsg" class="success-box slide-down">
            <mat-icon>check_circle</mat-icon>
            {{ successMsg }}
          </div>
          <div *ngIf="errors.length > 0" class="error-box slide-down">
            <mat-icon>error_outline</mat-icon>
            <div class="error-list">
              <div *ngFor="let e of errors">{{ e }}</div>
            </div>
          </div>

          <!-- STEP 1: Dados pessoais -->
          <div *ngIf="step === 1" class="step-content slide-in">
            <div class="form-header">
              <h2>Seus dados</h2>
              <p>Informacoes basicas para abrir sua conta</p>
            </div>

            <div class="field">
              <label>Nome completo</label>
              <div class="input-wrap" [class.focused]="focus==='name'" [class.error]="model.customerName && model.customerName.length < 3 && model.customerName.length > 0">
                <mat-icon class="field-icon">person</mat-icon>
                <input type="text" [(ngModel)]="model.customerName" placeholder="Como esta no seu documento"
                       maxlength="100" [disabled]="isLoading"
                       (focus)="focus='name'" (blur)="focus=''">
              </div>
              <span class="hint" *ngIf="model.customerName && model.customerName.length < 3">Minimo 3 caracteres</span>
            </div>

            <div class="field">
              <label>CPF</label>
              <div class="input-wrap" [class.focused]="focus==='cpf'" [class.error]="model.customerDocument && !isValidCpf() && model.customerDocument.length > 3">
                <mat-icon class="field-icon">badge</mat-icon>
                <input type="text" [(ngModel)]="model.customerDocument" placeholder="000.000.000-00"
                       maxlength="14" (input)="maskCpf($event)" [disabled]="isLoading"
                       (focus)="focus='cpf'" (blur)="focus=''">
                <mat-icon class="check-icon" *ngIf="isValidCpf()">check_circle</mat-icon>
              </div>
            </div>

            <div class="field">
              <label>Email</label>
              <div class="input-wrap" [class.focused]="focus==='email'" [class.error]="model.customerEmail && !isValidEmail() && model.customerEmail.length > 3">
                <mat-icon class="field-icon">email</mat-icon>
                <input type="email" [(ngModel)]="model.customerEmail" placeholder="seu@email.com"
                       [disabled]="isLoading"
                       (focus)="focus='email'" (blur)="focus=''">
                <mat-icon class="check-icon" *ngIf="isValidEmail()">check_circle</mat-icon>
              </div>
            </div>

            <div class="field">
              <label>Telefone</label>
              <div class="input-wrap" [class.focused]="focus==='phone'" [class.error]="model.customerPhone && !isValidPhone() && model.customerPhone.length > 3">
                <mat-icon class="field-icon">phone</mat-icon>
                <input type="text" [(ngModel)]="model.customerPhone" placeholder="(00) 00000-0000"
                       maxlength="15" (input)="maskPhone($event)" [disabled]="isLoading"
                       (focus)="focus='phone'" (blur)="focus=''">
                <mat-icon class="check-icon" *ngIf="isValidPhone()">check_circle</mat-icon>
              </div>
            </div>

            <button class="btn-primary" (click)="nextStep()"
                    [disabled]="!isStep1Valid()">
              Continuar
              <mat-icon>arrow_forward</mat-icon>
            </button>
          </div>

          <!-- STEP 2: Seguranca -->
          <div *ngIf="step === 2" class="step-content slide-in">
            <div class="form-header">
              <h2>Crie sua senha</h2>
              <p>Escolha uma senha forte para proteger sua conta</p>
            </div>

            <div class="field">
              <label>Senha</label>
              <div class="input-wrap" [class.focused]="focus==='pass'">
                <mat-icon class="field-icon">lock</mat-icon>
                <input [type]="showPass ? 'text' : 'password'" [(ngModel)]="model.password"
                       placeholder="Minimo 6 caracteres" [disabled]="isLoading"
                       (focus)="focus='pass'" (blur)="focus=''">
                <mat-icon class="toggle-pass" (click)="showPass = !showPass">
                  {{ showPass ? 'visibility_off' : 'visibility' }}
                </mat-icon>
              </div>
            </div>

            <!-- Password strength -->
            <div class="strength-bar" *ngIf="model.password">
              <div class="bar">
                <div class="fill" [style.width]="getStrengthPercent() + '%'"
                     [class.weak]="getStrength() === 'weak'"
                     [class.medium]="getStrength() === 'medium'"
                     [class.strong]="getStrength() === 'strong'"></div>
              </div>
              <span class="strength-label"
                    [class.weak]="getStrength() === 'weak'"
                    [class.medium]="getStrength() === 'medium'"
                    [class.strong]="getStrength() === 'strong'">
                {{ getStrengthLabel() }}
              </span>
            </div>

            <div class="field">
              <label>Confirmar senha</label>
              <div class="input-wrap" [class.focused]="focus==='confirm'"
                   [class.error]="confirmPassword && confirmPassword !== model.password">
                <mat-icon class="field-icon">lock_reset</mat-icon>
                <input [type]="showPass ? 'text' : 'password'" [(ngModel)]="confirmPassword"
                       placeholder="Repita a senha" [disabled]="isLoading"
                       (focus)="focus='confirm'" (blur)="focus=''">
                <mat-icon class="check-icon" *ngIf="confirmPassword && confirmPassword === model.password">check_circle</mat-icon>
              </div>
              <span class="hint error" *ngIf="confirmPassword && confirmPassword !== model.password">Senhas nao conferem</span>
            </div>

            <button class="btn-primary" (click)="nextStep()"
                    [disabled]="!isStep2Valid()">
              Revisar dados
              <mat-icon>arrow_forward</mat-icon>
            </button>
          </div>

          <!-- STEP 3: Confirmacao -->
          <div *ngIf="step === 3" class="step-content slide-in">
            <div class="form-header">
              <h2>Confirme seus dados</h2>
              <p>Revise antes de criar sua conta</p>
            </div>

            <div class="review-card">
              <div class="review-row">
                <span class="review-label">Nome</span>
                <span class="review-value">{{ model.customerName }}</span>
              </div>
              <div class="review-row">
                <span class="review-label">CPF</span>
                <span class="review-value">{{ model.customerDocument }}</span>
              </div>
              <div class="review-row">
                <span class="review-label">Email</span>
                <span class="review-value">{{ model.customerEmail }}</span>
              </div>
              <div class="review-row">
                <span class="review-label">Telefone</span>
                <span class="review-value">{{ model.customerPhone }}</span>
              </div>
              <div class="review-row">
                <span class="review-label">Senha</span>
                <span class="review-value">••••••••</span>
              </div>
            </div>

            <button class="btn-primary btn-success" (click)="submit()"
                    [disabled]="isLoading" [class.loading]="isLoading">
              <span *ngIf="!isLoading">Criar minha conta</span>
              <div class="spinner" *ngIf="isLoading">
                <div class="dot dot1"></div>
                <div class="dot dot2"></div>
                <div class="dot dot3"></div>
              </div>
            </button>
          </div>

          <p class="login-link" *ngIf="step === 1">
            Ja tem conta? <a (click)="goToLogin()">Fazer login</a>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    /* === BG === */
    .register-bg {
      min-height: 100vh; display: flex; align-items: center; justify-content: center;
      background: linear-gradient(135deg, #0a1628 0%, #0d2137 30%, #0a2a4a 60%, #071e3d 100%);
      padding: 20px; position: relative; overflow: hidden;
    }
    .bg-shapes { position: absolute; inset: 0; pointer-events: none; }
    .shape { position: absolute; border-radius: 50%; animation: float 20s infinite ease-in-out; }
    .shape-1 { width: 500px; height: 500px; top: -150px; right: -100px;
      background: radial-gradient(circle, rgba(0,100,255,0.15), transparent 70%); }
    .shape-2 { width: 400px; height: 400px; bottom: -100px; left: -80px; animation-delay: -7s;
      background: radial-gradient(circle, rgba(0,200,150,0.1), transparent 70%); }
    .shape-3 { width: 300px; height: 300px; top: 40%; left: 25%; animation-delay: -12s;
      background: radial-gradient(circle, rgba(100,100,255,0.08), transparent 70%); }
    @keyframes float {
      0%, 100% { transform: translate(0, 0) scale(1); }
      25% { transform: translate(30px, -40px) scale(1.05); }
      50% { transform: translate(-20px, 20px) scale(0.95); }
      75% { transform: translate(40px, 30px) scale(1.02); }
    }

    /* === WRAPPER === */
    .register-wrapper {
      display: flex; width: 100%; max-width: 920px;
      border-radius: 28px; overflow: hidden;
      box-shadow: 0 32px 64px rgba(0,0,0,0.4), 0 0 0 1px rgba(255,255,255,0.05);
      position: relative; z-index: 1;
    }

    /* === BRAND === */
    .brand-panel {
      flex: 0 0 300px; padding: 48px 32px;
      background: linear-gradient(135deg, #0047BB 0%, #002a70 50%, #001a4d 100%);
      display: flex; align-items: flex-start; justify-content: center;
      padding-top: 64px; position: relative; overflow: hidden;
    }
    .brand-panel::after {
      content: ''; position: absolute; inset: 0;
      background: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.03'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
    }
    .brand-content { position: relative; z-index: 1; color: white; }
    .logo-icon {
      width: 64px; height: 64px; border-radius: 18px;
      background: rgba(255,255,255,0.15); backdrop-filter: blur(10px);
      display: flex; align-items: center; justify-content: center;
      margin-bottom: 16px; border: 1px solid rgba(255,255,255,0.2);
    }
    .brand-content h1 { font-size: 1.7rem; font-weight: 800; margin: 0 0 4px; }
    .tagline { opacity: 0.7; font-size: 0.9rem; margin: 0 0 40px; }

    /* Stepper */
    .stepper { display: flex; flex-direction: column; gap: 0; align-items: flex-start; }
    .step { display: flex; align-items: center; gap: 12px; }
    .step-circle {
      width: 32px; height: 32px; border-radius: 50%;
      background: rgba(255,255,255,0.15); border: 2px solid rgba(255,255,255,0.3);
      display: flex; align-items: center; justify-content: center;
      font-size: 0.85rem; font-weight: 700; color: rgba(255,255,255,0.5);
      transition: all 0.3s;
    }
    .step.active .step-circle {
      background: white; color: #0047BB; border-color: white;
    }
    .step.done .step-circle { background: #22C55E; border-color: #22C55E; color: white; }
    .step.done .step-circle mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .step-label { font-size: 0.85rem; color: rgba(255,255,255,0.5); font-weight: 500; }
    .step.active .step-label { color: white; font-weight: 700; }
    .step-line {
      width: 2px; height: 24px; background: rgba(255,255,255,0.15);
      margin-left: 15px; transition: background 0.3s;
    }
    .step-line.active { background: #22C55E; }

    /* === FORM === */
    .form-panel {
      flex: 1; padding: 32px 40px; background: #ffffff;
      display: flex; flex-direction: column; min-height: 580px;
    }
    .back-btn {
      display: flex; align-items: center; gap: 4px;
      background: none; border: none; color: #9CA3AF; cursor: pointer;
      font-size: 0.85rem; font-weight: 500; padding: 0; margin-bottom: 20px;
      font-family: 'Plus Jakarta Sans', sans-serif; transition: color 0.2s;
    }
    .back-btn:hover { color: #0047BB; }
    .back-btn mat-icon { font-size: 20px; width: 20px; height: 20px; }

    .form-header h2 { font-size: 1.5rem; font-weight: 800; color: #1A1A2E; margin: 0 0 4px; }
    .form-header p { color: #9CA3AF; font-size: 0.9rem; margin: 0 0 28px; }

    /* Fields */
    .field { margin-bottom: 20px; }
    .field label {
      display: block; font-size: 0.78rem; font-weight: 700;
      color: #6B7280; margin-bottom: 7px; text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .input-wrap {
      display: flex; align-items: center; gap: 12px;
      border: 2px solid #E5E7EB; border-radius: 14px;
      padding: 0 16px; height: 52px;
      transition: all 0.3s; background: #F9FAFB;
    }
    .input-wrap.focused { border-color: #0047BB; background: #fff; box-shadow: 0 0 0 4px rgba(0,71,187,0.08); }
    .input-wrap.error { border-color: #EF4444; box-shadow: 0 0 0 4px rgba(239,68,68,0.08); }
    .input-wrap input {
      flex: 1; border: none; outline: none; background: transparent;
      font-size: 0.95rem; font-family: 'Plus Jakarta Sans', sans-serif; color: #1A1A2E;
    }
    .input-wrap input::placeholder { color: #C4C9D4; }
    .field-icon { color: #9CA3AF; font-size: 20px; width: 20px; height: 20px; }
    .input-wrap.focused .field-icon { color: #0047BB; }
    .check-icon { color: #22C55E !important; font-size: 20px; width: 20px; height: 20px; }
    .toggle-pass { cursor: pointer; color: #9CA3AF; font-size: 20px; width: 20px; height: 20px; }
    .hint { font-size: 0.78rem; color: #9CA3AF; margin-top: 4px; display: block; }
    .hint.error { color: #EF4444; }

    /* Strength bar */
    .strength-bar { margin: -8px 0 20px; display: flex; align-items: center; gap: 10px; }
    .bar { flex: 1; height: 4px; background: #E5E7EB; border-radius: 2px; overflow: hidden; }
    .fill { height: 100%; border-radius: 2px; transition: all 0.4s; }
    .fill.weak { background: #EF4444; }
    .fill.medium { background: #F59E0B; }
    .fill.strong { background: #22C55E; }
    .strength-label { font-size: 0.75rem; font-weight: 700; min-width: 50px; }
    .strength-label.weak { color: #EF4444; }
    .strength-label.medium { color: #F59E0B; }
    .strength-label.strong { color: #22C55E; }

    /* Review card */
    .review-card {
      background: #F9FAFB; border-radius: 16px; padding: 20px;
      border: 1px solid #E5E7EB; margin-bottom: 24px;
    }
    .review-row {
      display: flex; justify-content: space-between; padding: 10px 0;
      border-bottom: 1px solid #F0F0F0;
    }
    .review-row:last-child { border-bottom: none; }
    .review-label { font-size: 0.85rem; color: #9CA3AF; }
    .review-value { font-size: 0.88rem; color: #1A1A2E; font-weight: 600; }

    /* Buttons */
    .btn-primary {
      width: 100%; height: 52px; border: none; border-radius: 14px;
      background: linear-gradient(135deg, #0047BB 0%, #0035a0 100%);
      color: white; font-size: 0.95rem; font-weight: 700;
      cursor: pointer; margin-top: 8px;
      display: flex; align-items: center; justify-content: center; gap: 8px;
      box-shadow: 0 8px 24px rgba(0,71,187,0.25);
      transition: all 0.3s; font-family: 'Plus Jakarta Sans', sans-serif;
      position: relative; overflow: hidden;
    }
    .btn-primary mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .btn-primary:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 12px 32px rgba(0,71,187,0.35); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-success { background: linear-gradient(135deg, #16A34A 0%, #15803d 100%); box-shadow: 0 8px 24px rgba(22,163,74,0.3); }
    .btn-success:hover:not(:disabled) { box-shadow: 0 12px 32px rgba(22,163,74,0.4); }

    .spinner { display: flex; gap: 6px; }
    .dot { width: 8px; height: 8px; border-radius: 50%; background: white; animation: bounce 1.4s infinite ease-in-out both; }
    .dot1 { animation-delay: -0.32s; } .dot2 { animation-delay: -0.16s; }
    @keyframes bounce { 0%, 80%, 100% { transform: scale(0); } 40% { transform: scale(1); } }

    /* Messages */
    .error-box {
      background: #FEF2F2; color: #DC2626; padding: 14px 16px;
      border-radius: 12px; margin-bottom: 16px; font-size: 0.85rem;
      display: flex; align-items: flex-start; gap: 8px; border: 1px solid #FEE2E2;
    }
    .error-box mat-icon { font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; margin-top: 1px; }
    .success-box {
      background: #F0FDF4; color: #16A34A; padding: 14px 16px;
      border-radius: 12px; margin-bottom: 16px; font-size: 0.9rem; font-weight: 600;
      display: flex; align-items: center; gap: 8px; border: 1px solid #BBF7D0;
    }
    .success-box mat-icon { font-size: 20px; width: 20px; height: 20px; }

    .login-link { text-align: center; margin-top: 20px; color: #9CA3AF; font-size: 0.88rem; }
    .login-link a { color: #0047BB; cursor: pointer; font-weight: 600; text-decoration: none; }
    .login-link a:hover { text-decoration: underline; }

    /* Animations */
    .fade-in { animation: fadeIn 0.6s ease; }
    @keyframes fadeIn { from { opacity: 0; transform: translateY(16px); } to { opacity: 1; transform: translateY(0); } }
    .slide-in { animation: slideIn 0.35s ease; }
    @keyframes slideIn { from { opacity: 0; transform: translateX(20px); } to { opacity: 1; transform: translateX(0); } }
    .slide-down { animation: slideDown 0.3s ease; }
    @keyframes slideDown { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }

    /* Responsive */
    @media (max-width: 768px) {
      .register-wrapper { flex-direction: column; max-width: 440px; }
      .brand-panel { flex: 0 0 auto; padding: 28px 24px; }
      .brand-content { text-align: center; }
      .brand-content h1 { font-size: 1.4rem; }
      .logo-icon { margin: 0 auto 12px; }
      .tagline { margin-bottom: 20px; }
      .stepper { flex-direction: row; align-items: center; justify-content: center; width: 100%; }
      .step-label { display: none; }
      .step-line { width: 40px; height: 2px; margin: 0 4px; }
      .form-panel { padding: 24px 24px; min-height: auto; }
    }
  `]
})
export class CreateAccountComponent {
  step = 1;
  model: any = { customerName: '', customerDocument: '', customerEmail: '', customerPhone: '', password: '' };
  confirmPassword = '';
  showPass = false;
  isLoading = false;
  errors: string[] = [];
  successMsg = '';
  focus = '';

  constructor(private location: Location, private router: Router, private auth: AuthService) {}

  // === Navigation ===
  nextStep() {
    this.errors = [];
    if (this.step === 1 && this.isStep1Valid()) this.step = 2;
    else if (this.step === 2 && this.isStep2Valid()) this.step = 3;
  }

  handleBack() {
    if (this.step > 1) { this.step--; this.errors = []; }
    else this.router.navigate(['/login']);
  }

  // === Validations ===
  isStep1Valid(): boolean {
    return this.model.customerName.length >= 3 && this.isValidCpf()
           && this.isValidEmail() && this.isValidPhone();
  }

  isStep2Valid(): boolean {
    return this.model.password.length >= 6 && this.confirmPassword === this.model.password;
  }

  isValidCpf(): boolean {
    const cpf = this.model.customerDocument.replace(/\D/g, '');
    if (cpf.length !== 11) return false;
    if (/^(\d)\1{10}$/.test(cpf)) return false;
    let sum = 0;
    for (let i = 0; i < 9; i++) sum += parseInt(cpf[i]) * (10 - i);
    let d1 = sum % 11 < 2 ? 0 : 11 - (sum % 11);
    if (parseInt(cpf[9]) !== d1) return false;
    sum = 0;
    for (let i = 0; i < 10; i++) sum += parseInt(cpf[i]) * (11 - i);
    let d2 = sum % 11 < 2 ? 0 : 11 - (sum % 11);
    return parseInt(cpf[10]) === d2;
  }

  isValidEmail(): boolean { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.model.customerEmail); }

  isValidPhone(): boolean {
    const d = this.model.customerPhone.replace(/\D/g, '');
    return d.length === 10 || d.length === 11;
  }

  // === Password strength ===
  getStrength(): string {
    const p = this.model.password;
    if (p.length < 6) return 'weak';
    let score = 0;
    if (p.length >= 8) score++;
    if (/[A-Z]/.test(p)) score++;
    if (/[0-9]/.test(p)) score++;
    if (/[^A-Za-z0-9]/.test(p)) score++;
    if (score >= 3) return 'strong';
    if (score >= 2) return 'medium';
    return 'weak';
  }

  getStrengthPercent(): number {
    const s = this.getStrength();
    return s === 'strong' ? 100 : s === 'medium' ? 60 : 30;
  }

  getStrengthLabel(): string {
    const s = this.getStrength();
    return s === 'strong' ? 'Forte' : s === 'medium' ? 'Media' : 'Fraca';
  }

  // === Masks ===
  maskCpf(event: any) {
    let v = event.target.value.replace(/\D/g, '');
    if (v.length > 11) v = v.slice(0, 11);
    if (v.length > 9) v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{1,2})/, '$1.$2.$3-$4');
    else if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d{1,3})/, '$1.$2.$3');
    else if (v.length > 3) v = v.replace(/(\d{3})(\d{1,3})/, '$1.$2');
    this.model.customerDocument = v;
    event.target.value = v;
  }

  maskPhone(event: any) {
    let v = event.target.value.replace(/\D/g, '');
    if (v.length > 11) v = v.slice(0, 11);
    if (v.length > 6) v = v.replace(/(\d{2})(\d{5})(\d{1,4})/, '($1) $2-$3');
    else if (v.length > 2) v = v.replace(/(\d{2})(\d{1,5})/, '($1) $2');
    else if (v.length > 0) v = v.replace(/(\d{1,2})/, '($1');
    this.model.customerPhone = v;
    event.target.value = v;
  }

  // === Submit ===
  submit() {
    this.errors = [];
    this.successMsg = '';
    this.isLoading = true;

    this.auth.register(this.model).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res.success) {
          this.successMsg = 'Conta criada com sucesso! Redirecionando...';
          setTimeout(() => this.router.navigate(['/login']), 2000);
        } else {
          this.errors = res.errors || ['Erro ao criar conta'];
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errors = err.error?.errors || [err.error?.error || 'Erro ao criar conta. Tente novamente.'];
      }
    });
  }

  goToLogin() { this.router.navigate(['/login']); }
}
