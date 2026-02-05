import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-security',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Segurança</h1>
        <div style="width: 40px"></div>
      </header>

      <main class="container fade-in">
        
        <h3 class="section-title">Acesso</h3>
        <mat-card class="option-card">
            <div class="row-toggle">
                <div>
                    <div class="row-title">Biometria / FaceID</div>
                    <div class="row-desc">Usar para entrar no app</div>
                </div>
                <mat-slide-toggle color="primary" checked></mat-slide-toggle>
            </div>
        </mat-card>

        <h3 class="section-title">Senha do App</h3>
        <mat-card class="form-card">
            <mat-form-field appearance="outline" class="full">
                <mat-label>Senha Atual</mat-label>
                <input matInput type="password">
            </mat-form-field>
            
            <mat-form-field appearance="outline" class="full">
                <mat-label>Nova Senha</mat-label>
                <input matInput type="password">
            </mat-form-field>

            <mat-form-field appearance="outline" class="full">
                <mat-label>Confirmar Nova Senha</mat-label>
                <input matInput type="password">
            </mat-form-field>

            <button mat-stroked-button color="primary" class="full-btn" (click)="changePass()">
                ALTERAR SENHA
            </button>
        </mat-card>

      </main>
    </div>
  `,
  styles: [`
    .section-title { font-size: 0.9rem; color: #666; margin: 20px 0 10px 5px; text-transform: uppercase; font-weight: 600; }
    .option-card { padding: 20px; margin-bottom: 10px; }
    .row-toggle { display: flex; justify-content: space-between; align-items: center; }
    .row-title { font-weight: 600; font-size: 1rem; }
    .row-desc { font-size: 0.85rem; color: #888; }
    
    .form-card { padding: 20px; }
    .full { width: 100%; }
    .full-btn { width: 100%; height: 45px; margin-top: 10px; }
  `]
})
export class SecurityComponent {
  constructor(private location: Location, private notify: NotificationService) {}
  changePass() { this.notify.success('Senha alterada com sucesso.'); this.location.back(); }
  goBack() { this.location.back(); }
}
