import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-receipt',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>close</mat-icon></button>
        <h1>Comprovante</h1>
        <button mat-icon-button (click)="share()"><mat-icon>share</mat-icon></button>
      </header>

      <main class="container fade-in">
        <div class="receipt-paper">
            <div class="r-icon"><mat-icon>check_circle</mat-icon></div>
            <h2>Transferência realizada</h2>
            <p class="r-date">{{ date | date:'dd/MM/yyyy - HH:mm:ss' }}</p>
            
            <div class="r-amount">- R$ 150,00</div>
            
            <mat-divider class="dashed"></mat-divider>
            
            <div class="r-row">
                <span class="label">Tipo</span>
                <span class="value">Pix Enviado</span>
            </div>
            <div class="r-row">
                <span class="label">Para</span>
                <span class="value">Maria da Silva</span>
            </div>
            <div class="r-row">
                <span class="label">Instituição</span>
                <span class="value">Nubank S.A.</span>
            </div>
            <div class="r-row">
                <span class="label">CPF/CNPJ</span>
                <span class="value">***.456.789-**</span>
            </div>
            
            <mat-divider class="dashed"></mat-divider>
            
            <div class="r-row">
                <span class="label">Autenticação</span>
                <span class="value hash">{{ id }}</span>
            </div>
        </div>

        <button mat-raised-button color="primary" class="share-btn" (click)="share()">
            COMPARTILHAR COMPROVANTE
        </button>
      </main>
    </div>
  `,
  styles: [`
    .app-layout { background: #eee; } /* Fundo cinza pra destacar o papel */
    .receipt-paper {
        background: white; padding: 30px 20px; border-radius: 0 0 16px 16px;
        position: relative; margin-bottom: 20px;
        box-shadow: 0 4px 15px rgba(0,0,0,0.05);
        display: flex; flex-direction: column; align-items: center;
    }
    /* Efeito de papel rasgado no topo */
    .receipt-paper::before {
        content: ""; position: absolute; top: -10px; left: 0; width: 100%; height: 10px;
        background: white; /* Simples fix */
    }

    .r-icon { color: var(--accent); transform: scale(2); margin-bottom: 15px; }
    h2 { margin: 10px 0 5px; font-size: 1.2rem; font-weight: 700; color: #333; }
    .r-date { color: #888; font-size: 0.85rem; margin-bottom: 20px; }
    .r-amount { font-size: 1.8rem; font-weight: 700; color: #333; margin-bottom: 20px; }
    
    .dashed { border-top-style: dashed !important; width: 100%; margin: 15px 0; }
    
    .r-row { width: 100%; display: flex; justify-content: space-between; margin-bottom: 12px; font-size: 0.9rem; }
    .label { color: #666; }
    .value { font-weight: 600; color: #333; text-align: right; }
    .hash { font-family: monospace; font-size: 0.75rem; color: #999; word-break: break-all; max-width: 150px; }
    
    .share-btn { width: 100%; padding: 25px !important; }
  `]
})
export class ReceiptComponent implements OnInit {
  id = '';
  date = new Date();

  constructor(private location: Location, private route: ActivatedRoute, private notify: NotificationService) {}

  ngOnInit() {
      this.id = this.route.snapshot.paramMap.get('id') || 'TX-UNKNOWN';
  }

  share() {
      this.notify.success('Comprovante enviado para compartilhamento.');
  }
  
  goBack() { this.location.back(); }
}
