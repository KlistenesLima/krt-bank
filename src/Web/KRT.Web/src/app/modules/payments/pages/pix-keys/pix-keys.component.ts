import { Component } from '@angular/core';
import { Location } from '@angular/common';

@Component({
  selector: 'app-pix-keys',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Minhas Chaves</h1>
        <button mat-icon-button><mat-icon>help_outline</mat-icon></button>
      </header>

      <main class="container fade-in">
        <div class="info-box">
            <mat-icon>info</mat-icon>
            <p>Gerencie suas chaves para receber Pix.</p>
        </div>

        <h3>Chaves cadastradas</h3>
        <mat-card class="keys-list">
            <mat-list>
                <mat-list-item>
                    <mat-icon matListItemIcon>badge</mat-icon>
                    <div matListItemTitle>CPF</div>
                    <div matListItemLine>***.111.***-**</div>
                    <button mat-icon-button matListItemMeta><mat-icon>more_vert</mat-icon></button>
                </mat-list-item>
                <mat-divider></mat-divider>
                <mat-list-item>
                    <mat-icon matListItemIcon>shuffle</mat-icon>
                    <div matListItemTitle>Aleatória</div>
                    <div matListItemLine>7a8b-9c0d-e1f2-3456</div>
                    <button mat-icon-button matListItemMeta><mat-icon>content_copy</mat-icon></button>
                </mat-list-item>
            </mat-list>
        </mat-card>

        <button mat-stroked-button color="primary" class="add-btn">
            <mat-icon>add</mat-icon> NOVA CHAVE
        </button>
      </main>
    </div>
  `,
  styles: [`
    .info-box { background: #e3f2fd; color: #0d47a1; padding: 15px; border-radius: 8px; display: flex; gap: 10px; margin-bottom: 20px; font-size: 0.9rem; }
    .keys-list { padding: 0; }
    .add-btn { width: 100%; margin-top: 20px; padding: 20px !important; }
  `]
})
export class PixKeysComponent {
  constructor(private location: Location) {}
  goBack() { this.location.back(); }
}
