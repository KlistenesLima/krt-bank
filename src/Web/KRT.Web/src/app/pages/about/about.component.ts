import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="about-page" [class.visible]="visible">
      <!-- Hero -->
      <section class="hero">
        <div class="hero-bg"></div>
        <div class="hero-content">
          <div class="badge">Arquitetura do Sistema</div>
          <h1>KRT <span class="accent">Bank</span></h1>
          <p class="subtitle">Plataforma bancária digital com microsserviços .NET 8</p>
        </div>
      </section>

      <!-- O que é -->
      <section class="section">
        <h2>O que é este <span class="accent">projeto</span></h2>
        <div class="cards-grid">
          <div class="card" *ngFor="let card of whatCards">
            <h3>{{ card.title }}</h3>
            <p>{{ card.text }}</p>
          </div>
        </div>
      </section>

      <!-- Arquitetura -->
      <section class="section">
        <h2>Arquitetura do <span class="accent">Sistema</span></h2>
        <div class="arch-container">
          <svg viewBox="0 0 700 220" class="arch-svg">
            <!-- Connections -->
            <line x1="140" y1="30" x2="210" y2="30" stroke="#0047BB" stroke-width="2" stroke-dasharray="6,3" opacity="0.6"/>
            <line x1="330" y1="20" x2="400" y2="15" stroke="#0047BB" stroke-width="2" stroke-dasharray="6,3" opacity="0.6"/>
            <line x1="330" y1="40" x2="400" y2="70" stroke="#0047BB" stroke-width="2" stroke-dasharray="6,3" opacity="0.6"/>
            <line x1="270" y1="50" x2="270" y2="130" stroke="#0047BB" stroke-width="1.5" stroke-dasharray="4,4" opacity="0.4"/>
            <line x1="270" y1="170" x2="270" y2="190" stroke="#4D8B31" stroke-width="1.5" stroke-dasharray="4,4" opacity="0.4"/>

            <!-- Arrows -->
            <polygon points="208,27 200,23 200,31" fill="#0047BB" opacity="0.6"/>

            <!-- Angular App -->
            <rect x="5" y="10" width="130" height="40" rx="8" fill="rgba(221,51,51,0.12)" stroke="#DD3333" stroke-width="1.5"/>
            <text x="70" y="27" text-anchor="middle" fill="#fff" font-size="11" font-weight="600">Angular App</text>
            <text x="70" y="42" text-anchor="middle" fill="#DD3333" font-size="9">Frontend</text>

            <!-- Gateway -->
            <rect x="210" y="10" width="120" height="40" rx="8" fill="rgba(0,71,187,0.12)" stroke="#0047BB" stroke-width="1.5"/>
            <text x="270" y="27" text-anchor="middle" fill="#fff" font-size="11" font-weight="600">Gateway YARP</text>
            <text x="270" y="42" text-anchor="middle" fill="#0047BB" font-size="9">Reverse Proxy</text>

            <!-- Onboarding -->
            <rect x="400" y="0" width="140" height="35" rx="8" fill="rgba(81,43,212,0.12)" stroke="#512BD4" stroke-width="1.5"/>
            <text x="470" y="15" text-anchor="middle" fill="#fff" font-size="10" font-weight="600">Onboarding API</text>
            <text x="470" y="28" text-anchor="middle" fill="#512BD4" font-size="9">PostgreSQL</text>

            <!-- Payments -->
            <rect x="400" y="55" width="140" height="35" rx="8" fill="rgba(0,71,187,0.12)" stroke="#0047BB" stroke-width="1.5"/>
            <text x="470" y="70" text-anchor="middle" fill="#fff" font-size="10" font-weight="600">Payments API</text>
            <text x="470" y="83" text-anchor="middle" fill="#0047BB" font-size="9">PostgreSQL</text>

            <!-- Infra -->
            <rect x="150" y="130" width="250" height="32" rx="8" fill="rgba(255,102,0,0.12)" stroke="#FF6600" stroke-width="1.5"/>
            <text x="275" y="151" text-anchor="middle" fill="#fff" font-size="10" font-weight="600">Kafka  |  RabbitMQ  |  Redis</text>

            <!-- Auth -->
            <rect x="150" y="175" width="250" height="32" rx="8" fill="rgba(77,139,49,0.12)" stroke="#4D8B31" stroke-width="1.5"/>
            <text x="275" y="196" text-anchor="middle" fill="#fff" font-size="10" font-weight="600">Keycloak  |  Seq</text>
          </svg>
        </div>
      </section>

      <!-- Stack Técnico -->
      <section class="section">
        <h2>Stack <span class="accent">Técnico</span></h2>
        <div class="tech-grid">
          <div class="tech-card" *ngFor="let tech of techStack">
            <div class="tech-icon" [style.background]="tech.color + '25'" [style.border-color]="tech.color + '50'">
              {{ tech.name.charAt(0) }}
            </div>
            <h4>{{ tech.name }}</h4>
            <p>{{ tech.desc }}</p>
          </div>
        </div>
      </section>

      <!-- Números -->
      <section class="section">
        <h2>Em <span class="accent">Números</span></h2>
        <div class="metrics-grid">
          <div class="metric-card" *ngFor="let m of metrics">
            <div class="metric-value">{{ m.value }}</div>
            <div class="metric-label">{{ m.label }}</div>
          </div>
        </div>
      </section>

      <!-- Links -->
      <section class="section links-section">
        <a href="https://store.klisteneslima.dev" target="_blank" rel="noopener noreferrer" class="btn btn-gold">Ver AUREA Maison</a>
        <a href="https://github.com/KlistenesLima/krt-bank" target="_blank" rel="noopener noreferrer" class="btn btn-outline">GitHub</a>
        <a routerLink="/resume" class="btn btn-primary">Currículo</a>
      </section>
    </div>
  `,
  styles: [`
    .about-page { opacity: 0; transition: opacity 0.7s ease; }
    .about-page.visible { opacity: 1; }

    .hero { position: relative; padding: 80px 16px 60px; text-align: center; overflow: hidden; }
    .hero-bg { position: absolute; inset: 0; background: linear-gradient(180deg, rgba(0,47,98,0.3) 0%, transparent 100%); pointer-events: none; }
    .hero-content { position: relative; max-width: 800px; margin: 0 auto; }
    .badge { display: inline-block; padding: 6px 16px; border-radius: 20px; background: rgba(0,71,187,0.15); border: 1px solid rgba(0,71,187,0.3); color: #4d9fff; font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 2px; margin-bottom: 20px; }
    h1 { font-size: 48px; font-weight: 800; color: #fff; margin: 0 0 12px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .accent { color: #4d9fff; }
    .subtitle { color: #94a3b8; font-size: 18px; margin: 0; }

    .section { max-width: 1000px; margin: 0 auto; padding: 60px 16px; }
    h2 { font-size: 28px; font-weight: 700; color: #fff; text-align: center; margin: 0 0 32px; font-family: 'Plus Jakarta Sans', sans-serif; }

    .cards-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
    .card { padding: 24px; border-radius: 16px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); backdrop-filter: blur(8px); transition: border-color 0.3s; }
    .card:hover { border-color: rgba(0,71,187,0.4); }
    .card h3 { color: #4d9fff; font-size: 16px; font-weight: 600; margin: 0 0 8px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .card p { color: #94a3b8; font-size: 14px; margin: 0; line-height: 1.6; }

    .arch-container { overflow-x: auto; padding-bottom: 16px; }
    .arch-svg { width: 100%; min-width: 700px; background: rgba(26,35,50,0.4); border: 1px solid rgba(0,71,187,0.15); border-radius: 16px; padding: 16px; font-family: 'Plus Jakarta Sans', sans-serif; }

    .tech-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: 16px; }
    .tech-card { padding: 20px; border-radius: 12px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); text-align: center; transition: all 0.3s; }
    .tech-card:hover { border-color: rgba(0,71,187,0.4); transform: translateY(-2px); }
    .tech-icon { width: 40px; height: 40px; margin: 0 auto 12px; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 14px; color: #fff; border: 1px solid; }
    .tech-card h4 { color: #fff; font-size: 13px; font-weight: 600; margin: 0 0 4px; }
    .tech-card p { color: #94a3b8; font-size: 12px; margin: 0; }

    .metrics-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
    .metric-card { padding: 24px; border-radius: 12px; background: linear-gradient(135deg, rgba(26,35,50,0.8), rgba(26,35,50,0.4)); border: 1px solid rgba(0,71,187,0.15); text-align: center; transition: border-color 0.3s; }
    .metric-card:hover { border-color: rgba(0,71,187,0.4); }
    .metric-value { color: #4d9fff; font-size: 36px; font-weight: 800; margin-bottom: 4px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .metric-label { color: #94a3b8; font-size: 13px; }

    .links-section { text-align: center; display: flex; flex-wrap: wrap; justify-content: center; gap: 12px; padding-bottom: 80px; }
    .btn { display: inline-block; padding: 12px 32px; border-radius: 12px; font-size: 14px; font-weight: 600; text-decoration: none; transition: all 0.3s; font-family: 'Plus Jakarta Sans', sans-serif; }
    .btn-primary { background: #0047BB; color: #fff; }
    .btn-primary:hover { background: #0055DD; }
    .btn-outline { border: 1px solid rgba(0,71,187,0.4); color: #4d9fff; }
    .btn-outline:hover { background: rgba(0,71,187,0.1); }
    .btn-gold { background: #c9a962; color: #0a1628; }
    .btn-gold:hover { background: #d4b87a; }

    @media (max-width: 768px) {
      h1 { font-size: 32px; }
      .cards-grid { grid-template-columns: 1fr; }
      .metrics-grid { grid-template-columns: repeat(2, 1fr); }
      .hero { padding: 60px 16px 40px; }
    }
  `]
})
export class AboutComponent implements OnInit {
  visible = false;

  whatCards = [
    { title: 'Plataforma Bancária Completa', text: 'Sistema bancário digital com abertura de contas, dashboard, extrato e múltiplas formas de pagamento.' },
    { title: 'PIX (QR Code EMV)', text: 'Implementação completa de PIX com QR Code padrão EMV do Banco Central, chaves PIX e transferências P2P.' },
    { title: 'Boleto e Cartão Virtual', text: 'Pagamento de boletos com leitura de linha digitável e cartão de crédito virtual com limite gerenciado.' },
    { title: 'Integração E-commerce', text: 'Processa pagamentos do AUREA Maison (KLL Platform) como gateway de pagamentos via API Key.' },
  ];

  techStack = [
    { name: '.NET 8', desc: 'Backend', color: '#512BD4' },
    { name: 'Angular 17', desc: 'Frontend', color: '#DD0031' },
    { name: 'TypeScript', desc: 'Tipagem', color: '#3178C6' },
    { name: 'PostgreSQL', desc: 'Database', color: '#4169E1' },
    { name: 'Redis', desc: 'Cache', color: '#DC382D' },
    { name: 'Kafka', desc: 'Events', color: '#231F20' },
    { name: 'RabbitMQ', desc: 'Mensageria', color: '#FF6600' },
    { name: 'Docker', desc: 'Containers', color: '#2496ED' },
    { name: 'Keycloak', desc: 'Auth', color: '#4D8B31' },
    { name: 'YARP', desc: 'Gateway', color: '#0047BB' },
  ];

  metrics = [
    { value: '10', label: 'Microsserviços' },
    { value: '145', label: 'Testes' },
    { value: '3', label: 'APIs REST' },
    { value: '3', label: 'PIX / Boleto / Cartão' },
  ];

  ngOnInit() {
    window.scrollTo(0, 0);
    setTimeout(() => this.visible = true, 100);
  }
}
