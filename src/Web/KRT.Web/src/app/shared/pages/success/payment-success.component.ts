import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-payment-success',
  template: `
    <div class="success-bg" [class.show]="show">
      <!-- Confetti particles -->
      <div class="confetti-container">
        <div *ngFor="let c of confetti" class="confetti-piece"
             [style.left]="c.left" [style.background]="c.color"
             [style.animation-delay]="c.delay" [style.animation-duration]="c.duration"
             [style.width]="c.size" [style.height]="c.size"></div>
      </div>

      <div class="success-card" [class.pop]="showCard">
        <!-- Animated check circle -->
        <div class="check-container" [class.animate]="showCheck">
          <svg viewBox="0 0 120 120" class="check-svg">
            <!-- Glow pulse -->
            <circle cx="60" cy="60" r="54" class="glow-ring"/>
            <!-- Background circle -->
            <circle cx="60" cy="60" r="50" class="circle-bg"/>
            <!-- Animated stroke circle -->
            <circle cx="60" cy="60" r="50" class="circle-stroke"/>
            <!-- Check mark -->
            <polyline points="38,62 52,76 82,46" class="check-mark"/>
          </svg>
        </div>

        <h1 class="title" [class.show]="showText">Pronto!</h1>
        <p class="subtitle" [class.show]="showText">
          Sua transacao foi realizada<br>com sucesso
        </p>

        <div class="amount-badge" [class.show]="showAmount" *ngIf="amount">
          <span class="amount-label">Valor transferido</span>
          <span class="amount-value">R$ {{ amount }}</span>
        </div>

        <div class="actions" [class.show]="showActions">
          <button class="btn-receipt" (click)="viewReceipt()">
            <mat-icon>receipt_long</mat-icon>
            Ver comprovante
          </button>

          <button class="btn-share" (click)="share()">
            <mat-icon>share</mat-icon>
            Compartilhar
          </button>

          <button class="btn-home" (click)="goHome()">
            <mat-icon>home</mat-icon>
            Voltar ao inicio
          </button>
        </div>

        <p class="timestamp" [class.show]="showActions">
          {{ currentDate | date:'dd/MM/yyyy HH:mm:ss' }}
        </p>
      </div>
    </div>
  `,
  styles: [`
    /* === BG === */
    .success-bg {
      min-height: 100vh; display: flex; align-items: center; justify-content: center;
      background: linear-gradient(135deg, #059669 0%, #047857 40%, #065f46 100%);
      padding: 20px; position: relative; overflow: hidden;
      opacity: 0; transition: opacity 0.4s;
    }
    .success-bg.show { opacity: 1; }

    /* === CONFETTI === */
    .confetti-container {
      position: absolute; inset: 0; pointer-events: none; overflow: hidden;
    }
    .confetti-piece {
      position: absolute; top: -20px;
      border-radius: 2px;
      animation: confettiFall linear forwards;
      opacity: 0;
    }
    @keyframes confettiFall {
      0% { transform: translateY(-10vh) rotate(0deg); opacity: 1; }
      25% { opacity: 1; }
      100% { transform: translateY(110vh) rotate(720deg); opacity: 0; }
    }

    /* === CARD === */
    .success-card {
      background: rgba(255,255,255,0.12);
      backdrop-filter: blur(20px);
      border: 1px solid rgba(255,255,255,0.2);
      border-radius: 32px; padding: 48px 40px;
      text-align: center; max-width: 400px; width: 100%;
      position: relative; z-index: 1;
      transform: scale(0.8); opacity: 0;
      transition: all 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
    }
    .success-card.pop { transform: scale(1); opacity: 1; }

    /* === ANIMATED CHECK === */
    .check-container { width: 120px; height: 120px; margin: 0 auto 28px; }
    .check-svg { width: 100%; height: 100%; }

    .glow-ring {
      fill: none; stroke: rgba(255,255,255,0.15); stroke-width: 8;
      transform-origin: center;
      animation: none; opacity: 0;
    }
    .check-container.animate .glow-ring {
      animation: pulseGlow 2s ease-in-out infinite;
      opacity: 1;
    }

    .circle-bg { fill: rgba(255,255,255,0.15); }

    .circle-stroke {
      fill: none; stroke: white; stroke-width: 4;
      stroke-dasharray: 314; stroke-dashoffset: 314;
      stroke-linecap: round; transform-origin: center;
      transform: rotate(-90deg);
    }
    .check-container.animate .circle-stroke {
      animation: drawCircle 0.6s ease forwards;
    }

    .check-mark {
      fill: none; stroke: white; stroke-width: 5;
      stroke-linecap: round; stroke-linejoin: round;
      stroke-dasharray: 80; stroke-dashoffset: 80;
    }
    .check-container.animate .check-mark {
      animation: drawCheck 0.4s 0.5s ease forwards;
    }

    @keyframes drawCircle {
      to { stroke-dashoffset: 0; }
    }
    @keyframes drawCheck {
      to { stroke-dashoffset: 0; }
    }
    @keyframes pulseGlow {
      0%, 100% { r: 54; opacity: 0.3; }
      50% { r: 58; opacity: 0.6; }
    }

    /* === TEXT === */
    .title {
      font-size: 2.2rem; font-weight: 800; color: white; margin: 0 0 8px;
      opacity: 0; transform: translateY(15px);
      transition: all 0.5s ease;
    }
    .title.show { opacity: 1; transform: translateY(0); }

    .subtitle {
      font-size: 1.05rem; color: rgba(255,255,255,0.8); margin: 0 0 28px;
      line-height: 1.5;
      opacity: 0; transform: translateY(15px);
      transition: all 0.5s ease 0.1s;
    }
    .subtitle.show { opacity: 1; transform: translateY(0); }

    /* === AMOUNT === */
    .amount-badge {
      background: rgba(255,255,255,0.15); border: 1px solid rgba(255,255,255,0.25);
      border-radius: 16px; padding: 16px 24px; margin-bottom: 32px;
      display: inline-flex; flex-direction: column; gap: 4px;
      opacity: 0; transform: scale(0.9);
      transition: all 0.4s ease;
    }
    .amount-badge.show { opacity: 1; transform: scale(1); }
    .amount-label { font-size: 0.78rem; color: rgba(255,255,255,0.6); text-transform: uppercase; letter-spacing: 0.5px; }
    .amount-value { font-size: 1.6rem; font-weight: 800; color: white; }

    /* === ACTIONS === */
    .actions {
      display: flex; flex-direction: column; gap: 12px;
      opacity: 0; transform: translateY(15px);
      transition: all 0.5s ease;
    }
    .actions.show { opacity: 1; transform: translateY(0); }

    .btn-receipt, .btn-share, .btn-home {
      width: 100%; height: 50px; border-radius: 14px;
      display: flex; align-items: center; justify-content: center; gap: 8px;
      font-size: 0.92rem; font-weight: 700; cursor: pointer;
      font-family: 'Plus Jakarta Sans', sans-serif;
      transition: all 0.25s;
    }
    .btn-receipt {
      background: white; color: #059669; border: none;
      box-shadow: 0 4px 16px rgba(0,0,0,0.15);
    }
    .btn-receipt:hover { transform: translateY(-2px); box-shadow: 0 8px 24px rgba(0,0,0,0.2); }
    .btn-receipt mat-icon { font-size: 20px; width: 20px; height: 20px; }

    .btn-share {
      background: rgba(255,255,255,0.15); color: white;
      border: 1.5px solid rgba(255,255,255,0.3);
    }
    .btn-share:hover { background: rgba(255,255,255,0.25); }
    .btn-share mat-icon { font-size: 20px; width: 20px; height: 20px; }

    .btn-home {
      background: transparent; color: rgba(255,255,255,0.7);
      border: none;
    }
    .btn-home:hover { color: white; }
    .btn-home mat-icon { font-size: 20px; width: 20px; height: 20px; }

    .timestamp {
      margin-top: 20px; font-size: 0.78rem; color: rgba(255,255,255,0.4);
      opacity: 0; transition: opacity 0.5s ease 0.3s;
    }
    .timestamp.show { opacity: 1; }

    /* Responsive */
    @media (max-width: 440px) {
      .success-card { padding: 36px 24px; }
      .title { font-size: 1.8rem; }
      .amount-value { font-size: 1.3rem; }
    }
  `]
})
export class PaymentSuccessComponent implements OnInit {
  show = false;
  showCard = false;
  showCheck = false;
  showText = false;
  showAmount = false;
  showActions = false;
  amount = '';
  currentDate = new Date();
  confetti: any[] = [];

  constructor(private router: Router) {}

  ngOnInit() {
    this.amount = history.state?.amount || '';
    this.generateConfetti();

    // Staggered reveal
    setTimeout(() => this.show = true, 50);
    setTimeout(() => this.showCard = true, 200);
    setTimeout(() => this.showCheck = true, 500);
    setTimeout(() => this.showText = true, 900);
    setTimeout(() => this.showAmount = true, 1100);
    setTimeout(() => this.showActions = true, 1300);
  }

  generateConfetti() {
    const colors = ['#FFD700', '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEAA7', '#DDA0DD', '#98D8C8', '#F7DC6F', '#BB8FCE'];
    this.confetti = Array.from({ length: 60 }, (_, i) => ({
      left: Math.random() * 100 + '%',
      color: colors[i % colors.length],
      delay: (Math.random() * 2) + 's',
      duration: (Math.random() * 2 + 2.5) + 's',
      size: (Math.random() * 8 + 4) + 'px'
    }));
  }

  viewReceipt() {
    const txId = history.state?.transactionId || '123';
    this.router.navigate(['/statement']);
  }

  share() {
    if (navigator.share) {
      navigator.share({
        title: 'Comprovante PIX - KRT Bank',
        text: 'Transferencia PIX realizada com sucesso' + (this.amount ? ' - R$ ' + this.amount : ''),
      }).catch(() => {});
    } else {
      const text = 'PIX realizado com sucesso' + (this.amount ? ' - R$ ' + this.amount : '') + ' via KRT Bank';
      navigator.clipboard.writeText(text).then(() => {
        alert('Copiado para a area de transferencia!');
      });
    }
  }

  goHome() { this.router.navigate(['/dashboard']); }
}
