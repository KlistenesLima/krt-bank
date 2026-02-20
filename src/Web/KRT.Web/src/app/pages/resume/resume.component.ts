import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface StackCategory {
  title: string;
  items: string[];
}

interface Project {
  title: string;
  stack: string;
  points: string[];
  url: string;
  expanded: boolean;
}

@Component({
  selector: 'app-resume',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="resume-page" [class.visible]="visible">
      <!-- Header -->
      <section class="hero">
        <div class="hero-bg"></div>
        <div class="hero-content">
          <h1>Klístenes de Lima <span class="accent">Leite</span></h1>
          <p class="title">Engenheiro de Software Sênior .NET</p>
          <div class="badges">
            <span class="badge">7 anos exp</span>
            <span class="badge">Cajazeiras, PB</span>
            <span class="badge">Disponível</span>
          </div>
          <div class="actions">
            <button class="btn btn-primary" (click)="downloadPdf()" [disabled]="generating">
              {{ generating ? 'Gerando PDF...' : 'Download PDF' }}
            </button>
            <a href="https://www.linkedin.com/in/klistenes-de-lima-leite-257209194/" target="_blank" rel="noopener noreferrer" class="btn btn-outline">LinkedIn</a>
            <a href="https://github.com/KlistenesLima" target="_blank" rel="noopener noreferrer" class="btn btn-ghost">GitHub</a>
          </div>
        </div>
      </section>

      <!-- Resumo -->
      <section class="section">
        <h2>Resumo <span class="accent">Profissional</span></h2>
        <div class="summary-card">
          <p>Engenheiro de Software Sênior com 7 anos de experiência em desenvolvimento de sistemas distribuídos e microsserviços com .NET.
          Especialista em Clean Architecture, DDD, CQRS e Event-Driven Architecture. Experiência comprovada em construção de plataformas
          bancárias digitais e e-commerce de alta performance.</p>
        </div>
      </section>

      <!-- Stack -->
      <section class="section">
        <h2>Stack <span class="accent">Técnico</span></h2>
        <div class="stack-grid">
          <div class="stack-card" *ngFor="let cat of stackCategories">
            <h3>{{ cat.title }}</h3>
            <div class="chips">
              <span class="chip" *ngFor="let item of cat.items">{{ item }}</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Projetos -->
      <section class="section">
        <h2>Projetos <span class="accent">Destaque</span></h2>
        <div class="projects">
          <div class="project-card" *ngFor="let project of projects; let i = index">
            <button class="project-header" (click)="project.expanded = !project.expanded">
              <div>
                <h3>{{ project.title }}</h3>
                <p class="project-stack">{{ project.stack }}</p>
              </div>
              <span class="arrow" [class.open]="project.expanded">&#9662;</span>
            </button>
            <div class="project-body" [class.expanded]="project.expanded">
              <ul>
                <li *ngFor="let point of project.points">{{ point }}</li>
              </ul>
              <a [href]="project.url" target="_blank" rel="noopener noreferrer" class="project-link">Ver projeto ao vivo</a>
            </div>
          </div>
        </div>
      </section>

      <!-- Formação e Idiomas -->
      <section class="section">
        <div class="two-cols">
          <div class="info-card">
            <h2>Formação</h2>
            <p>Bacharelado em Ciência da Computação</p>
          </div>
          <div class="info-card">
            <h2>Idiomas</h2>
            <div class="lang-row"><span>Português</span><span class="lang-level">Nativo</span></div>
            <div class="lang-row"><span>Inglês</span><span class="lang-level">Profissional</span></div>
          </div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .resume-page { opacity: 0; transition: opacity 0.7s ease; padding-bottom: 60px; }
    .resume-page.visible { opacity: 1; }

    .hero { position: relative; padding: 80px 16px 60px; text-align: center; overflow: hidden; }
    .hero-bg { position: absolute; inset: 0; background: linear-gradient(180deg, rgba(0,47,98,0.3) 0%, transparent 100%); pointer-events: none; }
    .hero-content { position: relative; max-width: 700px; margin: 0 auto; }
    h1 { font-size: 42px; font-weight: 800; color: #fff; margin: 0 0 8px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .accent { color: #4d9fff; }
    .title { color: #4d9fff; font-size: 18px; font-weight: 600; margin: 0 0 20px; }
    .badges { display: flex; flex-wrap: wrap; justify-content: center; gap: 8px; margin-bottom: 24px; }
    .badge { padding: 6px 16px; border-radius: 20px; background: rgba(26,35,50,0.8); border: 1px solid rgba(0,71,187,0.2); color: #94a3b8; font-size: 13px; }
    .actions { display: flex; flex-wrap: wrap; justify-content: center; gap: 12px; }
    .btn { display: inline-block; padding: 12px 24px; border-radius: 12px; font-size: 14px; font-weight: 600; text-decoration: none; transition: all 0.3s; font-family: 'Plus Jakarta Sans', sans-serif; cursor: pointer; border: none; }
    .btn-primary { background: #0047BB; color: #fff; }
    .btn-primary:hover { background: #0055DD; }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-outline { border: 1px solid rgba(0,71,187,0.4); color: #4d9fff; background: transparent; }
    .btn-outline:hover { background: rgba(0,71,187,0.1); }
    .btn-ghost { border: 1px solid rgba(255,255,255,0.2); color: #fff; background: transparent; }
    .btn-ghost:hover { background: rgba(255,255,255,0.05); }

    .section { max-width: 900px; margin: 0 auto; padding: 40px 16px; }
    h2 { font-size: 24px; font-weight: 700; color: #fff; margin: 0 0 24px; font-family: 'Plus Jakarta Sans', sans-serif; }

    .summary-card { padding: 24px; border-radius: 16px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); }
    .summary-card p { color: #94a3b8; font-size: 15px; line-height: 1.7; margin: 0; }

    .stack-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 16px; }
    .stack-card { padding: 20px; border-radius: 12px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); }
    .stack-card h3 { color: #4d9fff; font-size: 13px; font-weight: 600; text-transform: uppercase; letter-spacing: 1.5px; margin: 0 0 12px; }
    .chips { display: flex; flex-wrap: wrap; gap: 8px; }
    .chip { padding: 4px 12px; border-radius: 20px; background: rgba(0,71,187,0.1); border: 1px solid rgba(0,71,187,0.15); color: #94a3b8; font-size: 12px; transition: border-color 0.2s; }
    .chip:hover { border-color: rgba(0,71,187,0.4); }

    .projects { display: flex; flex-direction: column; gap: 16px; }
    .project-card { border-radius: 16px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); overflow: hidden; transition: border-color 0.3s; }
    .project-card:hover { border-color: rgba(0,71,187,0.3); }
    .project-header { display: flex; align-items: flex-start; justify-content: space-between; width: 100%; padding: 20px; background: transparent; border: none; cursor: pointer; text-align: left; gap: 16px; }
    .project-header h3 { color: #fff; font-size: 16px; font-weight: 600; margin: 0 0 4px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .project-stack { color: rgba(77,159,255,0.7); font-size: 12px; margin: 0; }
    .arrow { color: #4d9fff; font-size: 18px; transition: transform 0.3s; flex-shrink: 0; }
    .arrow.open { transform: rotate(180deg); }
    .project-body { max-height: 0; overflow: hidden; transition: max-height 0.5s ease, opacity 0.5s; opacity: 0; }
    .project-body.expanded { max-height: 500px; opacity: 1; }
    .project-body ul { padding: 0 20px 20px; margin: 0; list-style: none; }
    .project-body li { color: #94a3b8; font-size: 14px; padding: 4px 0; padding-left: 16px; position: relative; }
    .project-body li::before { content: ''; position: absolute; left: 0; top: 12px; width: 6px; height: 6px; border-radius: 50%; background: #4d9fff; }
    .project-link { display: inline-block; margin: 0 20px 20px; padding: 8px 20px; border-radius: 8px; background: rgba(0,71,187,0.1); color: #4d9fff; font-size: 13px; font-weight: 500; text-decoration: none; transition: background 0.2s; }
    .project-link:hover { background: rgba(0,71,187,0.2); }

    .two-cols { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .info-card { padding: 24px; border-radius: 16px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); }
    .info-card h2 { margin-bottom: 16px; }
    .info-card p { color: #94a3b8; margin: 0; }
    .lang-row { display: flex; justify-content: space-between; font-size: 14px; padding: 4px 0; color: #94a3b8; }
    .lang-level { color: #4d9fff; }

    @media (max-width: 768px) {
      h1 { font-size: 28px; }
      .two-cols { grid-template-columns: 1fr; }
    }
  `]
})
export class ResumeComponent implements OnInit {
  visible = false;
  generating = false;

  stackCategories: StackCategory[] = [
    { title: 'Backend', items: ['C# / .NET 8', 'ASP.NET Core', 'Entity Framework Core', 'Dapper'] },
    { title: 'Arquitetura', items: ['Clean Architecture', 'DDD', 'CQRS', 'Saga Pattern', 'Microsserviços'] },
    { title: 'Mensageria', items: ['Apache Kafka', 'RabbitMQ'] },
    { title: 'Bancos de Dados', items: ['PostgreSQL', 'SQL Server', 'MongoDB', 'Redis'] },
    { title: 'Frontend', items: ['Angular 17', 'React 18', 'TypeScript', 'Tailwind CSS'] },
    { title: 'DevOps', items: ['Docker', 'Docker Compose', 'GitHub Actions', 'Terraform', 'Nginx'] },
    { title: 'Cloud', items: ['AWS (EC2, Route53)', 'Oracle Cloud', 'Cloudflare'] },
    { title: 'Auth & Observabilidade', items: ['Keycloak (OAuth2/OIDC)', 'JWT', 'Seq', 'Serilog', 'Grafana', 'OpenTelemetry'] },
    { title: 'Testes', items: ['xUnit', 'NSubstitute', 'Testcontainers', 'Vitest'] },
  ];

  projects: Project[] = [
    {
      title: 'KRT Bank — Plataforma Bancária Digital',
      stack: '.NET 8, Angular 17, PostgreSQL, Kafka, RabbitMQ, Redis, Keycloak, Docker',
      points: [
        'Plataforma bancária completa com abertura de contas, PIX (QR Code EMV), Boleto e Cartão de Crédito Virtual',
        'Operações bancárias atômicas (débito/crédito/extrato) com garantia de consistência',
        '10 microsserviços Docker com Gateway (YARP), Rate Limiting e API Key Authentication',
        '145 testes automatizados (unitários + integração)',
        'Comprovantes PDF gerados server-side, QR Code EMV padrão Banco Central',
      ],
      url: 'https://bank.klisteneslima.dev',
      expanded: false,
    },
    {
      title: 'KLL Platform (AUREA Maison) — E-commerce de Joias de Luxo',
      stack: '.NET 8, React 18, TypeScript, PostgreSQL, MongoDB, Kafka, RabbitMQ, Redis, Keycloak, Docker',
      points: [
        'E-commerce completo com catálogo, carrinho, checkout multi-pagamento (PIX, Boleto, Cartão)',
        'Integração real com plataforma bancária (KRT Bank) via Anti-Corruption Layer (DDD)',
        '15 microsserviços Docker incluindo Gateway, Store, Pay, Logistics e Notifications',
        'Saga Pattern para orquestração de pedidos, Circuit Breaker com Polly',
        '244 testes automatizados (unitários + integração + vitest)',
      ],
      url: 'https://store.klisteneslima.dev',
      expanded: false,
    },
  ];

  ngOnInit() {
    window.scrollTo(0, 0);
    setTimeout(() => this.visible = true, 100);
  }

  async downloadPdf() {
    this.generating = true;
    try {
      const html2pdf = (await import('html2pdf.js' as any)).default;

      const stackHtml = this.stackCategories.map(cat =>
        `<p style="font-size:12px;margin:6px 0;"><strong style="color:#333;">${cat.title}:</strong> <span style="color:#555;">${cat.items.join(', ')}</span></p>`
      ).join('');

      const projectsHtml = this.projects.map(p =>
        `<div style="margin-bottom:16px;">
          <h3 style="font-size:14px;color:#1a1a1a;margin:0 0 4px;">${p.title}</h3>
          <p style="font-size:11px;color:#0047BB;margin:0 0 8px;">${p.stack}</p>
          <ul style="margin:0;padding-left:18px;">
            ${p.points.map(pt => `<li style="font-size:12px;color:#444;margin-bottom:4px;">${pt}</li>`).join('')}
          </ul>
        </div>`
      ).join('');

      const content = document.createElement('div');
      content.innerHTML = `
        <div style="font-family:'Helvetica Neue',Arial,sans-serif;color:#1a1a1a;padding:40px;max-width:800px;margin:0 auto;line-height:1.6;">
          <div style="border-bottom:3px solid #0047BB;padding-bottom:20px;margin-bottom:24px;">
            <h1 style="font-size:28px;margin:0 0 4px;color:#1a1a1a;">Klístenes de Lima Leite</h1>
            <p style="font-size:16px;color:#0047BB;margin:0 0 12px;font-weight:600;">Engenheiro de Software Sênior .NET</p>
            <p style="font-size:12px;color:#666;margin:0;">Cajazeiras, PB, Brasil | klisteneswar3&#64;gmail.com | linkedin.com/in/klistenes-de-lima-leite-257209194 | github.com/KlistenesLima</p>
          </div>
          <div style="margin-bottom:24px;">
            <h2 style="font-size:16px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:6px;margin-bottom:12px;">Resumo Profissional</h2>
            <p style="font-size:13px;color:#333;">Engenheiro de Software Sênior com 7 anos de experiência em desenvolvimento de sistemas distribuídos e microsserviços com .NET. Especialista em Clean Architecture, DDD, CQRS e Event-Driven Architecture.</p>
          </div>
          <div style="margin-bottom:24px;">
            <h2 style="font-size:16px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:6px;margin-bottom:12px;">Stack Técnico</h2>
            ${stackHtml}
          </div>
          <div style="margin-bottom:24px;">
            <h2 style="font-size:16px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:6px;margin-bottom:12px;">Projetos Destaque</h2>
            ${projectsHtml}
          </div>
          <div style="margin-bottom:24px;">
            <h2 style="font-size:16px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:6px;margin-bottom:12px;">Formação</h2>
            <p style="font-size:13px;color:#333;">Bacharelado em Ciência da Computação</p>
          </div>
          <div>
            <h2 style="font-size:16px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:6px;margin-bottom:12px;">Idiomas</h2>
            <p style="font-size:13px;color:#333;">Português (Nativo) | Inglês (Profissional)</p>
          </div>
        </div>
      `;

      await html2pdf().set({
        margin: [10, 10, 10, 10],
        filename: 'Klistenes-Lima-Curriculo.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
      }).from(content).save();
    } catch (err) {
      console.error('Erro ao gerar PDF:', err);
    } finally {
      this.generating = false;
    }
  }
}
