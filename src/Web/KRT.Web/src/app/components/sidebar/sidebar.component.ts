import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

interface NavItem {
  icon: string;
  label: string;
  route: string;
  badge?: number;
}

interface NavGroup {
  title: string;
  items: NavItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  collapsed = false;
  unreadNotifications = 0;
  accountId = '';

  navGroups: NavGroup[] = [
    {
      title: 'Principal',
      items: [
        { icon: '📊', label: 'Dashboard', route: '/dashboard-charts' },
        { icon: '📋', label: 'Extrato', route: '/statement' },
        { icon: '🔔', label: 'Notificacoes', route: '/notifications' }
      ]
    },
    {
      title: 'Pix',
      items: [
        { icon: '⚡', label: 'Pix Transfer', route: '/pix' },
        { icon: '📱', label: 'QR Code', route: '/pix-qrcode' },
        { icon: '📅', label: 'Agendado', route: '/scheduled-pix' },
        { icon: '👥', label: 'Contatos', route: '/contacts' }
      ]
    },
    {
      title: 'Pagamentos',
      items: [
        { icon: '📄', label: 'Boletos', route: '/boletos' },
        { icon: '💳', label: 'Cartoes Virtuais', route: '/virtual-cards' }
      ]
    },
    {
      title: 'Conta',
      items: [
        { icon: '👤', label: 'Perfil', route: '/profile' },
        { icon: '⚙️', label: 'Configuracoes', route: '/profile' }
      ]
    }
  ];

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    this.loadUnreadCount();
  }

  loadUnreadCount(): void {
    this.http.get<any>(`${environment.apiUrl}/notifications/${this.accountId}/unread-count`)
      .subscribe({
        next: (data) => {
          this.unreadNotifications = data.unreadCount;
          const notifItem = this.navGroups[0]?.items.find(i => i.route === '/notifications');
          if (notifItem) notifItem.badge = data.unreadCount;
        },
        error: () => {}
      });
  }

  toggle(): void { this.collapsed = !this.collapsed; }

  isActive(route: string): boolean { return this.router.url === route; }
}