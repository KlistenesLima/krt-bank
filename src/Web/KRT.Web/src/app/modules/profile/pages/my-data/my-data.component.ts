import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { AccountService } from '../../../../core/services/account.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-my-data',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Meus Dados</h1>
        <div style="width: 40px"></div>
      </header>

      <main class="container fade-in">
        <div class="avatar-section">
            <div class="avatar-big">{{ getInitials() }}</div>
            <button mat-button color="primary">Alterar foto</button>
        </div>

        <form class="data-form">
            <mat-form-field appearance="outline">
                <mat-label>Nome Completo</mat-label>
                <input matInput [value]="account?.customerName" disabled>
                <mat-icon matSuffix>lock</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
                <mat-label>CPF</mat-label>
                <input matInput [value]="account?.customerDocument" disabled>
                <mat-icon matSuffix>lock</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
                <mat-label>E-mail</mat-label>
                <input matInput [value]="account?.customerEmail" disabled>
                <mat-icon matSuffix>lock</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
                <mat-label>Celular</mat-label>
                <input matInput placeholder="(00) 00000-0000" value="(11) 99999-9999">
                <mat-icon matSuffix>edit</mat-icon>
            </mat-form-field>

            <button mat-raised-button color="primary" class="save-btn" (click)="save()">
                SALVAR ALTERAÇÕES
            </button>
        </form>
      </main>
    </div>
  `,
  styles: [`
    .avatar-section { display: flex; flex-direction: column; align-items: center; margin: 20px 0 30px; }
    .avatar-big {
        width: 100px; height: 100px; background: #e0e0e0; color: #555;
        border-radius: 50%; font-size: 2.5rem; font-weight: 600;
        display: flex; align-items: center; justify-content: center; margin-bottom: 10px;
    }
    .data-form { display: flex; flex-direction: column; gap: 15px; }
    .save-btn { height: 50px; margin-top: 10px; }
  `]
})
export class MyDataComponent implements OnInit {
  account: any;
  constructor(private location: Location, private accService: AccountService, private notify: NotificationService) {}

  ngOnInit() {
    const id = localStorage.getItem('krt_account_id');
    if(id) this.accService.getById(id).subscribe(res => this.account = res);
  }

  getInitials() { return this.account?.customerName?.substring(0,2).toUpperCase() || 'US'; }
  save() { this.notify.success('Dados atualizados com sucesso!'); }
  goBack() { this.location.back(); }
}
