import { Component } from '@angular/core';
import { Location } from '@angular/common';

@Component({
  selector: 'app-pix-page',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>close</mat-icon></button>
        <h1>Área Pix</h1>
        <div style="width: 40px"></div>
      </header>

      <main class="container fade-in">
        <div class="pix-menu">
           <button mat-raised-button color="primary" class="big-btn">
             <mat-icon>qr_code_scanner</mat-icon> Ler QR Code
           </button>
           <div class="row-btns">
             <button mat-stroked-button><mat-icon>content_copy</mat-icon> Pix Copia e Cola</button>
             <button mat-stroked-button><mat-icon>vpn_key</mat-icon> Minhas Chaves</button>
           </div>
        </div>

        <h3>Transferir</h3>
        <mat-card class="transfer-card">
          <mat-form-field appearance="outline">
            <mat-label>Chave Pix (CPF, Email ou Aleatória)</mat-label>
            <input matInput placeholder="Digite a chave">
            <mat-icon matSuffix>search</mat-icon>
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Valor (R$)</mat-label>
            <input matInput type="number" placeholder="0,00">
          </mat-form-field>

          <button mat-raised-button color="primary" class="send-btn">
            AVANÇAR
          </button>
        </mat-card>
      </main>
    </div>
  `,
  styles: [`
    .header-simple { 
        background: #f5f7fa; color: #333; padding: 15px; 
        display: flex; align-items: center; justify-content: space-between;
    }
    .pix-menu { display: flex; flex-direction: column; gap: 10px; margin-bottom: 30px; margin-top: 10px; }
    .big-btn { padding: 30px !important; font-size: 1.1rem; }
    .row-btns { display: flex; gap: 10px; }
    .row-btns button { flex: 1; }
    .transfer-card { padding: 20px; }
    .send-btn { width: 100%; margin-top: 10px; padding: 25px !important; }
  `]
})
export class PixPageComponent {
  constructor(private location: Location) {}
  goBack() { this.location.back(); }
}
