import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-boleto',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>close</mat-icon></button>
        <h1>Pagar Boleto</h1>
        <div style="width: 40px"></div>
      </header>

      <main class="container fade-in">
        <div class="scan-area">
            <div class="scan-line"></div>
            <p>Posicione o código de barras na linha</p>
            <button mat-stroked-button color="white" class="scan-btn">
                <mat-icon>camera_alt</mat-icon> Usar Câmera
            </button>
        </div>

        <mat-card class="input-card">
            <h3>Ou digite o código</h3>
            <mat-form-field appearance="fill" class="full-width">
                <mat-label>Código do boleto</mat-label>
                <input matInput placeholder="00000.00000 00000.000000..." [(ngModel)]="barcode">
                <mat-icon matSuffix>qr_code</mat-icon>
            </mat-form-field>
            
            <button mat-raised-button color="primary" class="full-btn" (click)="pay()" [disabled]="!barcode">
                CONTINUAR
            </button>
        </mat-card>
      </main>
    </div>
  `,
  styles: [`
    .scan-area {
        background: #222; color: white; height: 200px; border-radius: 16px;
        display: flex; flex-direction: column; align-items: center; justify-content: center;
        margin-bottom: 20px; position: relative; overflow: hidden;
    }
    .scan-line { width: 80%; height: 2px; background: var(--accent); box-shadow: 0 0 10px var(--accent); margin-bottom: 15px; }
    .scan-btn { color: white; border-color: white; margin-top: 10px; }
    .input-card { padding: 20px; }
    .full-width { width: 100%; }
    .full-btn { width: 100%; margin-top: 10px; }
  `]
})
export class BoletoComponent {
  barcode = '';
  constructor(private location: Location, private router: Router) {}
  
  pay() { this.router.navigate(['/success']); }
  goBack() { this.location.back(); }
}
