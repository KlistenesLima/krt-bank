import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { SignalRService } from '../../../core/services/signalr.service';

interface Toast {
  id: number;
  type: string;
  title: string;
  message: string;
  visible: boolean;
}

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      <div *ngFor="let toast of toasts"
           class="toast"
           [class.toast-success]="toast.type === 'success'"
           [class.toast-error]="toast.type === 'error'"
           [class.toast-warning]="toast.type === 'warning'"
           [class.toast-info]="toast.type === 'info'"
           [class.toast-visible]="toast.visible"
           (click)="removeToast(toast.id)">
        <div class="toast-icon">
          <span *ngIf="toast.type === 'success'">&#10004;</span>
          <span *ngIf="toast.type === 'error'">&#10008;</span>
          <span *ngIf="toast.type === 'warning'">&#9888;</span>
          <span *ngIf="toast.type === 'info'">&#8505;</span>
        </div>
        <div class="toast-content">
          <strong>{{ toast.title }}</strong>
          <p>{{ toast.message }}</p>
        </div>
        <button class="toast-close" (click)="removeToast(toast.id)">&times;</button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 10000;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 380px;
    }
    .toast {
      display: flex;
      align-items: flex-start;
      padding: 14px 16px;
      border-radius: 12px;
      box-shadow: 0 8px 32px rgba(0,0,0,0.18);
      color: #fff;
      transform: translateX(120%);
      transition: transform 0.35s cubic-bezier(0.4,0,0.2,1), opacity 0.35s;
      opacity: 0;
      cursor: pointer;
      backdrop-filter: blur(8px);
    }
    .toast-visible {
      transform: translateX(0);
      opacity: 1;
    }
    .toast-success { background: linear-gradient(135deg, #00c853, #009624); }
    .toast-error   { background: linear-gradient(135deg, #ff1744, #d50000); }
    .toast-warning { background: linear-gradient(135deg, #ff9100, #ff6d00); }
    .toast-info    { background: linear-gradient(135deg, #2979ff, #2962ff); }
    .toast-icon {
      font-size: 20px;
      margin-right: 12px;
      flex-shrink: 0;
    }
    .toast-content {
      flex: 1;
    }
    .toast-content strong {
      display: block;
      font-size: 14px;
      margin-bottom: 2px;
    }
    .toast-content p {
      margin: 0;
      font-size: 13px;
      opacity: 0.9;
    }
    .toast-close {
      background: none;
      border: none;
      color: #fff;
      font-size: 20px;
      cursor: pointer;
      opacity: 0.7;
      padding: 0 0 0 8px;
      line-height: 1;
    }
    .toast-close:hover { opacity: 1; }
  `]
})
export class ToastComponent implements OnInit, OnDestroy {
  toasts: Toast[] = [];
  private nextId = 0;
  private sub!: Subscription;

  constructor(private signalR: SignalRService) {}

  ngOnInit(): void {
    this.sub = this.signalR.notifications.subscribe(n => {
      this.addToast(n.type, n.title, n.message);
    });
  }

  addToast(type: string, title: string, message: string): void {
    const id = this.nextId++;
    const toast: Toast = { id, type, title, message, visible: false };
    this.toasts.push(toast);

    // Anima entrada
    setTimeout(() => toast.visible = true, 50);

    // Auto-remove apos 5s
    setTimeout(() => this.removeToast(id), 5000);
  }

  removeToast(id: number): void {
    const toast = this.toasts.find(t => t.id === id);
    if (toast) {
      toast.visible = false;
      setTimeout(() => {
        this.toasts = this.toasts.filter(t => t.id !== id);
      }, 400);
    }
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}