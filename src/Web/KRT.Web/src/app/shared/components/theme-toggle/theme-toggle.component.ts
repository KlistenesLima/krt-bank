import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService, Theme } from '../../../core/services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button class="theme-toggle" (click)="toggle()" [title]="isDark ? 'Modo Claro' : 'Modo Escuro'">
      <span class="icon">{{ isDark ? '☀️' : '🌙' }}</span>
    </button>
  `,
  styles: [`
    .theme-toggle {
      background: var(--bg-toggle, rgba(0,0,0,0.05));
      border: 1.5px solid var(--border, #e0e0e0);
      border-radius: 50%;
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.3s;
      font-size: 18px;
    }
    .theme-toggle:hover {
      background: var(--bg-toggle-hover, rgba(0,0,0,0.1));
      transform: scale(1.1);
    }
  `]
})
export class ThemeToggleComponent implements OnInit {
  isDark = false;

  constructor(private themeService: ThemeService) {}

  ngOnInit(): void {
    this.themeService.currentTheme.subscribe(t => this.isDark = t === 'dark');
  }

  toggle(): void {
    this.themeService.toggle();
  }
}