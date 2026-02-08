import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-pix-qrcode',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pix-qrcode.component.html',
  styleUrls: ['./pix-qrcode.component.scss']
})
export class PixQrcodeComponent {
  activeTab: 'generate' | 'read' = 'generate';
  
  // Gerar QR Code
  pixKey = '';
  amount: number = 0;
  merchantName = 'KRT Bank';
  qrCodeImage: string | null = null;
  pixPayload: string | null = null;
  copySuccess = false;
  loading = false;

  // Ler QR Code
  readPixKey = '';
  readAmount = '';
  readResult: any = null;

  // Limites
  limits: any = null;

  constructor(private http: HttpClient) {}

  generateQrCode(): void {
    if (!this.pixKey || this.amount <= 0) return;
    this.loading = true;

    this.http.post<any>(`${environment.apiUrl}/pix/qrcode/generate`, {
      pixKey: this.pixKey,
      amount: this.amount,
      merchantName: this.merchantName
    }).subscribe({
      next: (res) => {
        this.qrCodeImage = res.qrCodeDataUrl;
        this.pixPayload = res.payload;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  copyPayload(): void {
    if (this.pixPayload) {
      navigator.clipboard.writeText(this.pixPayload);
      this.copySuccess = true;
      setTimeout(() => this.copySuccess = false, 2000);
    }
  }

  onQrFileSelected(event: any): void {
    const file = event.target.files[0];
    if (!file) return;
    
    // Simula leitura — em producao usaria uma lib de QR decode
    this.readResult = {
      pixKey: '10626054460',
      amount: 'R$ 150,00',
      merchant: 'KRT Bank',
      message: 'QR Code lido com sucesso (simulado)'
    };
  }

  loadLimits(): void {
    const accountId = localStorage.getItem('account_id');
    if (!accountId) return;

    this.http.get<any>(`${environment.apiUrl}/pix/limits/${accountId}`)
      .subscribe(res => this.limits = res);
  }

  downloadReceipt(transactionId: string): void {
    window.open(`${environment.apiUrl}/pix/receipt/${transactionId}`, '_blank');
  }
}