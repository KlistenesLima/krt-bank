import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface BannerLink {
  id: string;
  href: string;
  label: string;
  external: boolean;
}

@Component({
  selector: 'app-demo-banner',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="portfolio-banner" [class.visible]="visible">
      <div class="banner-inner">
        <div class="banner-text">
          <svg class="banner-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16">
            <rect x="4" y="2" width="16" height="20" rx="2" ry="2"/>
            <path d="M9 22V6h6v16"/>
            <path d="M4 6h16"/>
          </svg>
          <span class="banner-prefix-full">Case de Portfólio — </span>
          <span class="banner-prefix-short">Portfólio — </span>
          <span class="banner-name">Klístenes Lima</span>
        </div>
        <div class="banner-links">
          <ng-container *ngFor="let link of links">
            <a *ngIf="link.external"
               [href]="link.href"
               target="_blank"
               rel="noopener noreferrer"
               [title]="link.label"
               class="banner-link">
              <ng-container [ngSwitch]="link.id">
                <svg *ngSwitchCase="'linkedin'" viewBox="0 0 24 24" fill="currentColor" width="16" height="16">
                  <path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433a2.062 2.062 0 01-2.063-2.065 2.064 2.064 0 112.063 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/>
                </svg>
                <svg *ngSwitchCase="'github'" viewBox="0 0 24 24" fill="currentColor" width="16" height="16">
                  <path d="M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61C4.422 18.07 3.633 17.7 3.633 17.7c-1.087-.744.084-.729.084-.729 1.205.084 1.838 1.236 1.838 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.22 0 1.606-.015 2.896-.015 3.286 0 .315.21.69.825.57C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12"/>
                </svg>
              </ng-container>
            </a>
            <a *ngIf="!link.external"
               [routerLink]="link.href"
               [title]="link.label"
               class="banner-link">
              <ng-container [ngSwitch]="link.id">
                <svg *ngSwitchCase="'about'" viewBox="0 0 24 24" fill="currentColor" width="16" height="16">
                  <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                </svg>
                <svg *ngSwitchCase="'docs'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16">
                  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                  <polyline points="14 2 14 8 20 8"/>
                  <line x1="16" y1="13" x2="8" y2="13"/>
                  <line x1="16" y1="17" x2="8" y2="17"/>
                </svg>
                <svg *ngSwitchCase="'portfolio'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16">
                  <circle cx="12" cy="12" r="10"/>
                  <line x1="12" y1="16" x2="12" y2="12"/>
                  <line x1="12" y1="8" x2="12.01" y2="8"/>
                </svg>
              </ng-container>
            </a>
          </ng-container>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .portfolio-banner {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 9999;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
      border-bottom: 1px solid rgba(201, 169, 98, 0.3);
      opacity: 0;
      transition: opacity 0.5s ease;
    }
    .portfolio-banner.visible {
      opacity: 1;
    }
    .banner-inner {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 12px;
      padding: 8px 16px;
      font-family: 'Plus Jakarta Sans', 'Poppins', sans-serif;
      font-size: 13px;
      color: #e0e0e0;
      flex-wrap: nowrap;
    }
    .banner-text {
      display: flex;
      align-items: center;
      gap: 8px;
      white-space: nowrap;
    }
    .banner-icon {
      color: #c9a962;
    }
    .banner-prefix-full {
      display: inline;
    }
    .banner-prefix-short {
      display: none;
    }
    .banner-name {
      color: #c9a962;
      font-weight: 600;
    }
    .banner-links {
      display: flex;
      align-items: center;
      gap: 6px;
    }
    .banner-link {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      border-radius: 8px;
      background: rgba(255, 255, 255, 0.08);
      color: #a0a0b0;
      text-decoration: none;
      transition: all 0.2s;
      min-height: auto;
      min-width: auto;
    }
    .banner-link:hover {
      background: rgba(201, 169, 98, 0.2);
      color: #c9a962;
    }
    .banner-link svg {
      width: 16px;
      height: 16px;
    }

    @media (max-width: 600px) {
      .banner-inner {
        justify-content: space-between;
        padding: 6px 10px;
        gap: 6px;
        font-size: 12px;
      }
      .banner-text {
        font-size: 11px;
      }
      .banner-prefix-full {
        display: none;
      }
      .banner-prefix-short {
        display: inline;
      }
      .banner-links {
        gap: 4px;
      }
      .banner-link {
        width: 26px;
        height: 26px;
        border-radius: 6px;
      }
      .banner-link svg {
        width: 13px;
        height: 13px;
      }
    }

    @media (max-width: 380px) {
      .banner-inner {
        padding: 4px 6px;
        gap: 4px;
      }
      .banner-text {
        font-size: 10px;
      }
      .banner-links {
        gap: 3px;
      }
      .banner-link {
        width: 24px;
        height: 24px;
      }
      .banner-link svg {
        width: 12px;
        height: 12px;
      }
    }
  `]
})
export class DemoBannerComponent implements OnInit {
  visible = false;

  links: BannerLink[] = [
    { id: 'linkedin', href: 'https://linkedin.com/in/klisteneslima', label: 'LinkedIn', external: true },
    { id: 'about', href: '/about', label: 'Sobre', external: false },
    { id: 'github', href: 'https://github.com/KlistenesLima', label: 'GitHub', external: true },
    { id: 'docs', href: '/docs', label: 'Documentação', external: false },
    { id: 'portfolio', href: '/portfolio', label: 'Portfólio', external: false }
  ];

  ngOnInit() {
    setTimeout(() => this.visible = true, 100);
  }
}
