import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-investments',
  template: `
    <div class="app-layout">
      <header class="invest-header">
        <button mat-icon-button (click)="goBack()" class="white-icon"><mat-icon>arrow_back</mat-icon></button>
        <h1>Cofre KRT</h1>
        <button mat-icon-button class="white-icon"><mat-icon>history</mat-icon></button>
      </header>

      <main class="container content-overlap fade-in">
        <mat-card class="main-card">
            <div class="total-label">Total Guardado</div>
            <div class="total-value">{{ savedAmount | currency:'BRL' }}</div>
            <div class="yield-info">
                <mat-icon>trending_up</mat-icon> Renderam <strong>R$ 12,40</strong> este mês
            </div>
            
            <div class="actions-row">
                <button mat-raised-button color="primary" class="action-btn" (click)="deposit()">
                    <mat-icon>add</mat-icon> GUARDAR
                </button>
                <button mat-stroked-button color="primary" class="action-btn" (click)="withdraw()">
                    <mat-icon>remove</mat-icon> RESGATAR
                </button>
            </div>
        </mat-card>

        <h3 class="section-title">Objetivos</h3>
        <div class="goals-list">
            <mat-card class="goal-card">
                <div class="goal-icon">🚗</div>
                <div class="goal-info">
                    <div class="goal-name">Carro Novo</div>
                    <mat-progress-bar mode="determinate" value="45"></mat-progress-bar>
                    <div class="goal-meta">R$ 4.500 de R$ 10.000</div>
                </div>
            </mat-card>
            
            <mat-card class="goal-card">
                <div class="goal-icon">🏖️</div>
                <div class="goal-info">
                    <div class="goal-name">Viagem</div>
                    <mat-progress-bar mode="determinate" value="20" color="accent"></mat-progress-bar>
                    <div class="goal-meta">R$ 1.000 de R$ 5.000</div>
                </div>
            </mat-card>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .invest-header {
        background: #4a148c; color: white; padding: 20px 20px 80px; 
        border-radius: 0 0 32px 32px;
        display: flex; justify-content: space-between; align-items: center;
    }
    .white-icon { color: white; }
    .invest-header h1 { margin: 0; font-size: 1.2rem; }
    .content-overlap { margin-top: -60px; position: relative; z-index: 10; }
    .main-card { padding: 30px 20px; text-align: center; margin-bottom: 30px; }
    .total-label { color: #666; font-size: 0.9rem; text-transform: uppercase; letter-spacing: 1px; }
    .total-value { font-size: 2.5rem; font-weight: 700; color: #4a148c; margin: 10px 0; }
    .yield-info { 
        display: inline-flex; align-items: center; gap: 5px; 
        background: #f3e5f5; color: #4a148c; padding: 5px 12px; border-radius: 20px; font-size: 0.9rem;
    }
    .yield-info mat-icon { font-size: 18px; height: 18px; width: 18px; }
    .actions-row { display: flex; gap: 15px; margin-top: 30px; }
    .action-btn { flex: 1; height: 50px; }
    .section-title { margin-bottom: 15px; color: #444; margin-left: 5px; }
    .goal-card { padding: 15px; display: flex; gap: 15px; align-items: center; margin-bottom: 10px; }
    .goal-icon { font-size: 2rem; background: #f5f5f5; width: 50px; height: 50px; display: flex; justify-content: center; align-items: center; border-radius: 12px; }
    .goal-info { flex: 1; }
    .goal-name { font-weight: 600; margin-bottom: 5px; }
    .goal-meta { font-size: 0.8rem; color: #888; margin-top: 5px; text-align: right; }
  `]
})
export class InvestmentsPageComponent {
  savedAmount = 12500.50;
  constructor(private location: Location, private notify: NotificationService) {}
  deposit() { this.savedAmount += 100; this.notify.success('R$ 100,00 guardados!'); }
  withdraw() {
      if (this.savedAmount >= 100) { this.savedAmount -= 100; this.notify.success('R$ 100,00 resgatados.'); } 
      else { this.notify.error('Saldo insuficiente.'); }
  }
  goBack() { this.location.back(); }
}
