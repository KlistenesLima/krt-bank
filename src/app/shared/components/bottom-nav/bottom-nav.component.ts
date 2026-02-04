import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-bottom-nav',
  template: `
    <nav class="bottom-nav">
      <div class="nav-item" (click)="nav('/dashboard')" [class.active]="isActive('/dashboard')">
         <mat-icon>home</mat-icon>
         <span>Início</span>
      </div>
      <div class="nav-item" (click)="nav('/extract')" [class.active]="isActive('/extract')">
         <mat-icon>receipt_long</mat-icon>
         <span>Extrato</span>
      </div>
      <div class="nav-item" (click)="nav('/pix')" [class.active]="isActive('/pix')">
         <div class="pix-highlight"><mat-icon>pix</mat-icon></div>
         <span>Pix</span>
      </div>
      <div class="nav-item" (click)="nav('/cards')" [class.active]="isActive('/cards')">
         <mat-icon>credit_card</mat-icon>
         <span>Cartões</span>
      </div>
      <div class="nav-item" (click)="nav('/profile')" [class.active]="isActive('/profile')">
         <mat-icon>person</mat-icon>
         <span>Perfil</span>
      </div>
    </nav>
  `,
  styles: [`
    .bottom-nav {
        position: fixed; bottom: 0; left: 0; width: 100%; height: 60px;
        background: white; border-top: 1px solid #eee; display: flex; justify-content: space-around;
        align-items: center; z-index: 1000; padding-bottom: env(safe-area-inset-bottom);
        box-shadow: 0 -2px 10px rgba(0,0,0,0.05);
    }
    .nav-item {
        display: flex; flex-direction: column; align-items: center; justify-content: center;
        color: var(--text-secondary); cursor: pointer; flex: 1; height: 100%;
    }
    .nav-item mat-icon { font-size: 24px; margin-bottom: 2px; }
    .nav-item span { font-size: 0.7rem; font-weight: 500; }
    
    .nav-item.active { color: var(--primary); }
    
    .pix-highlight {
        background: #f0f2f5; color: var(--text-main); border-radius: 8px;
        width: 40px; height: 24px; display: flex; align-items: center; justify-content: center;
        margin-bottom: 4px;
    }
    .nav-item.active .pix-highlight { background: var(--primary); color: white; }
  `]
})
export class BottomNavComponent {
  constructor(private router: Router) {}
  nav(route: string) { this.router.navigate([route]); }
  isActive(route: string) { return this.router.url.includes(route); }
}
