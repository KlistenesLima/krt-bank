import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-create-account',
  template: `
    <div class="full-screen-center">
      <div class="auth-card fade-in">
        <header class="header-simple">
            <button mat-icon-button (click)="back()"><mat-icon>arrow_back</mat-icon></button>
            <h1>Criar Conta</h1>
            <div style="width: 40px"></div>
        </header>

        <div *ngIf="successMsg" class="success-box">{{ successMsg }}</div>
        <div *ngIf="errors.length > 0" class="error-box">
            <div *ngFor="let e of errors">• {{ e }}</div>
        </div>

        <div class="form-content">
            <!-- NOME -->
            <mat-form-field appearance="outline" class="full">
              <mat-label>Nome Completo</mat-label>
              <input matInput [(ngModel)]="model.customerName" name="name" maxlength="100"
                     [disabled]="isLoading">
              <mat-icon matSuffix>person</mat-icon>
              <mat-hint *ngIf="model.customerName && model.customerName.length < 3">Mínimo 3 caracteres</mat-hint>
            </mat-form-field>

            <!-- CPF -->
            <mat-form-field appearance="outline" class="full">
              <mat-label>CPF</mat-label>
              <input matInput [(ngModel)]="model.customerDocument" name="doc"
                     placeholder="000.000.000-00" maxlength="14" (input)="maskCpf($event)"
                     [disabled]="isLoading">
              <mat-icon matSuffix>badge</mat-icon>
              <mat-hint *ngIf="model.customerDocument && !isValidCpf()">CPF inválido</mat-hint>
            </mat-form-field>

            <!-- EMAIL -->
            <mat-form-field appearance="outline" class="full">
              <mat-label>Email</mat-label>
              <input matInput [(ngModel)]="model.customerEmail" name="email"
                     placeholder="seu@email.com" type="email" [disabled]="isLoading">
              <mat-icon matSuffix>email</mat-icon>
              <mat-hint *ngIf="model.customerEmail && !isValidEmail()">Email inválido</mat-hint>
            </mat-form-field>

            <!-- TELEFONE -->
            <mat-form-field appearance="outline" class="full">
              <mat-label>Telefone</mat-label>
              <input matInput [(ngModel)]="model.customerPhone" name="phone"
                     placeholder="(00) 00000-0000" maxlength="15" (input)="maskPhone($event)"
                     [disabled]="isLoading">
              <mat-icon matSuffix>phone</mat-icon>
              <mat-hint *ngIf="model.customerPhone && !isValidPhone()">Formato: (XX) XXXXX-XXXX</mat-hint>
            </mat-form-field>

            <!-- SENHA -->
            <mat-form-field appearance="outline" class="full">
              <mat-label>Senha</mat-label>
              <input matInput [type]="showPass ? 'text' : 'password'"
                     [(ngModel)]="model.password" name="pass" [disabled]="isLoading">
              <button mat-icon-button matSuffix type="button" (click)="showPass = !showPass">
                <mat-icon>{{ showPass ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
              <mat-hint *ngIf="model.password && model.password.length < 6">Mínimo 6 caracteres</mat-hint>
            </mat-form-field>

            <!-- CONFIRMAR SENHA -->
            <mat-form-field appearance="outline" class="full">
              <mat-label>Confirmar Senha</mat-label>
              <input matInput [type]="showPass ? 'text' : 'password'"
                     [(ngModel)]="confirmPassword" name="confirmPass" [disabled]="isLoading">
              <mat-icon matSuffix>lock</mat-icon>
              <mat-hint *ngIf="confirmPassword && confirmPassword !== model.password">Senhas não conferem</mat-hint>
            </mat-form-field>

            <button mat-raised-button color="primary" class="submit-btn"
                    (click)="submit()" [disabled]="isLoading || !isFormValid()">
              <span *ngIf="!isLoading">CONFIRMAR CADASTRO</span>
              <mat-spinner diameter="24" *ngIf="isLoading" color="accent"></mat-spinner>
            </button>

            <p class="login-link">
              Já tem conta? <a (click)="goToLogin()">Fazer login</a>
            </p>
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
      background: white; width: 100%; max-width: 450px;
      padding: 20px; border-radius: 24px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.3);
    }
    .header-simple {
        display: flex; justify-content: space-between; align-items: center;
        margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 15px;
    }
    .header-simple h1 { margin: 0; font-size: 1.2rem; color: #333; }
    .full { width: 100%; margin-bottom: 5px; }
    .submit-btn {
      width: 100%; padding: 16px !important; font-size: 1rem; font-weight: 600;
      margin-top: 10px; display: flex; justify-content: center; align-items: center;
      background: linear-gradient(135deg, #0052D4 0%, #0047BB 100%) !important;
      color: white !important; border: none; border-radius: 12px; cursor: pointer;
      letter-spacing: 0.5px; text-transform: uppercase;
    }
    .error-box { background: #FFEBEE; color: #C62828; padding: 10px 16px; border-radius: 8px; margin-bottom: 16px; font-size: 0.85rem; }
    .success-box { background: #E8F5E9; color: #2E7D32; padding: 10px 16px; border-radius: 8px; margin-bottom: 16px; font-size: 0.9rem; }
    .login-link { text-align: center; margin-top: 16px; color: #666; }
    .login-link a { color: #0047BB; cursor: pointer; font-weight: 500; text-decoration: underline; }
    ::ng-deep .mat-mdc-progress-spinner circle { stroke: white !important; }
  `]
})
export class CreateAccountComponent {
  model: any = {
    customerName: '',
    customerDocument: '',
    customerEmail: '',
    customerPhone: '',
    password: ''
  };
  confirmPassword = '';
  showPass = false;
  isLoading = false;
  errors: string[] = [];
  successMsg = '';

  constructor(
    private location: Location,
    private router: Router,
    private auth: AuthService
  ) {}

  // === MÁSCARA CPF: 000.000.000-00 ===
  maskCpf(event: any) {
    let v = event.target.value.replace(/\D/g, '');
    if (v.length > 11) v = v.slice(0, 11);
    if (v.length > 9) v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{1,2})/, '$1.$2.$3-$4');
    else if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d{1,3})/, '$1.$2.$3');
    else if (v.length > 3) v = v.replace(/(\d{3})(\d{1,3})/, '$1.$2');
    this.model.customerDocument = v;
    event.target.value = v;
  }

  // === MÁSCARA TELEFONE: (00) 00000-0000 ===
  maskPhone(event: any) {
    let v = event.target.value.replace(/\D/g, '');
    if (v.length > 11) v = v.slice(0, 11);
    if (v.length > 6) v = v.replace(/(\d{2})(\d{5})(\d{1,4})/, '($1) $2-$3');
    else if (v.length > 2) v = v.replace(/(\d{2})(\d{1,5})/, '($1) $2');
    else if (v.length > 0) v = v.replace(/(\d{1,2})/, '($1');
    this.model.customerPhone = v;
    event.target.value = v;
  }

  // === VALIDAÇÃO CPF (dígitos verificadores) ===
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

  isValidEmail(): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.model.customerEmail);
  }

  isValidPhone(): boolean {
    const digits = this.model.customerPhone.replace(/\D/g, '');
    return digits.length === 10 || digits.length === 11;
  }

  isFormValid(): boolean {
    return this.model.customerName.length >= 3
        && this.isValidCpf()
        && this.isValidEmail()
        && this.isValidPhone()
        && this.model.password.length >= 6
        && this.confirmPassword === this.model.password;
  }

  submit() {
    this.errors = [];
    this.successMsg = '';

    // Validação frontend
    if (!this.isFormValid()) {
      if (this.model.customerName.length < 3) this.errors.push('Nome deve ter no mínimo 3 caracteres');
      if (!this.isValidCpf()) this.errors.push('CPF inválido');
      if (!this.isValidEmail()) this.errors.push('Email inválido');
      if (!this.isValidPhone()) this.errors.push('Telefone inválido');
      if (this.model.password.length < 6) this.errors.push('Senha deve ter no mínimo 6 caracteres');
      if (this.confirmPassword !== this.model.password) this.errors.push('Senhas não conferem');
      return;
    }

    this.isLoading = true;

    this.auth.register(this.model).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res.success) {
          this.successMsg = 'Conta criada com sucesso! Redirecionando para login...';
          setTimeout(() => this.router.navigate(['/login']), 2000);
        } else {
          this.errors = res.errors || ['Erro ao criar conta'];
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        if (err.error?.errors) {
          this.errors = err.error.errors;
        } else {
          this.errors = [err.error?.error || 'Erro ao criar conta. Tente novamente.'];
        }
      }
    });
  }

  goToLogin() { this.router.navigate(['/login']); }
  back() { this.location.back(); }
}


