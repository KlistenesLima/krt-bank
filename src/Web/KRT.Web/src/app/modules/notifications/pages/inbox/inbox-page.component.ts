import { Component } from '@angular/core';
import { Location } from '@angular/common';

@Component({
  selector: 'app-inbox',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Notificações</h1>
        <button mat-icon-button (click)="markAllRead()"><mat-icon>done_all</mat-icon></button>
      </header>

      <main class="container fade-in">
        <div class="date-header">Hoje</div>
        
        <div class="notif-card unread">
            <div class="icon-circle bg-green"><mat-icon>pix</mat-icon></div>
            <div class="notif-content">
                <div class="notif-title">Pix Recebido</div>
                <div class="notif-body">Você recebeu R$ 150,00 de Maria Silva.</div>
                <div class="notif-time">10:42</div>
            </div>
        </div>

        <div class="notif-card unread">
            <div class="icon-circle bg-blue"><mat-icon>security</mat-icon></div>
            <div class="notif-content">
                <div class="notif-title">Acesso Detectado</div>
                <div class="notif-body">Novo acesso na sua conta pelo dispositivo iPhone 14.</div>
                <div class="notif-time">08:15</div>
            </div>
        </div>

        <div class="date-header">Ontem</div>

        <div class="notif-card">
            <div class="icon-circle bg-orange"><mat-icon>shopping_bag</mat-icon></div>
            <div class="notif-content">
                <div class="notif-title">Compra Aprovada</div>
                <div class="notif-body">Compra de R$ 89,90 no iFood aprovada.</div>
                <div class="notif-time">19:30</div>
            </div>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .date-header { font-size: 0.85rem; font-weight: 600; color: #888; margin: 15px 5px 10px; text-transform: uppercase; }
    
    .notif-card { 
        background: white; padding: 15px; border-radius: 12px; margin-bottom: 10px;
        display: flex; gap: 15px; align-items: flex-start;
        box-shadow: 0 2px 8px rgba(0,0,0,0.03);
        border-left: 4px solid transparent;
    }
    .notif-card.unread { border-left-color: var(--primary); background: #f8f9ff; }
    
    .icon-circle { 
        min-width: 40px; height: 40px; border-radius: 50%; 
        display: flex; justify-content: center; align-items: center; color: white;
    }
    .bg-green { background: #00D09E; }
    .bg-blue { background: var(--primary); }
    .bg-orange { background: #ff9800; }
    
    .notif-content { flex: 1; }
    .notif-title { font-weight: 700; font-size: 0.95rem; margin-bottom: 4px; color: var(--text-main); }
    .notif-body { font-size: 0.9rem; color: #555; line-height: 1.4; }
    .notif-time { font-size: 0.75rem; color: #999; margin-top: 8px; text-align: right; }
  `]
})
export class InboxPageComponent {
  constructor(private location: Location) {}
  markAllRead() { 
      // Lógica mock
      const cards = document.querySelectorAll('.notif-card');
      cards.forEach(c => c.classList.remove('unread'));
  }
  goBack() { this.location.back(); }
}
