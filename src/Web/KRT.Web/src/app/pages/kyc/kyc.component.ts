import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-kyc',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './kyc.component.html',
  styleUrls: ['./kyc.component.scss']
})
export class KycComponent implements OnInit {
  accountId = '';
  kyc: any = null;
  loading = false;

  docType = 'RG';
  docTypes = ['RG', 'CNH', 'Passaporte'];
  confirmForm = { fullName: '', cpf: '', birthDate: '', motherName: '' };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    this.loadKyc();
  }

  loadKyc(): void {
    this.http.get<any>(`${environment.apiUrl}/kyc/${this.accountId}`).subscribe(data => this.kyc = data);
  }

  uploadDocument(): void {
    // Simulando upload com base64 mock
    const mockBase64 = 'data:image/jpeg;base64,/9j/4AAQ...mock';
    this.http.post(`${environment.apiUrl}/kyc/${this.accountId}/document`, {
      documentType: this.docType, base64Data: mockBase64, fileName: `${this.docType.toLowerCase()}.jpg`
    }).subscribe({ next: () => this.loadKyc(), error: (e) => alert(e.error?.error || 'Erro') });
  }

  uploadSelfie(): void {
    const mockBase64 = 'data:image/jpeg;base64,/9j/4AAQ...selfie';
    this.http.post(`${environment.apiUrl}/kyc/${this.accountId}/selfie`, { base64Data: mockBase64 })
      .subscribe({ next: () => this.loadKyc(), error: (e) => alert(e.error?.error || 'Erro') });
  }

  confirmData(): void {
    this.http.post(`${environment.apiUrl}/kyc/${this.accountId}/confirm`, this.confirmForm)
      .subscribe({ next: () => this.loadKyc(), error: (e) => alert(e.error?.error || 'Erro') });
  }

  getStepNumber(): number {
    return { 'document': 1, 'selfie': 2, 'confirm': 3, 'review': 4 }[this.kyc?.currentStep] || 1;
  }
}