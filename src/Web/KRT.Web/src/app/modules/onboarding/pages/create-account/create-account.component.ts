import { Component } from '@angular/core';
import { AccountService } from '../../../../core/services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-account',
  template: `
    <div class="full-screen-center">
      <mat-card class="auth-card fade-in">
        <div class="header-left">
            <button mat-icon-button (click)="back()"><mat-icon>arrow_back</mat-icon></button>
            <h2>Criar Conta</h2>
        </div>

        <mat-card-content>
          <form (ngSubmit)="onSubmit()">
            <mat-form-field appearance="outline">
              <mat-label>Nome Completo</mat-label>
              <input matInput [(ngModel)]="model.customerName" name="name" required>
              <mat-icon matSuffix>person</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>CPF</mat-label>
              <input matInput [(ngModel)]="model.customerDocument" name="doc" required>
              <mat-icon matSuffix>badge</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Email</mat-label>
              <input matInput [(ngModel)]="model.customerEmail" name="email" required type="email">
              <mat-icon matSuffix>email</mat-icon>
            </mat-form-field>

            <button mat-raised-button color="primary" class="full-width-btn" type="submit">
              CONFIRMAR CADASTRO
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .auth-card { width: 100%; max-width: 400px; padding: 30px; }
    .header-left { display: flex; align-items: center; margin-bottom: 25px; gap: 10px; }
    .header-left h2 { margin: 0; color: var(--primary-dark); }
    .full-width-btn { width: 100%; margin-top: 15px; }
  `]
})
export class CreateAccountComponent {
  model = { customerName: '', customerDocument: '', customerEmail: '' };
  constructor(private accountService: AccountService, private router: Router) {}

  onSubmit() {
    const request = { ...this.model, branchCode: '0001' };
    this.accountService.create(request).subscribe({
      next: (res: any) => {
        // Correção de parsing manual do ID
        let id = '';
        if (typeof res === 'string') id = res;
        else if (res && res.id) id = res.id;
        else if (res && res.accountId) id = res.accountId;

        if (id) {
            localStorage.setItem('krt_account_id', id); 
            this.router.navigate(['/dashboard']);
        } else {
            alert('Conta criada, mas houve erro ao logar. Tente entrar novamente.');
            this.router.navigate(['/login']);
        }
      },
      error: (err) => alert('Erro: ' + JSON.stringify(err))
    });
  }
  back() { this.router.navigate(['/login']); }
}
