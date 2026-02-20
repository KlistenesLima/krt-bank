import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  badge?: number;
  group?: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: 
    <aside class="sidebar" [class.collapsed]="collapsed" [class.dark]="darkMode">
      <div class="sidebar-header">
        <span class="logo" *ngIf="!collapsed">KRT Bank</span>
        <span class="logo-mini" *ngIf="collapsed">K</span>
        <button class="toggle-btn" (click)="toggleCollapse()">
          {{ collapsed ? '>' : '<' }}
        </button>
      </div>
      <nav class="sidebar-nav">
        <div *ngFor="let group of groups" class="nav-group">
          <span class="group-label" *ngIf="!collapsed && group">{{ group }}</span>
          <a *ngFor="let item of getItemsByGroup(group)"
             [routerLink]="item.route"
             routerLinkActive="active"
             class="nav-item"
             [title]="item.label">
            <span class="nav-icon">{{ item.icon }}</span>
            <span class="nav-label" *ngIf="!collapsed">{{ item.label }}</span>
            <span class="badge" *ngIf="item.badge && !collapsed">{{ item.badge }}</span>
          </a>
        </div>
      </nav>
    </aside>
  ,
  styles: [
    .sidebar { width: 260px; height: 100vh; background: #1a1a2e; color: #fff; display: flex; flex-direction: column; transition: width 0.3s; position: fixed; left: 0; top: 0; z-index: 100; }
    .sidebar.collapsed { width: 64px; }
    .sidebar-header { display: flex; align-items: center; justify-content: space-between; padding: 16px; border-bottom: 1px solid rgba(255,255,255,0.1); }
    .logo { font-size: 1.2rem; font-weight: 700; }
    .logo-mini { font-size: 1.4rem; font-weight: 700; }
    .toggle-btn { background: none; border: none; color: #fff; cursor: pointer; font-size: 1.1rem; padding: 4px 8px; border-radius: 4px; }
    .toggle-btn:hover { background: rgba(255,255,255,0.1); }
    .sidebar-nav { flex: 1; overflow-y: auto; padding: 8px 0; }
    .nav-group { margin-bottom: 8px; }
    .group-label { display: block; padding: 8px 16px 4px; font-size: 0.7rem; text-transform: uppercase; letter-spacing: 1px; color: rgba(255,255,255,0.4); }
    .nav-item { display: flex; align-items: center; gap: 12px; padding: 10px 16px; color: rgba(255,255,255,0.7); text-decoration: none; transition: all 0.2s; border-left: 3px solid transparent; }
    .nav-item:hover { background: rgba(255,255,255,0.05); color: #fff; }
    .nav-item.active { background: rgba(99,102,241,0.15); color: #818cf8; border-left-color: #818cf8; }
    .nav-icon { font-size: 1.2rem; width: 24px; text-align: center; }
    .nav-label { font-size: 0.9rem; }
    .badge { background: #ef4444; color: #fff; border-radius: 10px; padding: 1px 7px; font-size: 0.7rem; margin-left: auto; }
    .sidebar.dark { background: #0f0f23; }
  ]
})
export class SidebarComponent {
  @Input() darkMode = false;
  @Input() unreadNotifications = 0;
  @Output() collapsedChange = new EventEmitter<boolean>();

  collapsed = false;

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: '📊', route: '/dashboard', group: 'Principal' },
    { label: 'Extrato', icon: '📋', route: '/statement', group: 'Principal' },
    { label: 'Pix', icon: '⚡', route: '/pix-qrcode', group: 'Transacoes' },
    { label: 'Pix Agendado', icon: '📅', route: '/scheduled-pix', group: 'Transacoes' },
    { label: 'Contatos', icon: '👥', route: '/contacts', group: 'Transacoes' },
    { label: 'Boletos', icon: '🏦', route: '/boletos', group: 'Transacoes' },
    { label: 'Cartao Virtual', icon: '💳', route: '/virtual-card', group: 'Produtos' },
    { label: 'Metas', icon: '🎯', route: '/goals', group: 'Produtos' },
    { label: 'Seguros', icon: '🛡️', route: '/insurance', group: 'Produtos' },
    { label: 'Marketplace', icon: '🛒', route: '/marketplace', group: 'Produtos' },
    { label: 'Notificacoes', icon: '🔔', route: '/notifications', group: 'Conta' },
    { label: 'Perfil', icon: '👤', route: '/profile', group: 'Conta' },
    { label: 'Chatbot', icon: '🤖', route: '/chatbot', group: 'Conta' },
    { label: 'Admin', icon: '⚙️', route: '/admin', group: 'Sistema' },
    { label: 'Monitoramento', icon: '📈', route: '/monitoring', group: 'Sistema' },
  ];

  get groups(): string[] {
    return [...new Set(this.navItems.map(i => i.group || ''))];
  }

  getItemsByGroup(group: string): NavItem[] {
    const items = this.navItems.filter(i => (i.group || '') === group);
    return items.map(i => ({
      ...i,
      badge: i.route === '/notifications' ? this.unreadNotifications : i.badge
    }));
  }

  toggleCollapse() {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }
}
