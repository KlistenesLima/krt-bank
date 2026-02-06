import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  template: `
    <div class="app-layout">
      <header class="header-section">
        <div class="container header-content">
            <button mat-icon-button (click)="goBack()" style="color:white"><mat-icon>arrow_back</mat-icon></button>
            <h1>Meu Perfil</h1>
            <div style="width:40px"></div>
        </div>
      </header>

      <main class="container fade-in" style="margin-top: -40px;">
        <mat-card class="profile-card">
            <div class="avatar-circle">{{ initials }}</div>
            <h2>{{ session?.customerName || 'Cliente' }}</h2>
            <p>{{ session?.email }}</p>
            <p class="doc">{{ session?.document }}</p>
        </mat-card>

        <mat-card class="menu-card">
            <mat-list>
                <mat-list-item (click)="goTo('/profile/data')">
                    <mat-icon matListItemIcon>person</mat-icon>
                    <span matListItemTitle>Meus Dados</span>
                    <mat-icon class="chevron">chevron_right</mat-icon>
                </mat-list-item>
                <mat-divider></mat-divider>
                <mat-list-item (click)="goTo('/profile/security')">
                    <mat-icon matListItemIcon>security</mat-icon>
                    <span matListItemTitle>Segurança</span>
                    <mat-icon class="chevron">chevron_right</mat-icon>
                </mat-list-item>
                <mat-divider></mat-divider>
                <mat-list-item (click)="goTo('/profile/notifications')">
                    <mat-icon matListItemIcon>notifications</mat-icon>
                    <span matListItemTitle>Notificações</span>
                    <mat-icon class="chevron">chevron_right</mat-icon>
                </mat-list-item>
                <mat-divider></mat-divider>
                <mat-list-item (click)="logout()">
                    <mat-icon matListItemIcon style="color:#ff5252">exit_to_app</mat-icon>
                    <span matListItemTitle style="color:#ff5252">Sair</span>
                </mat-list-item>
            </mat-list>
        </mat-card>
      </main>
      <app-bottom-nav></app-bottom-nav>
    </div>
  `,
  styles: [`
    .header-section { background: var(--primary); color: white; padding-bottom: 60px; border-radius: 0 0 24px 24px; }
    .header-content { display: flex; align-items: center; justify-content: space-between; padding-top: 15px; }
    .header-content h1 { color: white; margin: 0; font-size: 1.2rem; }
    .profile-card { text-align: center; padding: 30px; margin-bottom: 20px; }
    .avatar-circle {
        width: 72px; height: 72px; border-radius: 50%; background: var(--primary); color: white;
        display: flex; align-items: center; justify-content: center; font-size: 1.5rem; font-weight: 700;
        margin: 0 auto 15px;
    }
    .profile-card h2 { margin: 0; }
    .profile-card p { color: #666; margin: 4px 0; }
    .doc { font-size: 0.85rem; }
    .menu-card { padding: 0; }
    mat-list-item { cursor: pointer; }
    .chevron { margin-left: auto; color: #ccc; }
  `]
})
export class ProfilePageComponent implements OnInit {
  session: any;
  initials = '';

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit() {
    this.session = this.authService.currentSession;
    if (this.session?.customerName) {
      this.initials = this.session.customerName.split(' ')
        .map((n: string) => n[0]).join('').substring(0, 2).toUpperCase();
    }
  }

  goTo(path: string) { this.router.navigate([path]); }
  goBack() { this.router.navigate(['/dashboard']); }
  logout() { this.authService.logout(); }
}
