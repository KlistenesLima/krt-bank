import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-payment-success',
  template: `
    <div class="full-screen-success fade-in">
        <div class="content">
            <div class="icon-pulse">
                <mat-icon>check</mat-icon>
            </div>
            <h1>Pronto!</h1>
            <p>Sua transação foi realizada com sucesso.</p>
            
            <button mat-stroked-button class="white-btn" (click)="viewReceipt()">Ver Comprovante</button>
            <br><br>
            <button mat-button class="white-text" (click)="goHome()">Voltar ao início</button>
        </div>
    </div>
  `,
  styles: [`
    .full-screen-success {
        height: 100vh; width: 100vw;
        background: var(--accent); /* Verde Sucesso */
        display: flex; align-items: center; justify-content: center;
        color: #004d40; text-align: center;
    }
    .icon-pulse {
        width: 80px; height: 80px; background: white; border-radius: 50%;
        display: flex; align-items: center; justify-content: center;
        margin: 0 auto 20px;
        color: var(--accent);
        box-shadow: 0 0 0 0 rgba(255, 255, 255, 0.7);
        animation: pulse-white 2s infinite;
    }
    .icon-pulse mat-icon { font-size: 40px; width: 40px; height: 40px; font-weight: bold; }
    
    h1 { font-size: 2.5rem; margin-bottom: 10px; font-weight: 700; }
    p { font-size: 1.1rem; margin-bottom: 40px; opacity: 0.9; }

    .white-btn { border: 2px solid white; color: white; width: 200px; font-weight: bold; }
    .white-text { color: white; opacity: 0.8; }

    @keyframes pulse-white {
        0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(255, 255, 255, 0.7); }
        70% { transform: scale(1); box-shadow: 0 0 0 20px rgba(255, 255, 255, 0); }
        100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(255, 255, 255, 0); }
    }
  `]
})
export class PaymentSuccessComponent {
  constructor(private router: Router) {}
  viewReceipt() { this.router.navigate(['/receipt/123']); }
  goHome() { this.router.navigate(['/dashboard']); }
}
