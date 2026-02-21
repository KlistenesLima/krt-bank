import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-profile-page',
  template: `
    <div class="profile-container page-with-nav">
      <header class="profile-header">
        <div class="profile-avatar">{{ getInitials() }}</div>
        <h2>{{ userName }}</h2>
        <p>{{ userDoc }}</p>
      </header>

      <div class="menu-section">
        <button class="menu-item" (click)="router.navigate(['/profile/data'])">
          <mat-icon>person_outline</mat-icon>
          <span>Meus dados</span>
          <mat-icon class="chevron">chevron_right</mat-icon>
        </button>
        <button class="menu-item" (click)="router.navigate(['/pix/keys'])">
          <mat-icon>vpn_key</mat-icon>
          <span>Chaves Pix</span>
          <mat-icon class="chevron">chevron_right</mat-icon>
        </button>
        <button class="menu-item" (click)="router.navigate(['/profile/security'])">
          <mat-icon>security</mat-icon>
          <span>Segurança</span>
          <mat-icon class="chevron">chevron_right</mat-icon>
        </button>
        <button class="menu-item" (click)="router.navigate(['/profile/notifications'])">
          <mat-icon>notifications_none</mat-icon>
          <span>Notificações</span>
          <mat-icon class="chevron">chevron_right</mat-icon>
        </button>
      </div>

      <div class="menu-section">
        <button class="menu-item danger" (click)="logout()">
          <mat-icon>logout</mat-icon>
          <span>Sair da conta</span>
          <mat-icon class="chevron">chevron_right</mat-icon>
        </button>
      </div>

      <p class="version">KRT Bank v2.0 · Ambiente de desenvolvimento</p>
    </div>

    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .profile-container { min-height: 100vh; background: var(--krt-bg); }
    .profile-header {
      background: var(--krt-gradient); padding: 40px 20px 30px;
      text-align: center; border-radius: 0 0 32px 32px;
    }
    .profile-avatar {
      width: 72px; height: 72px; border-radius: 20px;
      background: rgba(255,255,255,0.2); color: white;
      display: flex; align-items: center; justify-content: center;
      font-weight: 800; font-size: 1.4rem; margin: 0 auto 12px;
      backdrop-filter: blur(10px);
    }
    .profile-header h2 { color: white; margin: 0; font-size: 1.2rem; }
    .profile-header p { color: rgba(255,255,255,0.7); font-size: 0.85rem; margin-top: 4px; }

    .menu-section {
      margin: 16px 20px; background: white; border-radius: var(--krt-radius);
      box-shadow: var(--krt-shadow-sm); overflow: hidden;
    }
    .menu-item {
      width: 100%; display: flex; align-items: center; gap: 14px;
      padding: 18px 20px; border: none; background: none;
      cursor: pointer; text-align: left; transition: background 0.2s;
      border-bottom: 1px solid var(--krt-divider);
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .menu-item:last-child { border-bottom: none; }
    .menu-item:hover { background: var(--krt-bg); }
    .menu-item mat-icon { color: var(--krt-text-secondary); }
    .menu-item span { flex: 1; font-size: 0.92rem; font-weight: 500; color: var(--krt-text); }
    .chevron { color: var(--krt-text-muted) !important; }
    .menu-item.danger mat-icon { color: var(--krt-danger); }
    .menu-item.danger span { color: var(--krt-danger); }

    .version { text-align: center; color: var(--krt-text-muted); font-size: 0.75rem; margin-top: 24px; }
  `]
})
export class ProfilePageComponent implements OnInit {
  userName = '';
  userDoc = '';

  constructor(public router: Router, private auth: AuthService) {}

  ngOnInit() {
    this.userName = localStorage.getItem('krt_account_name') || 'Usuário';
    const doc = localStorage.getItem('krt_account_doc') || '';
    if (doc.length === 11) {
      this.userDoc = doc.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    } else {
      this.userDoc = doc;
    }
  }

  getInitials(): string {
    return this.userName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
  }

  logout() { this.auth.logout(); }
}

