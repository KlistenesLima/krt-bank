import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../../core/services/account.service';
import { Router } from '@angular/router';
import { Location } from '@angular/common';

@Component({
  selector: 'app-profile-page',
  template: `
    <div class="app-layout" *ngIf="account">
      <header class="profile-header">
        <button mat-icon-button (click)="goBack()" class="back-btn"><mat-icon>arrow_back</mat-icon></button>
        <div class="avatar-circle">
           <span>{{ getInitials() }}</span>
        </div>
        <h2>{{ account.customerName }}</h2>
        <p>Agência 0001 • Conta {{ account.accountId.substring(0,8) }}</p>
      </header>

      <main class="container fade-in mt-minus">
        <mat-card class="menu-card">
          <mat-nav-list>
            <mat-list-item>
               <mat-icon matListItemIcon>person</mat-icon>
               <div matListItemTitle>Meus Dados</div>
            </mat-list-item>
            <mat-divider></mat-divider>
            <mat-list-item>
               <mat-icon matListItemIcon>security</mat-icon>
               <div matListItemTitle>Segurança</div>
            </mat-list-item>
            <mat-divider></mat-divider>
            <mat-list-item>
               <mat-icon matListItemIcon>notifications</mat-icon>
               <div matListItemTitle>Notificações</div>
            </mat-list-item>
            <mat-divider></mat-divider>
            <mat-list-item (click)="logout()">
               <mat-icon matListItemIcon color="warn">logout</mat-icon>
               <div matListItemTitle class="danger-text">Sair do App</div>
            </mat-list-item>
          </mat-nav-list>
        </mat-card>
      </main>
    </div>
  `,
  styles: [`
    .profile-header {
      background: var(--primary); color: white;
      display: flex; flex-direction: column; align-items: center;
      padding: 20px 20px 60px; border-radius: 0 0 30px 30px;
      position: relative;
    }
    .back-btn { position: absolute; left: 10px; top: 10px; color: white; }
    .avatar-circle {
      width: 80px; height: 80px; background: rgba(255,255,255,0.2);
      border-radius: 50%; display: flex; justify-content: center; align-items: center;
      font-size: 2rem; font-weight: bold; margin-bottom: 15px; margin-top: 10px;
    }
    .profile-header h2 { margin: 0; font-size: 1.4rem; }
    .profile-header p { opacity: 0.8; font-size: 0.9rem; margin-top: 5px; }
    
    .mt-minus { margin-top: -40px; }
    .menu-card { padding: 0; overflow: hidden; }
    .danger-text { color: #f44336; font-weight: 500; }
  `]
})
export class ProfilePageComponent implements OnInit {
  account: any;
  constructor(private accService: AccountService, private router: Router, private location: Location) {}

  ngOnInit() {
    const id = localStorage.getItem('krt_account_id');
    if(id) this.accService.getById(id).subscribe(res => this.account = res);
  }

  getInitials() {
    return this.account?.customerName?.substring(0,2).toUpperCase() || 'US';
  }

  logout() {
    localStorage.removeItem('krt_account_id');
    this.router.navigate(['/login']);
  }
  goBack() { this.location.back(); }
}
