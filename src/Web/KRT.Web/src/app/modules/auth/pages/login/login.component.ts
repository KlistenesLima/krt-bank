import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  template: `
    <div class="full-screen-center">
      <mat-card class="auth-card fade-in">
        
        <div class="brand-header">
           <div class="logo-icon">
             <mat-icon>account_balance_wallet</mat-icon>
           </div>
           <h2>KRT Bank</h2>
           <p>Banking Reimagined</p>
        </div>
        
        <mat-card-content>
          <form (ngSubmit)="login()">
            
            <mat-form-field appearance="fill">
              <mat-label>CPF</mat-label>
              <input matInput placeholder="000.000.000-00" [(ngModel)]="cpf" name="cpf">
              <mat-icon matSuffix>badge</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="fill">
              <mat-label>Senha</mat-label>
              <input matInput type="password" [(ngModel)]="password" name="password">
              <mat-icon matSuffix>lock</mat-icon>
            </mat-form-field>
            
            <button mat-raised-button color="primary" class="full-width-btn" type="submit">
              ACESSAR MINHA CONTA
            </button>
          </form>
        </mat-card-content>
        
        <mat-card-actions class="actions">
          <span class="divider-text">Novo por aqui?</span>
          <button mat-stroked-button color="primary" (click)="goToRegister()">
            Abrir Conta Gratuita
          </button>
        </mat-card-actions>

      </mat-card>
    </div>
  `,
  styles: [`
    .auth-card { 
      width: 100%; 
      max-width: 380px; 
      padding: 40px 30px; 
      text-align: center;
      /* Garantia extra de fundo branco caso o global falhe */
      background: white; 
    }

    .brand-header { margin-bottom: 30px; }
    
    .logo-icon { 
      background: rgba(0, 71, 187, 0.1); 
      color: var(--primary);
      width: 64px; height: 64px; 
      border-radius: 20px; 
      margin: 0 auto 16px;
      display: flex; align-items: center; justify-content: center;
    }
    .logo-icon mat-icon { font-size: 32px; width: 32px; height: 32px; }
    
    .brand-header h2 { margin: 0; font-size: 1.6rem; color: #002a70; font-weight: 700; }
    .brand-header p { color: #5E6C84; margin: 5px 0 0; font-size: 0.9rem; }
    
    .full-width-btn { width: 100%; margin-top: 10px; }
    
    .actions { flex-direction: column; padding-top: 24px; display: flex; align-items: center; }
    .divider-text { color: #5E6C84; font-size: 0.9rem; margin-bottom: 12px; display: block; }
    
    button[mat-stroked-button] { 
      width: 100%; 
      border-radius: 8px; 
      border: 2px solid var(--primary); 
      color: var(--primary);
      font-weight: 600;
    }
  `]
})
export class LoginComponent {
  cpf = '';
  password = '';
  constructor(private router: Router) {}

  login() {
    const id = localStorage.getItem('krt_account_id');
    if (id) this.router.navigate(['/dashboard']);
    else {
        alert('Conta não encontrada. Crie uma nova conta.');
        this.router.navigate(['/register']);
    }
  }
  goToRegister() { this.router.navigate(['/register']); }
}
