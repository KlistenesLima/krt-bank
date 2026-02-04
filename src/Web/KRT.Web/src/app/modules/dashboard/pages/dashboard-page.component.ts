import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../../core/services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  template: `
    <div class="container" *ngIf="account">
      <h1>Olá, {{ account.customerName }}</h1>
      <div class="card">
        <h3>Saldo: {{ balance | currency:'BRL' }}</h3>
        <button (click)="goToPix()">Fazer PIX</button>
      </div>
      
      <h3>Extrato</h3>
      <ul>
        <li *ngFor="let item of statement">
           {{ item.type }} - {{ item.amount | currency:'BRL' }} em {{ item.createdAt | date:'short' }}
        </li>
      </ul>
    </div>
  `
})
export class DashboardPageComponent implements OnInit {
  account: any;
  balance: number = 0;
  statement: any[] = [];
  accountId = localStorage.getItem('krt_account_id');

  constructor(private accountService: AccountService, private router: Router) {}

  ngOnInit() {
    if(!this.accountId) {
        this.router.navigate(['/']);
        return;
    }

    this.accountService.getById(this.accountId).subscribe(res => this.account = res);
    this.accountService.getBalance(this.accountId).subscribe((res: any) => this.balance = res.availableAmount);
    this.accountService.getStatement(this.accountId).subscribe((res: any) => this.statement = res.transactions || []);
  }

  goToPix() {
    this.router.navigate(['/pix']);
  }
}
