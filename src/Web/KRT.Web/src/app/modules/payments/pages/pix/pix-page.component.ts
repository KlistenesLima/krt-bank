import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { AccountService } from '../../../../core/services/account.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Router } from '@angular/router';

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
             <button mat-stroked-button (click)="goToKeys()"><mat-icon>vpn_key</mat-icon> Minhas Chaves</button>
           </div>
        </div>

        <h3>Transferir</h3>
        <mat-card class="transfer-card">
          <mat-form-field appearance="outline">
            <mat-label>Chave Pix</mat-label>
            <input matInput placeholder="CPF, Email ou Aleatória" [(ngModel)]="pixKey">
            <mat-icon matSuffix>search</mat-icon>
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Valor (R$)</mat-label>
            <input matInput type="number" placeholder="0.00" [(ngModel)]="amount">
          </mat-form-field>

          <button mat-raised-button color="primary" class="send-btn" 
                  (click)="sendPix()" [disabled]="loading">
            <span *ngIf="!loading">TRANSFERIR AGORA</span>
            <span *ngIf="loading">PROCESSANDO...</span>
          </button>
        </mat-card>
      </main>
    </div>
  `,
  styles: [`
    .header-simple { background: #f5f7fa; padding: 15px; display: flex; justify-content: space-between; align-items: center; }
    .pix-menu { display: flex; flex-direction: column; gap: 10px; margin: 10px 0 30px; }
    .big-btn { padding: 30px !important; font-size: 1.1rem; }
    .row-btns { display: flex; gap: 10px; }
    .row-btns button { flex: 1; }
    .transfer-card { padding: 20px; }
    .send-btn { width: 100%; margin-top: 10px; padding: 25px !important; font-weight: bold; }
  `]
})
export class PixPageComponent {
  pixKey = '';
  amount: number | null = null;
  loading = false;

  constructor(
    private location: Location, 
    private accService: AccountService,
    private notify: NotificationService,
    private router: Router
  ) {}

  sendPix() {
    if (!this.pixKey || !this.amount || this.amount <= 0) {
        this.notify.error('Preencha os dados corretamente.');
        return;
    }

    const id = localStorage.getItem('krt_account_id');
    if (!id) return;

    this.loading = true;
    this.accService.performPix(id, this.pixKey, this.amount).subscribe({
        next: (res: any) => { // TIPO ANY ADICIONADO
            this.loading = false;
            this.router.navigate(['/success']);
        },
        error: (err: any) => { // TIPO ANY ADICIONADO
            this.loading = false;
            const msg = err.error?.errors?.Balance?.[0] || 'Falha ao realizar Pix.';
            this.notify.error(msg);
        }
    });
  }

  goBack() { this.location.back(); }
  goToKeys() { this.router.navigate(['/pix/keys']); }
}
