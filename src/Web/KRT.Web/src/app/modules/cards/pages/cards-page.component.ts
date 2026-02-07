import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cards-page',
  template: `
    <div class="cards-container page-with-nav">
      <header class="page-header">
        <button mat-icon-button (click)="router.navigate(['/dashboard'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Cartões</h1>
        <div style="width:40px"></div>
      </header>

      <div class="card-display fade-in">
        <div class="credit-card">
          <div class="card-top">
            <span class="card-brand">KRT Bank</span>
            <mat-icon>contactless</mat-icon>
          </div>
          <div class="card-number">
            <span>{{ showCardNumber ? '5412 7534 8821 0039' : '•••• •••• •••• 0039' }}</span>
          </div>
          <div class="card-bottom">
            <div>
              <span class="card-label">TITULAR</span>
              <span class="card-value">{{ userName | uppercase }}</span>
            </div>
            <div>
              <span class="card-label">VALIDADE</span>
              <span class="card-value">12/29</span>
            </div>
          </div>
        </div>

        <button class="show-number-btn" (click)="showCardNumber = !showCardNumber">
          <mat-icon>{{ showCardNumber ? 'visibility_off' : 'visibility' }}</mat-icon>
          {{ showCardNumber ? 'Esconder número' : 'Mostrar número' }}
        </button>
      </div>

      <div class="card-info">
        <div class="info-card">
          <div class="info-row">
            <span>Fatura atual</span>
            <strong>R$ 0,00</strong>
          </div>
          <div class="info-row">
            <span>Limite disponível</span>
            <strong class="success">R$ 5.000,00</strong>
          </div>
          <div class="info-row">
            <span>Vencimento</span>
            <strong>Dia 15</strong>
          </div>
        </div>

        <button class="action-btn" (click)="router.navigate(['/dashboard'])">
          <mat-icon>lock</mat-icon>
          Bloquear cartão
        </button>
      </div>
    </div>

    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .cards-container { min-height: 100vh; background: var(--krt-bg); }
    .page-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 20px; background: white; border-bottom: 1px solid var(--krt-divider);
    }
    .page-header h1 { font-size: 1.1rem; font-weight: 700; margin: 0; }

    .card-display { padding: 24px 20px; max-width: 500px; margin: 0 auto; }
    .credit-card {
      background: var(--krt-gradient-card); border-radius: 20px;
      padding: 28px 24px; color: white; position: relative;
      box-shadow: 0 12px 40px rgba(0,51,153,0.3);
      aspect-ratio: 1.6;
      display: flex; flex-direction: column; justify-content: space-between;
    }
    .card-top { display: flex; justify-content: space-between; align-items: center; }
    .card-brand { font-weight: 800; font-size: 1.1rem; letter-spacing: 1px; }
    .card-number { margin: auto 0; }
    .card-number span { font-size: 1.3rem; letter-spacing: 3px; font-weight: 500; }
    .card-bottom { display: flex; justify-content: space-between; }
    .card-label { display: block; font-size: 0.6rem; color: rgba(255,255,255,0.6); letter-spacing: 1px; }
    .card-value { font-size: 0.85rem; font-weight: 600; }

    .show-number-btn {
      display: flex; align-items: center; gap: 8px; justify-content: center;
      width: 100%; margin-top: 16px; padding: 12px;
      background: none; border: none; cursor: pointer;
      color: var(--krt-primary); font-weight: 600; font-size: 0.88rem;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }

    .card-info { padding: 0 20px; max-width: 500px; margin: 0 auto; }
    .info-card {
      background: white; border-radius: var(--krt-radius); padding: 4px 20px;
      box-shadow: var(--krt-shadow-sm);
    }
    .info-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 16px 0; border-bottom: 1px solid var(--krt-divider);
    }
    .info-row:last-child { border-bottom: none; }
    .info-row span { font-size: 0.88rem; color: var(--krt-text-secondary); }
    .info-row strong { font-size: 0.95rem; }
    .info-row .success { color: var(--krt-success); }

    .action-btn {
      display: flex; align-items: center; gap: 8px; justify-content: center;
      width: 100%; margin-top: 16px; padding: 16px;
      background: white; border: 1.5px solid var(--krt-danger);
      border-radius: var(--krt-radius-sm); cursor: pointer;
      color: var(--krt-danger); font-weight: 600; font-size: 0.9rem;
      font-family: 'Plus Jakarta Sans', sans-serif;
      transition: all 0.2s;
    }
    .action-btn:hover { background: #FFF5F5; }
  `]
})
export class CardsPageComponent {
  showCardNumber = false;
  userName = localStorage.getItem('krt_account_name') || 'Usuário';
  constructor(public router: Router) {}
}
