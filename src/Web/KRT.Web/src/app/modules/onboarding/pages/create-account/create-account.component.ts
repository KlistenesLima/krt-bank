import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';

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

        <div class="form-content">
            <mat-form-field appearance="outline" class="full">
              <mat-label>Nome Completo</mat-label>
              <input matInput [(ngModel)]="model.customerName" name="name">
              <mat-icon matSuffix>person</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full">
              <mat-label>CPF</mat-label>
              <input matInput [(ngModel)]="model.customerDocument" name="doc" placeholder="000.000.000-00">
              <mat-icon matSuffix>badge</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full">
              <mat-label>Email</mat-label>
              <input matInput [(ngModel)]="model.customerEmail" name="email" placeholder="seu@email.com">
              <mat-icon matSuffix>email</mat-icon>
            </mat-form-field>
            
            <mat-form-field appearance="outline" class="full">
              <mat-label>Senha</mat-label>
              <input matInput type="password" [(ngModel)]="model.password" name="pass">
              <mat-icon matSuffix>lock</mat-icon>
            </mat-form-field>

            <button mat-raised-button color="primary" class="submit-btn" (click)="submit()">
               CONFIRMAR CADASTRO
            </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .full-screen-center {
        min-height: 100vh; display: flex; align-items: center; justify-content: center;
        background: linear-gradient(135deg, #0047BB 0%, #002a70 100%); /* Fundo Azul KRT */
        padding: 20px;
    }
    .auth-card { 
      background: white; width: 100%; max-width: 450px; 
      padding: 20px; border-radius: 24px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.3);
    }
    .header-simple { 
        display: flex; justify-content: space-between; align-items: center; 
        margin-bottom: 30px; border-bottom: 1px solid #eee; padding-bottom: 15px;
    }
    .header-simple h1 { margin: 0; font-size: 1.2rem; color: #333; }
    
    .full { width: 100%; margin-bottom: 5px; }
    .submit-btn { width: 100%; padding: 25px !important; font-size: 1rem; margin-top: 10px; }
  `]
})
export class CreateAccountComponent {
  model: any = {};
  constructor(private location: Location, private router: Router) {}

  submit() {
      // Simulação de cadastro
      setTimeout(() => {
          localStorage.setItem('krt_account_id', 'new-user');
          this.router.navigate(['/dashboard']);
      }, 1000);
  }

  back() { this.location.back(); }
}
