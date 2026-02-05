import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-cards-page',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Meus Cartões</h1>
        <div style="width: 40px"></div>
      </header>

      <main class="container fade-in">
        <h3>Cartão Físico</h3>
        <div class="credit-card physical" [class.blocked]="isBlocked">
            <div class="card-overlay" *ngIf="isBlocked"><mat-icon>lock</mat-icon> BLOQUEADO</div>
            <div class="card-top">
                <mat-icon>contactless</mat-icon>
                <span class="bank-name">KRT Black</span>
            </div>
            <div class="card-chip"></div>
            <div class="card-number">•••• •••• •••• 8829</div>
            <div class="card-footer">
                <span class="card-holder">CLIENTE KRT</span>
                <span class="card-expiry">12/30</span>
            </div>
        </div>

        <div class="card-actions">
            <button mat-stroked-button [color]="isBlocked ? 'primary' : 'warn'" (click)="toggleBlock()">
                <mat-icon>{{ isBlocked ? 'lock_open' : 'lock' }}</mat-icon> 
                {{ isBlocked ? 'Desbloquear' : 'Bloquear' }}
            </button>
            <button mat-stroked-button><mat-icon>settings</mat-icon> Configurar</button>
        </div>

        <mat-card class="limit-card">
            <div class="limit-header">
                <span>Ajustar limite</span>
                <span class="limit-value">{{ limitValue | currency:'BRL' }}</span>
            </div>
            <mat-slider min="0" max="10000" step="100" showTickMarks discrete>
                <input matSliderThumb [(ngModel)]="limitValue">
            </mat-slider>
            <p class="limit-desc">R$ 2.450,00 gastos • R$ {{ limitValue - 2450 | number:'1.2-2' }} disponíveis</p>
        </mat-card>

        <mat-divider style="margin: 20px 0"></mat-divider>

        <div class="virtual-section">
            <div class="section-header">
                <h3>Cartão Virtual</h3>
                <span class="tag">Para compras online</span>
            </div>
            
            <div *ngIf="!hasVirtual" class="empty-virtual">
                <p>Você ainda não gerou seu cartão virtual.</p>
                <button mat-raised-button color="primary" (click)="createVirtual()">
                    <mat-icon>add_card</mat-icon> Gerar Cartão Virtual
                </button>
            </div>

            <div *ngIf="hasVirtual" class="virtual-card-row fade-in">
                <div class="mini-card">
                    <div class="mini-chip"></div>
                    <span>Final 4021</span>
                </div>
                <div class="virtual-info">
                    <span class="status active">Ativo</span>
                    <button mat-button color="primary">Ver dados</button>
                </div>
                <button mat-icon-button color="warn" (click)="deleteVirtual()"><mat-icon>delete</mat-icon></button>
            </div>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .header-simple { background: var(--bg-app); padding: 15px; display: flex; align-items: center; justify-content: space-between; }
    
    .credit-card {
        background: linear-gradient(135deg, #000000 0%, #1a1a1a 100%);
        color: white; border-radius: 16px; padding: 25px;
        box-shadow: 0 10px 20px rgba(0,0,0,0.3);
        margin-bottom: 20px; position: relative; overflow: hidden;
        height: 180px; display: flex; flex-direction: column; justify-content: space-between;
        transition: filter 0.3s;
    }
    .credit-card::before {
        content: ''; position: absolute; top: -50px; right: -50px;
        width: 150px; height: 150px; background: rgba(255,255,255,0.1);
        border-radius: 50%;
    }
    
    .credit-card.blocked { filter: grayscale(100%); }
    .card-overlay {
        position: absolute; top:0; left:0; width:100%; height:100%;
        background: rgba(0,0,0,0.6); z-index: 10;
        display: flex; flex-direction: column; align-items: center; justify-content: center;
        font-weight: bold; font-size: 1.2rem; letter-spacing: 2px;
    }

    .card-top { display: flex; justify-content: space-between; align-items: center; }
    .card-chip { width: 40px; height: 30px; background: #d4af37; border-radius: 4px; margin-top: 10px; }
    .card-number { font-family: 'Courier New', monospace; font-size: 1.4rem; letter-spacing: 2px; text-shadow: 0 1px 2px black; }
    .card-footer { display: flex; justify-content: space-between; font-size: 0.9rem; text-transform: uppercase; }
    
    .card-actions { display: flex; gap: 10px; margin-bottom: 20px; }
    .card-actions button { flex: 1; }

    /* Limit Card */
    .limit-card { padding: 20px; margin-bottom: 20px; }
    .limit-header { display: flex; justify-content: space-between; font-weight: 600; margin-bottom: 10px; }
    .limit-value { color: var(--primary); font-size: 1.1rem; }
    .limit-desc { font-size: 0.8rem; color: #666; margin-top: 5px; }
    mat-slider { width: 100%; }

    .tag { background: #e0f2f1; color: var(--primary); padding: 4px 8px; border-radius: 4px; font-size: 0.75rem; font-weight: 600; }
    .section-header { display: flex; align-items: center; gap: 10px; margin-bottom: 15px; }
    
    .empty-virtual { text-align: center; padding: 20px; background: white; border-radius: 12px; border: 1px dashed #ccc; }
    
    .virtual-card-row { 
        display: flex; align-items: center; background: white; padding: 15px; border-radius: 12px; 
        box-shadow: 0 2px 8px rgba(0,0,0,0.05); gap: 15px;
    }
    .mini-card { 
        width: 50px; height: 35px; background: var(--primary); border-radius: 4px; 
        display: flex; flex-direction: column; justify-content: center; align-items: center; color: white; font-size: 0.6rem;
    }
    .mini-chip { width: 10px; height: 8px; background: #d4af37; border-radius: 2px; margin-bottom: 2px; }
    .virtual-info { flex: 1; display: flex; flex-direction: column; align-items: flex-start; }
    .status.active { color: var(--accent); font-weight: 600; font-size: 0.8rem; }
  `]
})
export class CardsPageComponent {
  hasVirtual = false;
  isBlocked = false;
  limitValue = 5000;

  constructor(private location: Location, private notify: NotificationService) {}

  toggleBlock() {
      this.isBlocked = !this.isBlocked;
      if(this.isBlocked) this.notify.error('Cartão bloqueado temporariamente.');
      else this.notify.success('Cartão desbloqueado.');
  }

  createVirtual() { this.hasVirtual = true; this.notify.success('Cartão virtual gerado!'); }
  deleteVirtual() { this.hasVirtual = false; this.notify.error('Cartão virtual excluído.'); }
  goBack() { this.location.back(); }
}
