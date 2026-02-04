import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-receipt',
  template: `
    <div class="app-layout grey-bg">
      <header class="header-transparent">
        <button mat-icon-button (click)="goBack()"><mat-icon>close</mat-icon></button>
      </header>

      <main class="container">
        <div class="receipt-paper fade-in">
            <div class="receipt-icon">
                <mat-icon>check_circle</mat-icon>
            </div>
            <h2>Transferência enviada</h2>
            <div class="amount">- R$ 150,00</div>
            <div class="date">04 FEV 2026 - 15:42</div>

            <mat-divider class="dashed"></mat-divider>

            <div class="detail-row">
                <span class="label">Tipo</span>
                <span class="value">Pix Transferência</span>
            </div>
            <div class="detail-row">
                <span class="label">Destino</span>
                <span class="value">Maria Silva</span>
            </div>
            <div class="detail-row">
                <span class="label">Instituição</span>
                <span class="value">Nubank</span>
            </div>
            <div class="detail-row">
                <span class="label">CPF</span>
                <span class="value">***.452.111-**</span>
            </div>

            <mat-divider class="dashed"></mat-divider>

            <div class="auth-code">
                <small>ID da transação:</small>
                <code>E9293829382938293829</code>
            </div>

            <button mat-flat-button color="primary" class="share-btn">
                <mat-icon>share</mat-icon> Compartilhar
            </button>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .grey-bg { background-color: #333; min-height: 100vh; }
    .header-transparent { padding: 15px; }
    .header-transparent button { color: white; }

    .receipt-paper {
        background: #fffdf0; /* Cor de papel levemente amarelado/off-white */
        margin: 20px auto;
        padding: 30px 20px;
        border-radius: 0; /* Papel não tem borda arredondada ou tem serrilhado */
        mask-image: radial-gradient(circle at 10px bottom, transparent 10px, black 11px);
        /* Simulação visual de ticket */
        position: relative;
        text-align: center;
        box-shadow: 0 10px 30px rgba(0,0,0,0.5);
    }
    
    .receipt-icon mat-icon { font-size: 50px; width: 50px; height: 50px; color: var(--accent); margin-bottom: 10px; }
    h2 { font-size: 1.1rem; margin-bottom: 5px; }
    .amount { font-size: 2rem; font-weight: 700; color: #333; margin-bottom: 5px; }
    .date { color: #888; font-size: 0.8rem; margin-bottom: 20px; }
    
    .dashed { border-top-style: dashed !important; margin: 20px 0; }
    
    .detail-row { display: flex; justify-content: space-between; margin-bottom: 12px; font-size: 0.9rem; }
    .detail-row .label { color: #666; }
    .detail-row .value { font-weight: 600; }
    
    .auth-code { text-align: left; margin-top: 20px; margin-bottom: 20px; background: #f0f0f0; padding: 10px; border-radius: 4px; }
    .auth-code small { display: block; color: #999; font-size: 0.7rem; }
    .auth-code code { font-family: monospace; word-break: break-all; }
    
    .share-btn { width: 100%; border-radius: 8px; }
  `]
})
export class ReceiptComponent implements OnInit {
  constructor(private location: Location, private route: ActivatedRoute) {}
  ngOnInit() {
     // Aqui pegariamos o ID da rota para buscar na API
  }
  goBack() { this.location.back(); }
}
