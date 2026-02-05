import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../../core/services/account.service';
import { Router } from '@angular/router';
import { Location } from '@angular/common';

@Component({
  selector: 'app-profile-page',
  template: `
    <div class="app-layout" *ngIf="account">
      <header class="profile-header">
        <div class="header-top">
           <button mat-icon-button (click)="goBack()" class="white-icon">
              <mat-icon>arrow_back</mat-icon>
           </button>
           <span class="header-title">Meu Perfil</span>
           <div style="width: 40px"></div>
        </div>

        <div class="profile-summary">
           <div class="avatar-circle fade-in">
              <span>{{ getInitials() }}</span>
           </div>
           <h2 class="fade-in">{{ account.customerName }}</h2>
           <div class="account-badge fade-in">
              <span>Agência 0001</span>
              <span class="dot">•</span>
              <span>Conta {{ account.accountId.substring(0,8) }}</span>
           </div>
        </div>
      </header>

      <main class="container content-overlap fade-in">
        <mat-card class="menu-card">
          <mat-nav-list>
            
            <h3 class="menu-header">Configurações</h3>
            
            <a mat-list-item class="menu-item" (click)="nav('/profile/data')">
               <mat-icon matListItemIcon class="menu-icon">person_outline</mat-icon>
               <span matListItemTitle>Meus Dados</span>
               <mat-icon matListItemMeta class="chevron">chevron_right</mat-icon>
            </a>
            
            <mat-divider class="inset-divider"></mat-divider>
            
            <a mat-list-item class="menu-item" (click)="nav('/profile/security')">
               <mat-icon matListItemIcon class="menu-icon">lock_open</mat-icon>
               <span matListItemTitle>Segurança</span>
               <mat-icon matListItemMeta class="chevron">chevron_right</mat-icon>
            </a>

            <mat-divider class="inset-divider"></mat-divider>
            
            <a mat-list-item class="menu-item" (click)="nav('/profile/notifications')">
               <mat-icon matListItemIcon class="menu-icon">notifications_none</mat-icon>
               <span matListItemTitle>Notificações</span>
               <mat-icon matListItemMeta class="chevron">chevron_right</mat-icon>
            </a>

            <h3 class="menu-header mt-3">Conta</h3>

            <a mat-list-item class="menu-item logout-item" (click)="logout()">
               <mat-icon matListItemIcon class="menu-icon warn">logout</mat-icon>
               <span matListItemTitle class="danger-text">Sair do App</span>
            </a>
          </mat-nav-list>
        </mat-card>

        <p class="app-version">Versão 1.0.0 • KRT Bank</p>
      </main>
    </div>
  `,
  styles: [`
    .profile-header {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      color: white; padding-bottom: 90px;
      border-radius: 0 0 32px 32px;
      box-shadow: 0 4px 20px rgba(0,42,112,0.2);
    }
    .header-top { display: flex; justify-content: space-between; align-items: center; padding: 15px 10px; }
    .white-icon { color: white; }
    .header-title { font-weight: 500; font-size: 1rem; opacity: 0.9; }
    .profile-summary { display: flex; flex-direction: column; align-items: center; text-align: center; margin-top: 10px; }
    .avatar-circle {
      width: 88px; height: 88px; background: rgba(255,255,255,0.15);
      border: 2px solid rgba(255,255,255,0.3); backdrop-filter: blur(5px);
      border-radius: 50%; display: flex; justify-content: center; align-items: center;
      font-size: 2.2rem; font-weight: 600; margin-bottom: 16px;
      box-shadow: 0 8px 16px rgba(0,0,0,0.1);
    }
    .profile-summary h2 { margin: 0; font-size: 1.5rem; font-weight: 600; letter-spacing: -0.5px; }
    .account-badge { 
      background: rgba(255,255,255,0.2); padding: 6px 14px; border-radius: 20px; 
      margin-top: 8px; font-size: 0.85rem; display: flex; align-items: center; gap: 8px;
      font-weight: 500; color: rgba(255,255,255,0.9);
    }
    .dot { font-weight: bold; }
    .content-overlap { margin-top: -65px; position: relative; z-index: 10; padding-bottom: 40px; }
    .menu-card { padding: 8px 0; overflow: hidden; }
    .menu-header {
        font-size: 0.75rem; font-weight: 700; color: var(--text-secondary);
        text-transform: uppercase; letter-spacing: 1.2px;
        margin: 16px 0 8px 0; padding-left: 24px;
    }
    .mt-3 { margin-top: 24px; }
    .menu-item { height: 56px; cursor: pointer; }
    .menu-item:hover { background-color: #f9f9f9; }
    .menu-icon { color: var(--text-secondary); margin-right: 16px; }
    .menu-icon.warn { color: #ff5252; }
    .chevron { color: #ddd; font-size: 20px; }
    .danger-text { color: #ff5252; font-weight: 600; }
    .inset-divider { margin-left: 56px !important; }
    .app-version { text-align: center; color: #999; font-size: 0.75rem; margin-top: 30px; }
  `]
})
export class ProfilePageComponent implements OnInit {
  account: any;
  constructor(private accService: AccountService, private router: Router, private location: Location) {}

  ngOnInit() {
    const id = localStorage.getItem('krt_account_id');
    if(id) this.accService.getById(id).subscribe(res => this.account = res);
  }
  getInitials() { return this.account?.customerName?.substring(0,2).toUpperCase() || 'US'; }
  logout() { localStorage.removeItem('krt_account_id'); this.router.navigate(['/login']); }
  goBack() { this.location.back(); }
  nav(route: string) { this.router.navigate([route]); }
}
