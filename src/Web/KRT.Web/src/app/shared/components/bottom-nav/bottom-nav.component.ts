import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-bottom-nav',
  template: `
    <nav class="bottom-nav">
      <button class="nav-item" [class.active]="isActive('/dashboard')" (click)="go('/dashboard')">
        <mat-icon>home</mat-icon>
        <span>Início</span>
      </button>
      <button class="nav-item" [class.active]="isActive('/extract')" (click)="go('/extract')">
        <mat-icon>swap_vert</mat-icon>
        <span>Extrato</span>
      </button>
      <button class="nav-pix" (click)="go('/pix')">
        <div class="pix-circle">
          <mat-icon>flash_on</mat-icon>
        </div>
      </button>
      <button class="nav-item" [class.active]="isActive('/cards')" (click)="go('/cards')">
        <mat-icon>credit_card</mat-icon>
        <span>Cartões</span>
      </button>
      <button class="nav-item" [class.active]="isActive('/profile')" (click)="go('/profile')">
        <mat-icon>person_outline</mat-icon>
        <span>Perfil</span>
      </button>
    </nav>
  `,
  styles: [`
    .bottom-nav {
      position: fixed; bottom: 0; left: 0; right: 0;
      background: white; height: 68px;
      display: flex; justify-content: space-around; align-items: center;
      border-top: 1px solid var(--krt-divider);
      box-shadow: 0 -4px 20px rgba(0,0,0,0.06);
      z-index: 1000;
      padding: 0 8px;
      max-width: 100%; /* Mobile-first */
    }
    .nav-item {
      background: none; border: none; cursor: pointer;
      display: flex; flex-direction: column; align-items: center; gap: 2px;
      color: var(--krt-text-muted); padding: 8px 12px;
      transition: color 0.2s;
      min-width: 56px;
    }
    .nav-item.active { color: var(--krt-primary); }
    .nav-item mat-icon { font-size: 24px; width: 24px; height: 24px; }
    .nav-item span { font-size: 0.65rem; font-weight: 600; }

    .nav-pix {
      background: none; border: none; cursor: pointer;
      margin-top: -24px; padding: 0;
    }
    .pix-circle {
      width: 56px; height: 56px; border-radius: 18px;
      background: var(--krt-gradient-accent);
      display: flex; align-items: center; justify-content: center;
      box-shadow: 0 6px 20px rgba(0,212,170,0.4);
      transition: transform 0.2s;
    }
    .pix-circle:hover { transform: scale(1.05); }
    .pix-circle mat-icon { color: white; font-size: 28px; width: 28px; height: 28px; }
  `]
})
export class BottomNavComponent {
  constructor(private router: Router) {}
  go(path: string) { this.router.navigate([path]); }
  isActive(path: string): boolean { return this.router.url === path; }
}
