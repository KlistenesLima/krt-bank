import { Component } from '@angular/core';
import { PaymentService } from '../../../core/services/payment.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-pix-area',
  template: `
    <div class="container">
      <h2>Área PIX</h2>
      <div>
        <label>Chave do Recebedor (CPF/Email):</label>
        <input [(ngModel)]="receiverKey">
      </div>
      <div>
        <label>Valor:</label>
        <input type="number" [(ngModel)]="amount">
      </div>
      <button (click)="send()">Enviar Pix</button>
      <button (click)="back()">Voltar</button>
    </div>
  `
})
export class PixAreaComponent {
  accountId = localStorage.getItem('krt_account_id');
  receiverKey = '';
  amount = 0;

  constructor(private paymentService: PaymentService, private router: Router) {}

  send() {
    if (!this.accountId) return;
    
    this.paymentService.sendPix({
        accountId: this.accountId,
        receiverKey: this.receiverKey,
        amount: this.amount
    }).subscribe({
        next: () => {
            alert('Pix enviado com sucesso!');
            this.router.navigate(['/dashboard']);
        },
        error: (err) => alert('Erro no Pix: ' + JSON.stringify(err))
    });
  }

  back() {
    this.router.navigate(['/dashboard']);
  }
}
