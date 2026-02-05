import { Component } from '@angular/core';
import { Location } from '@angular/common';

@Component({
  selector: 'app-notifications',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Notificações</h1>
        <div style="width: 40px"></div>
      </header>

      <main class="container fade-in">
        <mat-card class="list-card">
            <div class="toggle-item">
                <span>Transações e Pix</span>
                <mat-slide-toggle color="primary" checked></mat-slide-toggle>
            </div>
            <mat-divider></mat-divider>
            <div class="toggle-item">
                <span>Novidades e Ofertas</span>
                <mat-slide-toggle color="primary"></mat-slide-toggle>
            </div>
            <mat-divider></mat-divider>
            <div class="toggle-item">
                <span>Avisos de Segurança</span>
                <mat-slide-toggle color="primary" checked disabled></mat-slide-toggle>
            </div>
        </mat-card>
        
        <p class="hint">Notificações de segurança não podem ser desativadas.</p>
      </main>
    </div>
  `,
  styles: [`
    .list-card { padding: 0; }
    .toggle-item { display: flex; justify-content: space-between; align-items: center; padding: 20px; font-weight: 500; }
    .hint { text-align: center; color: #999; font-size: 0.8rem; margin-top: 20px; }
  `]
})
export class NotificationsComponent {
  constructor(private location: Location) {}
  goBack() { this.location.back(); }
}
