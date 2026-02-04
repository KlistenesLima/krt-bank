import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../core/services/account.service';

@Component({
  selector: 'app-dashboard',
  template: \
    <div class="card">
      <h2>Minha Conta</h2>
      <div *ngIf="account">
        <h3>Saldo: {{ account.balance | currency:'BRL' }}</h3>
        <p>Conta: {{ account.number }}</p>
      </div>
      <div *ngIf="!account">
        <button (click)="createAccount()">Abrir Conta Digital</button>
      </div>
    </div>
  \,
  styles: [\.card { border: 1px solid #ccc; padding: 20px; border-radius: 8px; max-width: 400px; }\]
})
export class DashboardPageComponent implements OnInit {
  account: any = null;

  constructor(private accountService: AccountService) {}

  ngOnInit() {
    // Mock para exemplo. Na vida real, pegaria do Backend
    // this.loadAccount();
  }

  createAccount() {
    const fakeData = { customerName: "User Demo", cpf: "12345678900", email: "demo@krt.com" };
    this.accountService.createAccount(fakeData).subscribe({
      next: (res) => alert('Conta criada com sucesso! ID: ' + res),
      error: (err) => alert('Erro ao criar conta: ' + JSON.stringify(err))
    });
  }
}
