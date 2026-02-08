import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-investments',
  template: `
    <div class="invest-container page-with-nav">
      <header class="page-header">
        <button class="back-btn" (click)="router.navigate(['/dashboard'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Investimentos</h1>
        <div style="width:40px"></div>
      </header>

      <div class="content fade-in">
        <!-- Patrimônio total -->
        <div class="patrimony-card">
          <span class="patrimony-label">Patrimônio investido</span>
          <div class="patrimony-row">
            <span class="patrimony-value">R$ 12.500,00</span>
            <span class="patrimony-badge">+2,4%</span>
          </div>
          <span class="patrimony-hint">Rendimento este mês: R$ 98,75</span>
        </div>

        <!-- Meus investimentos -->
        <div class="section-label">Meus investimentos</div>

        <div class="invest-card" *ngFor="let inv of myInvestments">
          <div class="invest-icon" [style.background]="inv.color + '15'">
            <mat-icon [style.color]="inv.color">{{ inv.icon }}</mat-icon>
          </div>
          <div class="invest-info">
            <span class="invest-name">{{ inv.name }}</span>
            <span class="invest-detail">{{ inv.detail }}</span>
          </div>
          <div class="invest-right">
            <span class="invest-amount">{{ inv.amount }}</span>
            <span class="invest-yield" [class.positive]="inv.positive">{{ inv.yield }}</span>
          </div>
        </div>

        <!-- Descobrir -->
        <div class="section-label" style="margin-top:32px">Descubra novos investimentos</div>

        <div class="discover-grid">
          <div class="discover-card" *ngFor="let d of discover" (click)="d.expanded = !d.expanded">
            <div class="discover-icon" [style.background]="d.color + '12'">
              <mat-icon [style.color]="d.color">{{ d.icon }}</mat-icon>
            </div>
            <div class="discover-info">
              <span class="discover-name">{{ d.name }}</span>
              <span class="discover-desc">{{ d.desc }}</span>
            </div>
            <div class="discover-badge">{{ d.badge }}</div>
          </div>
        </div>

        <!-- CTA -->
        <div class="cta-card">
          <mat-icon>lightbulb</mat-icon>
          <div>
            <strong>Dica KRT</strong>
            <p>Seu dinheiro na conta rende automaticamente 100% do CDI. Sem taxa, sem burocracia.</p>
          </div>
        </div>
      </div>
    </div>

    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .invest-container { min-height: 100vh; background: var(--krt-bg); }

    .page-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 20px; background: #fff; border-bottom: 1px solid #F0F0F0;
    }
    .page-header h1 { font-size: 1.05rem; font-weight: 700; margin: 0; color: #1A1A2E; }
    .back-btn { background: none; border: none; cursor: pointer; padding: 4px; color: #1A1A2E; display: flex; }

    .content { padding: 24px 20px; max-width: 480px; margin: 0 auto; padding-bottom: 100px; }

    /* Patrimônio */
    .patrimony-card {
      background: linear-gradient(135deg, #0047BB, #002a70);
      border-radius: 20px; padding: 24px; margin-bottom: 28px;
      box-shadow: 0 8px 30px rgba(0,71,187,0.25);
    }
    .patrimony-label { font-size: 0.82rem; color: rgba(255,255,255,0.7); }
    .patrimony-row { display: flex; align-items: center; gap: 12px; margin: 6px 0 8px; }
    .patrimony-value { font-size: 1.7rem; font-weight: 800; color: #fff; }
    .patrimony-badge {
      background: rgba(0,200,83,0.2); color: #69F0AE;
      padding: 4px 12px; border-radius: 20px; font-size: 0.78rem; font-weight: 700;
    }
    .patrimony-hint { font-size: 0.8rem; color: rgba(255,255,255,0.6); }

    .section-label { font-size: 1rem; font-weight: 700; color: #1A1A2E; margin-bottom: 16px; }

    /* Meus investimentos */
    .invest-card {
      display: flex; align-items: center; gap: 14px;
      background: #fff; border-radius: 16px; padding: 18px 16px;
      margin-bottom: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.04);
    }
    .invest-icon {
      width: 46px; height: 46px; border-radius: 14px;
      display: flex; align-items: center; justify-content: center; flex-shrink: 0;
    }
    .invest-icon mat-icon { font-size: 22px; width: 22px; height: 22px; }
    .invest-info { flex: 1; display: flex; flex-direction: column; gap: 2px; }
    .invest-name { font-size: 0.92rem; font-weight: 600; color: #1A1A2E; }
    .invest-detail { font-size: 0.78rem; color: #9CA3AF; }
    .invest-right { text-align: right; display: flex; flex-direction: column; gap: 2px; }
    .invest-amount { font-size: 0.95rem; font-weight: 700; color: #1A1A2E; }
    .invest-yield { font-size: 0.78rem; font-weight: 600; color: #E53935; }
    .invest-yield.positive { color: #00C853; }

    /* Descobrir */
    .discover-grid { display: flex; flex-direction: column; gap: 10px; }
    .discover-card {
      display: flex; align-items: center; gap: 14px;
      background: #fff; border-radius: 16px; padding: 18px 16px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04); cursor: pointer;
      transition: all 0.2s;
    }
    .discover-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.08); }
    .discover-icon {
      width: 46px; height: 46px; border-radius: 14px;
      display: flex; align-items: center; justify-content: center; flex-shrink: 0;
    }
    .discover-icon mat-icon { font-size: 22px; width: 22px; height: 22px; }
    .discover-info { flex: 1; display: flex; flex-direction: column; gap: 2px; }
    .discover-name { font-size: 0.92rem; font-weight: 600; color: #1A1A2E; }
    .discover-desc { font-size: 0.78rem; color: #9CA3AF; }
    .discover-badge {
      background: #F0F4FF; color: #0047BB; padding: 4px 12px;
      border-radius: 20px; font-size: 0.72rem; font-weight: 700; white-space: nowrap;
    }

    /* CTA */
    .cta-card {
      display: flex; align-items: flex-start; gap: 14px;
      background: linear-gradient(135deg, rgba(0,212,170,0.08), rgba(0,71,187,0.06));
      border: 1px solid rgba(0,212,170,0.2);
      border-radius: 16px; padding: 18px 16px; margin-top: 28px;
    }
    .cta-card mat-icon { color: #00D4AA; font-size: 28px; width: 28px; height: 28px; margin-top: 2px; }
    .cta-card strong { font-size: 0.9rem; color: #1A1A2E; display: block; margin-bottom: 4px; }
    .cta-card p { font-size: 0.82rem; color: #6B7280; margin: 0; line-height: 1.4; }
  `]
})
export class InvestmentsComponent {
  constructor(public router: Router) {}

  myInvestments = [
    { name: 'Cofre KRT', detail: 'Liquidez diária · 100% CDI', amount: 'R$ 8.200,00', yield: '+R$ 62,30', positive: true, icon: 'savings', color: '#0047BB' },
    { name: 'CDB 120%', detail: 'Vencimento: Mar/2026', amount: 'R$ 3.000,00', yield: '+R$ 28,50', positive: true, icon: 'trending_up', color: '#00C853' },
    { name: 'Fundo Multimercado', detail: 'Risco moderado', amount: 'R$ 1.300,00', yield: '+R$ 7,95', positive: true, icon: 'show_chart', color: '#FF6B35' },
  ];

  discover = [
    { name: 'Tesouro Selic', desc: 'Renda fixa · Baixo risco', badge: '13,25% a.a.', icon: 'account_balance', color: '#0047BB', expanded: false },
    { name: 'CDB 130% CDI', desc: 'Prazo 2 anos · FGC', badge: '130% CDI', icon: 'lock', color: '#7C3AED', expanded: false },
    { name: 'Fundos Imobiliários', desc: 'Dividendos mensais', badge: '~0,85% a.m.', icon: 'apartment', color: '#FF6B35', expanded: false },
    { name: 'Ações', desc: 'Renda variável · Alto risco', badge: 'Variável', icon: 'candlestick_chart', color: '#E53935', expanded: false },
  ];
}
