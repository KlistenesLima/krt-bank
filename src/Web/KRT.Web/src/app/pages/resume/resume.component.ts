import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface StackCategory {
  title: string;
  items: string[];
}

interface Experience {
  company: string;
  role: string;
  period: string;
  points: string[];
  expanded: boolean;
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
  imports: [CommonModule, RouterModule],
  template: `
    <div class="resume-page" [class.visible]="visible">
      <!-- Back to KRT Bank Bar -->
      <div class="krt-back-bar">
        <a routerLink="/dashboard" class="krt-back-link">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
          <span>Voltar ao KRT Bank</span>
        </a>
      </div>
      <!-- Header -->
      <section class="hero">
        <div class="hero-bg"></div>
        <div class="hero-content">
          <img src="assets/foto-perfil.png" alt="Kl&iacute;stenes Lima" class="profile-photo" />
          <h1>Kl&iacute;stenes de Lima <span class="accent">Leite</span></h1>
          <p class="title">Senior Software Engineer | .NET Specialist</p>
          <p class="subtitle">Sistemas Distribu&iacute;dos | Arquitetura Backend | Microsservi&ccedil;os</p>
          <div class="badges">
            <span class="badge">+7 anos exp</span>
            <span class="badge">Jo&atilde;o Pessoa, PB</span>
            <span class="badge">Dispon&iacute;vel</span>
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
          <p>Engenheiro de Software S&ecirc;nior com 7+ anos de experi&ecirc;ncia em desenvolvimento de sistemas distribu&iacute;dos de alta performance.
          Especialista em ecossistemas .NET e arquitetura de microsservi&ccedil;os para o setor financeiro e banc&aacute;rio.
          Experi&ecirc;ncia comprovada na constru&ccedil;&atilde;o de plataformas banc&aacute;rias digitais completas com processamento de pagamentos em tempo real,
          detec&ccedil;&atilde;o de fraude e conformidade regulat&oacute;ria. Apaixonado por Clean Architecture, Domain-Driven Design e pr&aacute;ticas de
          engenharia que garantem escalabilidade, resili&ecirc;ncia e seguran&ccedil;a.</p>
        </div>
      </section>

      <!-- Stack -->
      <section class="section">
        <h2>Compet&ecirc;ncias <span class="accent">T&eacute;cnicas</span></h2>
        <div class="stack-grid">
          <div class="stack-card" *ngFor="let cat of stackCategories">
            <h3>{{ cat.title }}</h3>
            <div class="chips">
              <span class="chip" *ngFor="let item of cat.items">{{ item }}</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Experi&ecirc;ncia -->
      <section class="section">
        <h2>Experi&ecirc;ncia <span class="accent">Profissional</span></h2>
        <div class="experiences">
          <div class="exp-card" *ngFor="let exp of experiences; let i = index">
            <button class="exp-header" (click)="exp.expanded = !exp.expanded">
              <div>
                <h3>{{ exp.company }}</h3>
                <p class="exp-role">{{ exp.role }}</p>
                <p class="exp-period">{{ exp.period }}</p>
              </div>
              <span class="arrow" [class.open]="exp.expanded">&#9662;</span>
            </button>
            <div class="exp-body" [class.expanded]="exp.expanded">
              <ul>
                <li *ngFor="let point of exp.points">{{ point }}</li>
              </ul>
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

      <!-- Forma&ccedil;&atilde;o e Idiomas -->
      <section class="section">
        <div class="two-cols">
          <div class="info-card">
            <h2>Forma&ccedil;&atilde;o Acad&ecirc;mica</h2>
            <ul class="edu-list">
              <li *ngFor="let ed of education">{{ ed }}</li>
            </ul>
          </div>
          <div class="info-card">
            <h2>Idiomas</h2>
            <div class="lang-row"><span>Portugu&ecirc;s</span><span class="lang-level">Nativo</span></div>
            <div class="lang-row"><span>Ingl&ecirc;s</span><span class="lang-level">Intermedi&aacute;rio</span></div>
            <div class="lang-row"><span>Espanhol</span><span class="lang-level">Intermedi&aacute;rio</span></div>
          </div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    :host { display: block; background: #0a0f1e; }
    .resume-page { opacity: 0; transition: opacity 0.7s ease; padding-bottom: 60px; background: #0a0f1e; color: #e2e8f0; min-height: 100vh; font-family: 'Plus Jakarta Sans', sans-serif; }
    .resume-page.visible { opacity: 1; }

    .krt-back-bar { position: fixed; top: 36px; left: 0; right: 0; z-index: 9998; background: #0047BB; box-shadow: 0 2px 8px rgba(0,0,0,0.2); }
    .krt-back-link { display: flex; align-items: center; justify-content: center; gap: 8px; padding: 12px 16px; color: #fff; text-decoration: none; font-size: 0.88rem; font-weight: 600; font-family: 'Plus Jakarta Sans', sans-serif; transition: background 0.2s; }
    .krt-back-link:hover { background: rgba(255,255,255,0.1); }

    .profile-photo { width: 160px; height: 160px; border-radius: 50%; object-fit: cover; object-position: center top; border: 3px solid rgba(0,71,187,0.5); box-shadow: 0 8px 32px rgba(0,0,0,0.3); margin: 0 auto 20px; display: block; }
    .hero { position: relative; padding: 128px 16px 60px; text-align: center; overflow: hidden; }
    .hero-bg { position: absolute; inset: 0; background: linear-gradient(180deg, rgba(0,47,98,0.3) 0%, transparent 100%); pointer-events: none; }
    .hero-content { position: relative; max-width: 700px; margin: 0 auto; }
    h1 { font-size: 42px; font-weight: 800; color: #fff; margin: 0 0 8px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .accent { color: #4d9fff; }
    .title { color: #4d9fff; font-size: 18px; font-weight: 600; margin: 0 0 4px; }
    .subtitle { color: #94a3b8; font-size: 14px; font-weight: 400; margin: 0 0 20px; }
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

    .experiences { display: flex; flex-direction: column; gap: 16px; }
    .exp-card { border-radius: 16px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); overflow: hidden; transition: border-color 0.3s; }
    .exp-card:hover { border-color: rgba(0,71,187,0.3); }
    .exp-header { display: flex; align-items: flex-start; justify-content: space-between; width: 100%; padding: 20px; background: transparent; border: none; cursor: pointer; text-align: left; gap: 16px; }
    .exp-header h3 { color: #fff; font-size: 16px; font-weight: 600; margin: 0 0 4px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .exp-role { color: rgba(77,159,255,0.8); font-size: 13px; margin: 0 0 2px; }
    .exp-period { color: rgba(148,163,184,0.6); font-size: 12px; margin: 0; }
    .exp-body { max-height: 0; overflow: hidden; transition: max-height 0.5s ease, opacity 0.5s; opacity: 0; }
    .exp-body.expanded { max-height: 500px; opacity: 1; }
    .exp-body ul { padding: 0 20px 20px; margin: 0; list-style: none; }
    .exp-body li { color: #94a3b8; font-size: 14px; padding: 4px 0; padding-left: 16px; position: relative; }
    .exp-body li::before { content: ''; position: absolute; left: 0; top: 12px; width: 6px; height: 6px; border-radius: 50%; background: #4d9fff; }

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
    .edu-list { list-style: none; padding: 0; margin: 0; }
    .edu-list li { color: #94a3b8; font-size: 13px; padding: 4px 0; padding-left: 16px; position: relative; }
    .edu-list li::before { content: ''; position: absolute; left: 0; top: 11px; width: 5px; height: 5px; border-radius: 50%; background: #4d9fff; }
    .lang-row { display: flex; justify-content: space-between; font-size: 14px; padding: 4px 0; color: #94a3b8; }
    .lang-level { color: #4d9fff; }

    @media (max-width: 768px) {
      h1 { font-size: 28px; }
      .two-cols { grid-template-columns: 1fr; }
      .profile-photo { width: 120px; height: 120px; }
      .stack-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class ResumeComponent implements OnInit {
  visible = false;
  generating = false;

  stackCategories: StackCategory[] = [
    { title: '\uD83D\uDD27 Backend & Runtime', items: ['C# 12', '.NET 8', 'ASP.NET Core Web API', 'Entity Framework Core 8', 'Dapper', 'LINQ', 'FluentValidation', 'MediatR (CQRS)', 'SignalR (WebSockets)', 'Polly (Resili\u00eancia)', 'Hangfire', 'Background Services', 'gRPC'] },
    { title: '\uD83C\uDFD7\uFE0F Arquitetura & Padr\u00f5es', items: ['Clean Architecture', 'Domain-Driven Design (DDD)', 'CQRS', 'Event-Driven Architecture', 'Saga Pattern (Orquestra\u00e7\u00e3o + Coreografia)', 'Outbox Pattern', 'Anti-Corruption Layer (ACL)', 'Repository Pattern', 'Unit of Work', 'Specification Pattern', 'Circuit Breaker / Retry / Timeout', 'Event Sourcing', 'API Gateway (YARP)', 'Compensating Transaction'] },
    { title: '\uD83C\uDFE6 Sistemas Financeiros & Banc\u00e1rios', items: ['Processamento de Pagamentos (PIX / Boleto / Cart\u00e3o)', 'Transfer\u00eancias em Tempo Real (PIX Instant)', 'Detec\u00e7\u00e3o de Fraude (Scoring & Analysis)', 'Gera\u00e7\u00e3o de QR Code (PIX/EMV)', 'Cart\u00f5es Virtuais (Luhn / CVV Rotation)', 'Emiss\u00e3o de Boletos', 'Concilia\u00e7\u00e3o Banc\u00e1ria', 'KYC (Know Your Customer)', 'Idempot\u00eancia em Transa\u00e7\u00f5es', 'Optimistic Concurrency Control'] },
    { title: '\uD83D\uDDC4\uFE0F Banco de Dados', items: ['PostgreSQL 16', 'SQL Server', 'MongoDB 7', 'Redis 7 (Cache & Session)', 'EF Core Migrations', 'Query Optimization / Indexing', 'Transa\u00e7\u00f5es Distribu\u00eddas'] },
    { title: '\uD83D\uDCE8 Mensageria & Event Streaming', items: ['Apache Kafka (Confluent)', 'RabbitMQ (Exchanges / Queues / DLX)', 'Event Bus / Integration Events', 'Dead Letter Queues', 'Idempotent Consumers', 'Kafka Topics & Consumer Groups', 'Message Ordering & Partitioning'] },
    { title: '\u2601\uFE0F Cloud & DevOps', items: ['AWS EC2', 'AWS S3 / Backblaze B2', 'Docker & Docker Compose (26+ containers)', 'Nginx (Reverse Proxy & SSL)', "Let's Encrypt (SSL/TLS)", 'CI/CD Pipelines', 'GitHub Actions', 'Linux Administration (Ubuntu)', 'SSH & Firewall (UFW)'] },
    { title: '\uD83D\uDD10 Seguran\u00e7a', items: ['JWT (HS256/RS256)', 'Keycloak 23 (IAM)', 'OAuth 2.0 / OpenID Connect', 'RBAC', 'BCrypt Password Hashing', 'API Key Authentication', 'Rate Limiting & Throttling', 'Security Headers (CSP / HSTS)', 'CORS Configuration', 'Credential Rotation'] },
    { title: '\uD83D\uDCCA Observabilidade & Logging', items: ['Serilog + Seq', 'OpenTelemetry', 'Grafana Cloud (Tempo / Mimir / Loki)', 'Structured Logging', 'Distributed Tracing', 'Health Checks', 'APM'] },
    { title: '\uD83C\uDFA8 Frontend', items: ['Angular 17 + Material 17', 'React 18 + TypeScript 5', 'Tailwind CSS 3', 'Material-UI (MUI) v5', 'Zustand', 'TanStack React Query', 'Vite 5', 'Responsive Design (Mobile-First)'] },
    { title: '\uD83E\uDDEA Testes', items: ['xUnit / NUnit', 'Integration Tests (WebApplicationFactory)', 'Vitest + React Testing Library', 'Test Containers', 'Moq / NSubstitute', 'TDD / BDD Practices', '244+ testes automatizados'] },
    { title: '\uD83D\uDD04 Metodologias & Pr\u00e1ticas', items: ['SOLID Principles', 'Design Patterns (GoF)', 'Conventional Commits', 'Git Flow', 'Code Review', 'Agile / Scrum / Kanban', 'API-First Design', 'RESTful API Design', 'Semantic Versioning'] },
  ];

  experiences: Experience[] = [
    {
      company: 'Projetos Independentes',
      role: 'Senior Software Engineer',
      period: '2024 \u2014 Presente',
      points: [
        'Arquitetei e desenvolvi ecossistema completo de fintech: plataforma banc\u00e1ria digital (KRT Bank) + e-commerce de luxo (KLL Platform) integrados via microsservi\u00e7os',
        'Implementei processamento de pagamentos em tempo real (PIX, Boleto, Cart\u00e3o) com detec\u00e7\u00e3o de fraude, saga pattern para transa\u00e7\u00f5es distribu\u00eddas e compensa\u00e7\u00e3o autom\u00e1tica',
        'Constru\u00ed infraestrutura de 26 containers Docker em produ\u00e7\u00e3o na AWS EC2 com PostgreSQL, MongoDB, Redis, Kafka, RabbitMQ, Keycloak e Seq',
        'Desenvolvi 3 frontends (Angular 17 + React 18) com design responsivo, autentica\u00e7\u00e3o JWT/Keycloak e RBAC para 5 tipos de usu\u00e1rio',
        'Implementei event-driven architecture com Kafka para integra\u00e7\u00e3o entre sistemas e RabbitMQ para notifica\u00e7\u00f5es, garantindo entrega at-least-once via outbox pattern',
        'Conduzi auditoria de seguran\u00e7a completa com remedia\u00e7\u00e3o de vulnerabilidades cr\u00edticas, rota\u00e7\u00e3o de credenciais e hardening de infraestrutura',
        'Alcancei 244+ testes automatizados (unit\u00e1rios + integra\u00e7\u00e3o) com cobertura de dom\u00ednio, handlers e APIs',
      ],
      expanded: true,
    },
    {
      company: 'QINTESS',
      role: 'Senior Software Engineer .NET',
      period: 'Mar\u00e7o de 2025 \u2014 Atual',
      points: [
        'Lideran\u00e7a t\u00e9cnica na arquitetura e evolu\u00e7\u00e3o de sistemas cr\u00edticos governamentais e de sa\u00fade de alta disponibilidade',
        'Modelagem e implementa\u00e7\u00e3o de dom\u00ednios complexos com DDD, APIs resilientes e workflows ass\u00edncronos com RabbitMQ',
        'Defini\u00e7\u00e3o de pr\u00e1ticas arquiteturais, pipelines CI/CD robustos e melhores pr\u00e1ticas de engenharia backend',
        'Design e implementa\u00e7\u00e3o de servi\u00e7os distribu\u00eddos escal\u00e1veis com SLA exigentes',
      ],
      expanded: true,
    },
    {
      company: 'G4F Solu\u00e7\u00f5es Corporativas',
      role: 'Senior Software Engineer .NET',
      period: 'Junho de 2022 \u2014 Mar\u00e7o de 2025',
      points: [
        'Lideran\u00e7a t\u00e9cnica em projetos enterprise complexos, coordenando integra\u00e7\u00e3o entre sistemas legados e plataformas modernas',
        'Defini\u00e7\u00e3o e implementa\u00e7\u00e3o de padr\u00f5es arquiteturais para toda a organiza\u00e7\u00e3o',
        'Constru\u00e7\u00e3o e otimiza\u00e7\u00e3o de pipelines CI/CD, reduzindo tempo de deploy',
        'Mentoria de engenheiros juniores e plenos, elevando a maturidade t\u00e9cnica da equipe',
        'Implementa\u00e7\u00e3o de APIs RESTful e microsservi\u00e7os com .NET Core',
      ],
      expanded: false,
    },
    {
      company: 'Afixcode',
      role: 'Software Engineer .NET',
      period: 'Setembro de 2021 \u2014 Junho de 2022',
      points: [
        'Desenvolvimento de APIs REST robustas e sistemas SaaS escal\u00e1veis com .NET Core',
        'Implementa\u00e7\u00e3o de arquiteturas orientadas a eventos com RabbitMQ',
        'Cria\u00e7\u00e3o de interfaces frontend responsivas com React, TypeScript e Redux',
        'Integra\u00e7\u00e3o com SQL Server, PostgreSQL e MongoDB usando EF Core e Dapper',
      ],
      expanded: false,
    },
    {
      company: 'Neotriad S/A',
      role: 'Software Engineer .NET',
      period: 'Novembro de 2019 \u2014 Junho de 2022',
      points: [
        'Desenvolvimento de sistemas backend corporativos com foco em performance e estabilidade',
        'Implementa\u00e7\u00e3o de solu\u00e7\u00f5es com ASP.NET MVC e Web API',
        'Otimiza\u00e7\u00e3o de queries e modelagem de banco de dados para aplica\u00e7\u00f5es cr\u00edticas',
        'Implementa\u00e7\u00e3o de testes unit\u00e1rios e de integra\u00e7\u00e3o',
      ],
      expanded: false,
    },
    {
      company: 'IBGE',
      role: 'Agente de Pesquisa e Mapeamento',
      period: 'Novembro de 2016 \u2014 Novembro de 2019',
      points: [
        'Execu\u00e7\u00e3o de projetos estat\u00edsticos de grande escala e controle de qualidade de dados em n\u00edvel nacional',
        'Coleta, valida\u00e7\u00e3o e an\u00e1lise de dados cens\u00edt\u00e1rios e pesquisas estat\u00edsticas em campo',
      ],
      expanded: false,
    },
  ];

  education: string[] = [
    'P\u00f3s-Gradua\u00e7\u00e3o em Desenvolvimento de Aplica\u00e7\u00f5es .NET',
    'P\u00f3s-Gradua\u00e7\u00e3o em Engenharia de Software',
    'P\u00f3s-Gradua\u00e7\u00e3o em Business Intelligence, Big Data e Intelig\u00eancia Artificial',
    'P\u00f3s-Gradua\u00e7\u00e3o em Administra\u00e7\u00e3o de Banco de Dados',
    'P\u00f3s-Gradua\u00e7\u00e3o em Ci\u00eancia de Dados',
    'P\u00f3s-Gradua\u00e7\u00e3o em Desenvolvimento Web',
    'P\u00f3s-Gradua\u00e7\u00e3o em Algoritmos e Estruturas de Dados',
    'P\u00f3s-Gradua\u00e7\u00e3o em Product Management',
    'Tecn\u00f3logo em An\u00e1lise e Desenvolvimento de Sistemas \u2014 UNIP',
  ];

  projects: Project[] = [
    {
      title: 'KRT Bank \u2014 Plataforma Banc\u00e1ria Digital',
      stack: '.NET 8, PostgreSQL, Redis, Kafka, RabbitMQ, SignalR, Keycloak, Angular 17',
      points: [
        'Banco digital completo com contas, PIX, boletos, cart\u00f5es virtuais',
        'Saga pattern para transfer\u00eancias PIX com an\u00e1lise de fraude em tempo real',
        'Dashboard admin com m\u00e9tricas via SignalR (WebSocket)',
        'API RESTful com rate limiting, health checks e observabilidade via OpenTelemetry',
      ],
      url: 'https://bank.klisteneslima.dev',
      expanded: false,
    },
    {
      title: 'KLL Platform \u2014 E-commerce de Luxo (AUREA Maison)',
      stack: '.NET 8, PostgreSQL, MongoDB, Kafka, RabbitMQ, React 18, Tailwind CSS',
      points: [
        '3 microsservi\u00e7os (Store, Pay, Logistics) + 3 frontends',
        'Anti-Corruption Layer para integra\u00e7\u00e3o com sistema banc\u00e1rio',
        'Saga de pedidos: reserva de estoque \u2192 pagamento \u2192 entrega',
        'Admin dashboard com MUI Data Grid e React Query',
      ],
      url: 'https://store.klisteneslima.dev',
      expanded: false,
    },
    {
      title: 'Infraestrutura Cloud (AWS)',
      stack: 'AWS EC2, Docker Compose, Nginx, Let\'s Encrypt, GitHub Actions',
      points: [
        '26 containers em produ\u00e7\u00e3o com limites de mem\u00f3ria otimizados (~4GB total)',
        'SSL/TLS autom\u00e1tico via certbot para 5 dom\u00ednios personalizados',
        'Reverse proxy Nginx para roteamento de m\u00faltiplos servi\u00e7os',
        'Deploy automatizado com scripts bash e git workflow',
      ],
      url: 'https://github.com/KlistenesLima',
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

      const expHtml = this.experiences.map(exp =>
        `<div style="margin-bottom:14px;">
          <h3 style="font-size:14px;color:#1a1a1a;margin:0 0 2px;">${exp.company} \u2014 ${exp.role}</h3>
          <p style="font-size:11px;color:#0047BB;margin:0 0 6px;font-style:italic;">${exp.period}</p>
          <ul style="margin:0;padding-left:18px;">${exp.points.map(pt => `<li style="font-size:12px;color:#444;margin-bottom:3px;">${pt}</li>`).join('')}</ul>
        </div>`
      ).join('');

      const projHtml = this.projects.map(p =>
        `<div style="margin-bottom:14px;">
          <h3 style="font-size:14px;color:#1a1a1a;margin:0 0 2px;">${p.title}</h3>
          <p style="font-size:11px;color:#0047BB;margin:0 0 6px;">${p.stack}</p>
          <ul style="margin:0;padding-left:18px;">${p.points.map(pt => `<li style="font-size:12px;color:#444;margin-bottom:3px;">${pt}</li>`).join('')}</ul>
        </div>`
      ).join('');

      const eduHtml = this.education.map(e => `<li style="font-size:12px;color:#444;margin-bottom:3px;">${e}</li>`).join('');

      const content = document.createElement('div');
      content.innerHTML = `
        <div style="font-family:'Helvetica Neue',Arial,sans-serif;color:#1a1a1a;padding:40px;max-width:800px;margin:0 auto;line-height:1.5;">
          <div style="border-bottom:3px solid #0047BB;padding-bottom:16px;margin-bottom:20px;">
            <h1 style="font-size:26px;margin:0 0 4px;color:#1a1a1a;">KL\u00cdSTENES DE LIMA LEITE</h1>
            <p style="font-size:14px;color:#0047BB;margin:0 0 8px;font-weight:600;">Senior Software Engineer | .NET Specialist</p>
            <p style="font-size:11px;color:#666;margin:0;">Jo\u00e3o Pessoa, PB | (83) 9 8177-9792 | klisteneswar2@hotmail.com | linkedin.com/in/klisteneslima | github.com/KlistenesLima</p>
          </div>
          <div style="margin-bottom:18px;">
            <h2 style="font-size:14px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:4px;margin-bottom:8px;">Resumo Profissional</h2>
            <p style="font-size:12px;color:#333;">Engenheiro de Software S\u00eanior com 7+ anos de experi\u00eancia em desenvolvimento de sistemas distribu\u00eddos de alta performance. Especialista em ecossistemas .NET e arquitetura de microsservi\u00e7os para o setor financeiro e banc\u00e1rio. Experi\u00eancia comprovada na constru\u00e7\u00e3o de plataformas banc\u00e1rias digitais completas com processamento de pagamentos em tempo real, detec\u00e7\u00e3o de fraude e conformidade regulat\u00f3ria.</p>
          </div>
          <div style="margin-bottom:18px;">
            <h2 style="font-size:14px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:4px;margin-bottom:8px;">Compet\u00eancias T\u00e9cnicas</h2>
            ${stackHtml}
          </div>
          <div style="margin-bottom:18px;">
            <h2 style="font-size:14px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:4px;margin-bottom:8px;">Experi\u00eancia Profissional</h2>
            ${expHtml}
          </div>
          <div style="margin-bottom:18px;">
            <h2 style="font-size:14px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:4px;margin-bottom:8px;">Projetos Destaque</h2>
            ${projHtml}
          </div>
          <div style="margin-bottom:18px;">
            <h2 style="font-size:14px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:4px;margin-bottom:8px;">Forma\u00e7\u00e3o Acad\u00eamica</h2>
            <ul style="margin:0;padding-left:18px;">${eduHtml}</ul>
          </div>
          <div>
            <h2 style="font-size:14px;text-transform:uppercase;letter-spacing:2px;color:#0047BB;border-bottom:1px solid #ddd;padding-bottom:4px;margin-bottom:8px;">Idiomas</h2>
            <p style="font-size:12px;color:#333;">Portugu\u00eas (Nativo) | Ingl\u00eas (Intermedi\u00e1rio) | Espanhol (Intermedi\u00e1rio)</p>
          </div>
        </div>
      `;

      await html2pdf().set({
        margin: [10, 10, 10, 10],
        filename: 'Klistenes-Lima-Curriculo.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
      }).from(content).save();
    } catch (err) {
      console.error('Erro ao gerar PDF:', err);
    } finally {
      this.generating = false;
    }
  }
}
