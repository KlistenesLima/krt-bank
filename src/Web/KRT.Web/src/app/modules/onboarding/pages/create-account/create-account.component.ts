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
          <input [(ngModel)]="model.customerName" name="name" required placeholder="Seu nome completo">
        </div>
        <div>
          <label>CPF:</label>
          <input [(ngModel)]="model.customerDocument" name="doc" required placeholder="Apenas números">
        </div>
        <div>
          <label>Email:</label>
          <input [(ngModel)]="model.customerEmail" name="email" required placeholder="seu@email.com">
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
    const request = {
        ...this.model,
        branchCode: '0001' 
    };

    this.accountService.create(request).subscribe({
      next: (res: any) => {
        console.log('Resposta bruta da API:', res);

        // CORREÇÃO: Verifica se a resposta JÁ É o ID (string) ou um objeto
        let id = '';
        if (typeof res === 'string') {
            id = res;
        } else {
            id = res.id || res.accountId;
        }
        
        if (id) {
            alert('Conta criada! ID: ' + id);
            localStorage.setItem('krt_account_id', id); 
            this.router.navigate(['/dashboard']);
        } else {
            alert('Erro: ID não identificado na resposta.');
        }
      },
      error: (err) => {
        console.error(err);
        const msg = err.error?.errors ? JSON.stringify(err.error.errors) : JSON.stringify(err);
        alert('Erro ao criar conta: ' + msg);
      }
    });
  }
}
