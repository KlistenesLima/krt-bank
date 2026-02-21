import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <footer class="footer">
      <div class="footer-inner">
        <!-- Brand -->
        <div class="footer-brand">
          <div class="brand-name">
            <span class="brand-krt">KRT</span>
            <span class="brand-bank">Bank</span>
          </div>
          <p class="brand-desc">Plataforma bancária digital — Case de portfólio</p>
          <div class="brand-line"></div>
        </div>

        <!-- Columns -->
        <div class="footer-grid">
          <!-- Navegação -->
          <div class="footer-col">
            <h4>Navegação</h4>
            <ul>
              <li><a routerLink="/dashboard">Dashboard</a></li>
              <li><a routerLink="/pix">PIX</a></li>
              <li><a routerLink="/boleto">Boleto</a></li>
              <li><a routerLink="/cards">Cartões</a></li>
              <li><a routerLink="/extract">Extrato</a></li>
            </ul>
          </div>

          <!-- Portfólio -->
          <div class="footer-col">
            <h4>Portfólio</h4>
            <ul>
              <li><a routerLink="/about">Sobre o Projeto</a></li>
              <li><a routerLink="/docs">Documentação</a></li>
              <li><a routerLink="/resume">Currículo</a></li>
              <li><a href="https://store.klisteneslima.dev" target="_blank" rel="noopener noreferrer">AUREA Maison</a></li>
            </ul>
          </div>

          <!-- Contato -->
          <div class="footer-col">
            <h4>Contato</h4>
            <ul>
              <li><a href="mailto:klisteneswar2@hotmail.com">klisteneswar2&#64;hotmail.com</a></li>
              <li><a href="https://www.linkedin.com/in/klistenes-de-lima-leite-257209194/" target="_blank" rel="noopener noreferrer">LinkedIn</a></li>
              <li><a href="https://github.com/KlistenesLima" target="_blank" rel="noopener noreferrer">GitHub</a></li>
            </ul>
          </div>
        </div>

        <!-- Copyright -->
        <div class="footer-bottom">
          <p>&copy; 2026 Klístenes Lima. Projeto de portfólio — nenhuma transação é real.</p>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      background: rgba(10, 22, 40, 0.9);
      border-top: 1px solid rgba(0, 71, 187, 0.15);
    }
    .footer-inner {
      max-width: 1200px;
      margin: 0 auto;
      padding: 48px 16px 24px;
    }
    .footer-brand {
      text-align: center;
      margin-bottom: 40px;
    }
    .brand-name {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
      margin-bottom: 8px;
    }
    .brand-krt {
      font-size: 22px;
      font-weight: 800;
      color: #4d9fff;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .brand-bank {
      font-size: 18px;
      font-weight: 300;
      color: #94a3b8;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .brand-desc {
      color: rgba(148, 163, 184, 0.6);
      font-size: 13px;
      margin: 0;
    }
    .brand-line {
      width: 48px;
      height: 1px;
      background: rgba(0, 71, 187, 0.3);
      margin: 16px auto 0;
    }
    .footer-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 32px;
      margin-bottom: 40px;
    }
    .footer-col h4 {
      color: #fff;
      font-size: 11px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 2px;
      margin: 0 0 16px;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .footer-col ul {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-direction: column;
      gap: 10px;
    }
    .footer-col a {
      color: rgba(148, 163, 184, 0.6);
      text-decoration: none;
      font-size: 13px;
      transition: color 0.2s;
    }
    .footer-col a:hover {
      color: #4d9fff;
    }
    .footer-bottom {
      border-top: 1px solid rgba(0, 71, 187, 0.1);
      padding-top: 24px;
      text-align: center;
    }
    .footer-bottom p {
      color: rgba(148, 163, 184, 0.4);
      font-size: 12px;
      margin: 0;
    }

    @media (max-width: 768px) {
      .footer-grid {
        grid-template-columns: 1fr;
        gap: 24px;
      }
    }
  `]
})
export class FooterComponent {}
