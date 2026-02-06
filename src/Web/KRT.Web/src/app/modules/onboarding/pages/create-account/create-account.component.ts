import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { AccountService } from '../../../../core/services/account.service';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';

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

        <div *ngIf="errorMsg" class="error-banner">
          <mat-icon>error_outline</mat-icon> {{ errorMsg }}
        </div>

        <div *ngIf="createdId" class="success-banner">
          <mat-icon>check_circle</mat-icon>
          <div>
            <strong>Conta criada!</strong><br>
            Seu ID: <code>{{ createdId }}</code><br>
            <small>Copie este ID para fazer login.</small>
          </div>
        </div>

        <div class="form-content" *ngIf="!createdId">
            <mat-form-field appearance="outline" class="full">
              <mat-label>Nome Completo</mat-label>
              <input matInput [(ngModel)]="model.customerName" name="name">
              <mat-icon matSuffix>person</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full">
              <mat-label>CPF</mat-label>
              <input matInput [(ngModel)]="model.customerDocument" name="doc" placeholder="00000000000">
              <mat-icon matSuffix>badge</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full">
              <mat-label>Email</mat-label>
              <input matInput [(ngModel)]="model.customerEmail" name="email" placeholder="seu@email.com">
              <mat-icon matSuffix>email</mat-icon>
            </mat-form-field>

            <button mat-raised-button color="primary" class="submit-btn" 
                    (click)="submit()" [disabled]="isLoading">
               <span *ngIf="!isLoading">CONFIRMAR CADASTRO</span>
               <mat-spinner diameter="24" *ngIf="isLoading"></mat-spinner>
            </button>
        </div>

        <div *ngIf="createdId" class="actions-after">
            <button mat-raised-button color="primary" class="submit-btn" (click)="goToLogin()">
              IR PARA LOGIN
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
      background: white; width: 100%; max-width: 450px; 
      padding: 20px; border-radius: 24px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.3);
    }
    .header-simple { 
        display: flex; justify-content: space-between; align-items: center; 
        margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 15px;
    }
    .header-simple h1 { margin: 0; font-size: 1.2rem; color: #333; }
    .error-banner { 
      background: #ffebee; color: #c62828; padding: 10px 15px; border-radius: 8px; 
      margin-bottom: 15px; display: flex; align-items: center; gap: 8px; font-size: 0.9rem;
    }
    .success-banner {
      background: #e8f5e9; color: #2e7d32; padding: 15px; border-radius: 12px;
      display: flex; align-items: flex-start; gap: 10px; margin-bottom: 20px;
    }
    .success-banner code { 
      background: #c8e6c9; padding: 4px 8px; border-radius: 4px; 
      font-size: 0.8rem; word-break: break-all; 
    }
    .full { width: 100%; margin-bottom: 5px; }
    .submit-btn { width: 100%; padding: 25px !important; font-size: 1rem; margin-top: 10px; }
    .actions-after { margin-top: 10px; }
    ::ng-deep .mat-mdc-progress-spinner circle { stroke: white !important; }
  `]
})
export class CreateAccountComponent {
  model: any = { customerName: '', customerDocument: '', customerEmail: '' };
  isLoading = false;
  errorMsg = '';
  createdId = '';

  constructor(
    private location: Location,
    private router: Router,
    private accountService: AccountService,
    private authService: AuthService,
    private notify: NotificationService
  ) {}

  submit() {
    if (!this.model.customerName || !this.model.customerDocument || !this.model.customerEmail) {
      this.errorMsg = 'Preencha todos os campos.';
      return;
    }

    this.isLoading = true;
    this.errorMsg = '';

    this.accountService.create({
      customerName: this.model.customerName,
      customerDocument: this.model.customerDocument,
      customerEmail: this.model.customerEmail,
      branchCode: '0001'
    }).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        // O backend retorna { success: true, id: "guid" } ou ApiResponse<{id}>
        const id = res?.data?.id || res?.id;
        if (id) {
          this.createdId = id;
          this.notify.success('Conta criada com sucesso!');
        } else {
          this.errorMsg = 'Resposta inesperada do servidor.';
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        const errors = err.error?.errors;
        this.errorMsg = Array.isArray(errors) ? errors.join('. ') : 'Erro ao criar conta.';
      }
    });
  }

  goToLogin() { this.router.navigate(['/login']); }
  back() { this.location.back(); }
}
