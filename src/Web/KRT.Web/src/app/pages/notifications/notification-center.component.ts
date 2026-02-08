import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notification-center.component.html',
  styleUrls: ['./notification-center.component.scss']
})
export class NotificationCenterComponent implements OnInit {
  accountId = '';
  notifications: any[] = [];
  unreadCount = 0;
  loading = false;
  categoryFilter = '';
  showUnreadOnly = false;
  page = 1;
  totalPages = 0;

  categories = ['pix', 'transferencia', 'cartao', 'seguranca', 'limite', 'promocao', 'sistema'];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.loading = true;
    let url = `${environment.apiUrl}/notifications/${this.accountId}?page=${this.page}&pageSize=15`;
    if (this.showUnreadOnly) url += '&unreadOnly=true';
    if (this.categoryFilter) url += `&category=${this.categoryFilter}`;

    this.http.get<any>(url).subscribe({
      next: (data) => {
        this.notifications = data.items;
        this.unreadCount = data.unreadCount;
        this.totalPages = data.totalPages;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  markAsRead(notificationId: string): void {
    this.http.post(`${environment.apiUrl}/notifications/${this.accountId}/read/${notificationId}`, {})
      .subscribe(() => {
        const n = this.notifications.find(x => x.id === notificationId);
        if (n) { n.isRead = true; this.unreadCount--; }
      });
  }

  markAllAsRead(): void {
    this.http.post(`${environment.apiUrl}/notifications/${this.accountId}/read-all`, {})
      .subscribe(() => {
        this.notifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
      });
  }

  deleteNotification(notificationId: string): void {
    this.http.delete(`${environment.apiUrl}/notifications/${this.accountId}/${notificationId}`)
      .subscribe(() => {
        this.notifications = this.notifications.filter(n => n.id !== notificationId);
      });
  }

  toggleFilter(): void {
    this.page = 1;
    this.loadNotifications();
  }

  goToPage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.page = p;
      this.loadNotifications();
    }
  }

  getTimeAgo(date: string): string {
    const diff = Date.now() - new Date(date).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'agora';
    if (mins < 60) return `${mins}min`;
    const hours = Math.floor(mins / 60);
    if (hours < 24) return `${hours}h`;
    const days = Math.floor(hours / 24);
    if (days < 30) return `${days}d`;
    return `${Math.floor(days / 30)}m`;
  }

  getPriorityClass(priority: string): string {
    return priority === 'high' ? 'priority-high' : priority === 'low' ? 'priority-low' : '';
  }
}