import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type Theme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private theme$ = new BehaviorSubject<Theme>(this.getStoredTheme());
  readonly currentTheme = this.theme$.asObservable();

  constructor() {
    this.applyTheme(this.theme$.value);
  }

  toggle(): void {
    const next = this.theme$.value === 'light' ? 'dark' : 'light';
    this.setTheme(next);
  }

  setTheme(theme: Theme): void {
    this.theme$.next(theme);
    this.applyTheme(theme);
    localStorage.setItem('krt-theme', theme);
  }

  isDark(): boolean {
    return this.theme$.value === 'dark';
  }

  private getStoredTheme(): Theme {
    const stored = localStorage.getItem('krt-theme');
    if (stored === 'dark' || stored === 'light') return stored;
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  private applyTheme(theme: Theme): void {
    document.documentElement.setAttribute('data-theme', theme);
    document.body.classList.remove('theme-light', 'theme-dark');
    document.body.classList.add(`theme-${theme}`);
  }
}