import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-recharge',
  template: `
    <div class="app-layout">
      <header class="header-simple">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Recarga</h1>
        <div style="width: 40px"></div>
      </header>

      <main class="container fade-in">
        <mat-card class="input-card">
            <h3>Qual número deseja recarregar?</h3>
            <mat-form-field appearance="outline" class="full">
                <mat-label>Número com DDD</mat-label>
                <input matInput placeholder="(11) 99999-9999" [(ngModel)]="phone">
                <mat-icon matSuffix>smartphone</mat-icon>
            </mat-form-field>
        </mat-card>

        <h3>Valor da recarga</h3>
        <div class="values-grid">
            <div class="value-option" *ngFor="let v of values" 
                 [class.selected]="selectedValue === v" (click)="selectValue(v)">
                <span class="v-label">{{ v | currency:'BRL' }}</span>
                <span class="v-bonus" *ngIf="v >= 30">+1GB Bônus</span>
            </div>
        </div>

        <button mat-raised-button color="primary" class="confirm-btn" 
                [disabled]="!phone || !selectedValue" (click)="confirm()">
            CONFIRMAR RECARGA
        </button>
      </main>
    </div>
  `,
  styles: [`
    .input-card { padding: 20px; margin-bottom: 25px; }
    .full { width: 100%; }
    
    .values-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 30px; }
    .value-option { 
        background: white; border: 2px solid #eee; border-radius: 12px; 
        padding: 20px; text-align: center; cursor: pointer; transition: all 0.2s;
        display: flex; flex-direction: column; align-items: center;
    }
    .value-option.selected { border-color: var(--primary); background: #eef2ff; }
    .v-label { font-weight: 700; font-size: 1.1rem; color: var(--text-main); }
    .v-bonus { font-size: 0.7rem; color: var(--accent); font-weight: 600; margin-top: 5px; }
    
    .confirm-btn { width: 100%; padding: 25px !important; font-size: 1rem; }
  `]
})
export class RechargeComponent {
  phone = '';
  values = [15, 20, 30, 50];
  selectedValue: number | null = null;

  constructor(private location: Location, private notify: NotificationService, private router: Router) {}

  selectValue(val: number) { this.selectedValue = val; }
  
  confirm() {
      this.notify.success(`Recarga de R$ ${this.selectedValue},00 realizada!`);
      this.router.navigate(['/success']);
  }
  
  goBack() { this.location.back(); }
}
