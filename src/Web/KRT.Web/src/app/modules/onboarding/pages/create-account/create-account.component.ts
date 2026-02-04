import { Component } from '@angular/core';
import { AccountService } from '../../../../core/services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-account',
  template: `
    <div class="container">
      <h2>Abra sua conta KRT</h2>
      <form (ngSubmit)="onSubmit()">
        <div>
          <label>Nome:</label>
          <input [(ngModel)]="model.customerName" name="name" required>
        </div>
        <div>
          <label>CPF:</label>
          <input [(ngModel)]="model.customerDocument" name="doc" required>
        </div>
        <div>
          <label>Email:</label>
          <input [(ngModel)]="model.customerEmail" name="email" required>
        </div>
        <button type="submit">Criar Conta</button>
      </form>
    </div>
  `
})
export class CreateAccountComponent {
  model = { customerName: '', customerDocument: '', customerEmail: '' };

  constructor(private accountService: AccountService, private router: Router) {}

  onSubmit() {
    this.accountService.create(this.model).subscribe({
      next: (res: any) => {
        alert('Conta criada! ID: ' + res.accountId);
        localStorage.setItem('krt_account_id', res.accountId); 
        this.router.navigate(['/dashboard']);
      },
      error: (err) => alert('Erro: ' + JSON.stringify(err))
    });
  }
}
