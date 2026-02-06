import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { PaymentService } from '../../../../core/services/payment.service';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AccountService } from '../../../../core/services/account.service';

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
             <button mat-stroked-button><mat-icon>content_copy</mat-icon> Copia e Cola</button>
             <button mat-stroked-button (click)="goToKeys()"><mat-icon>vpn_key</mat-icon> Minhas Chaves</button>
           </div>
        </div>

        <div class="favorites-section">
            <h3>Favoritos Recentes</h3>
            <div class="favorites-scroll">
                <div class="fav-item" (click)="selectContact(null)">
                    <div class="fav-avatar add"><mat-icon>add</mat-icon></div>
                    <span>Novo</span>
                </div>
                <div class="fav-item" *ngFor="let c of contacts" (click)="selectContact(c)">
                    <div class="fav-avatar">{{ c.initials }}</div>
                    <span>{{ c.name }}</span>
                </div>
            </div>
        </div>

        <h3>Transferir</h3>
        <mat-card class="transfer-card">
          <mat-form-field appearance="outline">
            <mat-label>ID da Conta Destino</mat-label>
            <input matInput placeholder="GUID da conta destino" [(ngModel)]="destinationAccountId">
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Chave Pix</mat-label>
            <input matInput placeholder="CPF, Email ou Aleatória" [(ngModel)]="pixKey">
            <mat-icon matSuffix>search</mat-icon>
          </mat-form-field>
          
          <mat-form-field appearance="outline">
            <mat-label>Valor (R$)</mat-label>
            <input matInput type="number" placeholder="0.00" [(ngModel)]="amount">
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Descrição (opcional)</mat-label>
            <input matInput placeholder="Ex: Almoço" [(ngModel)]="description">
          </mat-form-field>

          <button mat-raised-button color="primary" class="send-btn" 
                  (click)="sendPix()" [disabled]="loading">
            <span *ngIf="!loading">TRANSFERIR AGORA</span>
            <mat-spinner diameter="24" *ngIf="loading"></mat-spinner>
          </button>
        </mat-card>
      </main>
    </div>
  `,
  styles: [`
    .header-simple { background: #f5f7fa; padding: 15px; display: flex; justify-content: space-between; align-items: center; }
    .pix-menu { display: flex; flex-direction: column; gap: 10px; margin: 10px 0 20px; }
    .big-btn { padding: 30px !important; font-size: 1.1rem; }
    .row-btns { display: flex; gap: 10px; }
    .row-btns button { flex: 1; }
    .favorites-section { margin-bottom: 20px; }
    .favorites-scroll { display: flex; gap: 15px; overflow-x: auto; padding-bottom: 10px; scrollbar-width: none; }
    .fav-item { display: flex; flex-direction: column; align-items: center; cursor: pointer; min-width: 60px; }
    .fav-avatar { 
        width: 56px; height: 56px; background: #e0e0e0; border-radius: 50%; 
        display: flex; justify-content: center; align-items: center; font-weight: 600; color: #555; margin-bottom: 5px;
    }
    .fav-avatar.add { background: #e3f2fd; color: var(--primary); }
    .fav-item span { font-size: 0.75rem; text-align: center; max-width: 60px; overflow: hidden; text-overflow: ellipsis; }
    .transfer-card { padding: 20px; }
    .transfer-card mat-form-field { width: 100%; }
    .send-btn { width: 100%; margin-top: 10px; padding: 25px !important; font-weight: bold; }
    ::ng-deep .mat-mdc-progress-spinner circle { stroke: white !important; }
  `]
})
export class PixPageComponent implements OnInit {
  destinationAccountId = '';
  pixKey = '';
  amount: number | null = null;
  description = '';
  loading = false;
  
  contacts = [
      { name: 'Ana Silva', initials: 'AS', key: '11999998888' },
      { name: 'Bruno K.', initials: 'BK', key: 'bruno@email.com' },
      { name: 'Loja Tech', initials: 'LT', key: '44.222.111/0001-90' },
      { name: 'Mãe', initials: 'M', key: '11988887777' }
  ];

  constructor(
    private location: Location,
    private paymentService: PaymentService,
    private authService: AuthService,
    private notify: NotificationService,
    private router: Router
  ) {}

  ngOnInit() {}

  selectContact(contact: any) {
      if (contact) {
          this.pixKey = contact.key;
          this.notify.success('Chave de ' + contact.name + ' selecionada.');
      } else {
          this.pixKey = '';
          this.amount = null;
          this.destinationAccountId = '';
      }
  }

  sendPix() {
    if (!this.destinationAccountId || !this.pixKey || !this.amount || this.amount <= 0) {
        this.notify.error('Preencha todos os campos corretamente.');
        return;
    }

    const sourceId = this.authService.accountId;
    if (!sourceId) { this.router.navigate(['/login']); return; }

    this.loading = true;

    this.paymentService.sendPix({
      sourceAccountId: sourceId,
      destinationAccountId: this.destinationAccountId,
      pixKey: this.pixKey,
      amount: this.amount,
      description: this.description || undefined,
      idempotencyKey: crypto.randomUUID()
    }).subscribe({
      next: (res: any) => {
        this.loading = false;
        if (res.success) {
          this.router.navigate(['/success']);
        } else {
          this.notify.error(res.errors?.join('. ') || 'Falha na transferência.');
        }
      },
      error: (err: any) => {
        this.loading = false;
        const errors = err.error?.errors;
        const msg = Array.isArray(errors) ? errors.join('. ') : 'Falha ao realizar Pix.';
        this.notify.error(msg);
      }
    });
  }

  goBack() { this.location.back(); }
  goToKeys() { this.router.navigate(['/pix/keys']); }
}
