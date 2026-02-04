import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PaymentService } from '../../core/services/payment.service';

@Component({
  selector: 'app-pix-area',
  template: \
    <div class="card">
      <h2>Realizar Transferência PIX</h2>
      <form [formGroup]="pixForm" (ngSubmit)="onSubmit()">
        <div style="margin-bottom: 10px">
          <label>Chave Pix Destino:</label><br>
          <input formControlName="key" type="text" style="width: 100%; padding: 8px;">
        </div>
        
        <div style="margin-bottom: 10px">
          <label>Valor (R$):</label><br>
          <input formControlName="amount" type="number" style="width: 100%; padding: 8px;">
        </div>

        <button type="submit" [disabled]="pixForm.invalid" 
                style="background: #004d40; color: white; padding: 10px 20px; border: none; cursor: pointer;">
          Enviar Pix
        </button>
      </form>
    </div>
  \,
  styles: [\.card { border: 1px solid #ccc; padding: 20px; border-radius: 8px; max-width: 400px; }\]
})
export class PixAreaComponent {
  pixForm: FormGroup;

  constructor(private fb: FormBuilder, private payService: PaymentService) {
    this.pixForm = this.fb.group({
      accountId: ['3fa85f64-5717-4562-b3fc-2c963f66afa6'], // Mock ID fixo para teste
      key: ['', Validators.required],
      amount: [0, [Validators.required, Validators.min(0.01)]]
    });
  }

  onSubmit() {
    if (this.pixForm.valid) {
      this.payService.sendPix(this.pixForm.value).subscribe({
        next: (res: any) => alert('Pix realizado! Status: ' + res.status),
        error: (err) => alert('Falha no Pix: ' + JSON.stringify(err))
      });
    }
  }
}
