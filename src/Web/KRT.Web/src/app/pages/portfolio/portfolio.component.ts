import { Component, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface TechItem {
  name: string;
  color: string;
}

interface TechCategory {
  title: string;
  icon: string;
  items: TechItem[];
}

interface Experience {
  company: string;
  role: string;
  period: string;
  current: boolean;
  bullets: string[];
}

interface Project {
  name: string;
  subtitle: string;
  type: string;
  description: string;
  tech: string[];
  metrics: { label: string; value: string }[];
  github: string;
  accent: string;
}

interface Education {
  degree: string;
  institution?: string;
}

@Component({
  selector: 'app-portfolio',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <!-- Portfolio Page -->
    <div class="portfolio-page">

      <!-- Hero Section -->
      <section class="hero">
        <div class="hero-pattern"></div>
        <div class="hero-content">
          <div class="hero-avatar">
            <svg viewBox="0 0 120 120" fill="none" xmlns="http://www.w3.org/2000/svg">
              <circle cx="60" cy="60" r="58" stroke="url(#avatarGrad)" stroke-width="3" fill="rgba(0,71,187,0.15)"/>
              <circle cx="60" cy="45" r="18" fill="rgba(0,71,187,0.4)"/>
              <ellipse cx="60" cy="85" rx="28" ry="20" fill="rgba(0,71,187,0.4)"/>
              <defs>
                <linearGradient id="avatarGrad" x1="0" y1="0" x2="120" y2="120">
                  <stop offset="0%" stop-color="#0047BB"/>
                  <stop offset="100%" stop-color="#00D4AA"/>
                </linearGradient>
              </defs>
            </svg>
          </div>
          <h1 class="hero-name">Klístenes de Lima Leite</h1>
          <p class="hero-title">
            <span class="typed-text">{{ typedText }}</span><span class="cursor" [class.blink]="typingDone">|</span>
          </p>
          <div class="hero-location">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
            <span>João Pessoa, PB</span>
          </div>
          <div class="hero-links">
            <a href="https://www.linkedin.com/in/klistenes-de-lima-leite-257209194/" target="_blank" rel="noopener" class="social-btn" title="LinkedIn">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433a2.062 2.062 0 0 1-2.063-2.065 2.064 2.064 0 1 1 2.063 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/></svg>
              <span>LinkedIn</span>
            </a>
            <a href="https://github.com/KlistenesLima" target="_blank" rel="noopener" class="social-btn" title="GitHub">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/></svg>
              <span>GitHub</span>
            </a>
            <a href="mailto:klisteneswar2@hotmail.com" class="social-btn" title="Email">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="4" width="20" height="16" rx="2"/><path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/></svg>
              <span>Email</span>
            </a>
            <a href="tel:+5583981779792" class="social-btn" title="Telefone">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
              <span>(83) 9 8177-9792</span>
            </a>
          </div>
          <div class="scroll-indicator">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M19 12l-7 7-7-7"/></svg>
          </div>
        </div>
      </section>

      <!-- About Section -->
      <section class="section about reveal">
        <div class="container">
          <h2 class="section-title">Sobre</h2>
          <div class="about-grid">
            <div class="about-text">
              <p>Senior Full Stack Engineer com mais de <strong>7 anos de experiência</strong> projetando e escalando sistemas distribuídos, APIs críticas e plataformas backend modernas em ambientes enterprise e governamentais.</p>
              <p>Especialista em <strong>Domain-Driven Design (DDD)</strong>, arquitetura de mensageria, integração de sistemas complexos e desenvolvimento de arquiteturas resilientes e escaláveis.</p>
              <p>Atualmente liderando a arquitetura e evolução de sistemas críticos governamentais e de saúde na <strong>Qintess</strong>, com foco em modelagem DDD, APIs resilientes e workflows de mensageria assíncrona.</p>
            </div>
            <div class="stats-grid">
              <div class="stat-card" *ngFor="let stat of stats">
                <span class="counter-value" [attr.data-target]="stat.value">0</span>
                <span class="counter-suffix">{{ stat.suffix }}</span>
                <span class="stat-label">{{ stat.label }}</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Tech Stack Section -->
      <section class="section tech reveal">
        <div class="container">
          <h2 class="section-title">Tech Stack</h2>
          <div class="tech-categories">
            <div class="tech-category reveal" *ngFor="let category of techCategories; let i = index" [style.transition-delay]="(i * 100) + 'ms'">
              <h3 class="tech-category-title">
                <span class="tech-icon" [innerHTML]="category.icon"></span>
                {{ category.title }}
              </h3>
              <div class="tech-grid">
                <span class="tech-badge" *ngFor="let item of category.items" [style.border-color]="item.color + '40'" [style.color]="item.color">
                  {{ item.name }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Experience Section -->
      <section class="section experience reveal">
        <div class="container">
          <h2 class="section-title">Experiência</h2>
          <div class="timeline">
            <div class="timeline-item reveal" *ngFor="let exp of experiences; let i = index" [style.transition-delay]="(i * 150) + 'ms'">
              <div class="timeline-dot" [class.current]="exp.current"></div>
              <div class="timeline-content">
                <div class="timeline-header">
                  <div>
                    <h3 class="timeline-company">{{ exp.company }}</h3>
                    <p class="timeline-role">{{ exp.role }}</p>
                  </div>
                  <div class="timeline-period">
                    <span class="current-badge" *ngIf="exp.current">Atual</span>
                    {{ exp.period }}
                  </div>
                </div>
                <ul class="timeline-bullets">
                  <li *ngFor="let bullet of exp.bullets">{{ bullet }}</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Projects Section -->
      <section class="section projects reveal">
        <div class="container">
          <h2 class="section-title">Projetos em Destaque</h2>
          <p class="section-subtitle">Ecossistema integrado de microsserviços — 25 containers, 389 testes, 0 falhas</p>
          <div class="projects-grid">
            <div class="project-card reveal" *ngFor="let project of projects; let i = index" [style.transition-delay]="(i * 200) + 'ms'">
              <div class="project-accent" [style.background]="project.accent"></div>
              <div class="project-body">
                <div class="project-header">
                  <h3 class="project-name">{{ project.name }}</h3>
                  <span class="project-type">{{ project.type }}</span>
                </div>
                <p class="project-subtitle">{{ project.subtitle }}</p>
                <p class="project-desc">{{ project.description }}</p>
                <div class="project-metrics">
                  <div class="metric" *ngFor="let m of project.metrics">
                    <span class="metric-value">{{ m.value }}</span>
                    <span class="metric-label">{{ m.label }}</span>
                  </div>
                </div>
                <div class="project-tech">
                  <span class="project-tech-badge" *ngFor="let t of project.tech">{{ t }}</span>
                </div>
                <a [href]="project.github" target="_blank" rel="noopener" class="project-link">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/></svg>
                  Ver no GitHub
                </a>
              </div>
            </div>
          </div>
          <div class="ecosystem-badge reveal">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></svg>
            <span>Integração via Anti-Corruption Layer (KLL Pay → KRT Bank) com Circuit Breaker (Polly)</span>
          </div>
        </div>
      </section>

      <!-- Education Section -->
      <section class="section education reveal">
        <div class="container">
          <h2 class="section-title">Formação</h2>
          <div class="edu-grid">
            <div class="edu-card reveal" *ngFor="let edu of education; let i = index" [style.transition-delay]="(i * 80) + 'ms'">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 10v6M2 10l10-5 10 5-10 5z"/><path d="M6 12v5c0 1.1 2.7 3 6 3s6-1.9 6-3v-5"/></svg>
              <div>
                <p class="edu-degree">{{ edu.degree }}</p>
                <p class="edu-institution" *ngIf="edu.institution">{{ edu.institution }}</p>
              </div>
            </div>
          </div>
          <div class="languages reveal">
            <h3>Idiomas</h3>
            <div class="lang-badges">
              <span class="lang-badge">Português <small>(Nativo)</small></span>
              <span class="lang-badge">Inglês <small>(Intermediário)</small></span>
              <span class="lang-badge">Espanhol <small>(Intermediário)</small></span>
            </div>
          </div>
        </div>
      </section>

      <!-- Contact Section -->
      <section class="section contact reveal">
        <div class="container">
          <h2 class="section-title">Vamos Conversar?</h2>
          <p class="contact-text">Estou aberto a oportunidades de liderança técnica, arquitetura de sistemas e projetos desafiadores.</p>
          <div class="contact-links">
            <a href="mailto:klisteneswar2@hotmail.com" class="contact-btn primary">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="4" width="20" height="16" rx="2"/><path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/></svg>
              Enviar Email
            </a>
            <a href="https://www.linkedin.com/in/klistenes-de-lima-leite-257209194/" target="_blank" rel="noopener" class="contact-btn">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433a2.062 2.062 0 0 1-2.063-2.065 2.064 2.064 0 1 1 2.063 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/></svg>
              LinkedIn
            </a>
            <a href="https://github.com/KlistenesLima" target="_blank" rel="noopener" class="contact-btn">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/></svg>
              GitHub
            </a>
          </div>
        </div>
      </section>

      <!-- Back Button -->
      <div class="back-section">
        <a routerLink="/" class="back-btn">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
          Voltar ao App
        </a>
      </div>

    </div>
  `,
  styles: [`
    /* ========== Base & Reset ========== */
    :host {
      display: block;
      --blue: #0047BB;
      --blue-dark: #002a70;
      --blue-light: #3375d6;
      --accent: #00D4AA;
      --accent-dark: #00B894;
      --bg: #0a0f1e;
      --surface: #111827;
      --surface-light: #1a2332;
      --text: #e2e8f0;
      --text-secondary: #94a3b8;
      --text-muted: #64748b;
      --border: rgba(0,71,187,0.2);
      --radius: 16px;
      --radius-sm: 10px;
      --font: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    }

    .portfolio-page {
      font-family: var(--font);
      color: var(--text);
      background: var(--bg);
      min-height: 100vh;
      overflow-x: hidden;
    }

    .container {
      max-width: 1100px;
      margin: 0 auto;
      padding: 0 24px;
    }

    /* ========== Hero Section ========== */
    .hero {
      position: relative;
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      text-align: center;
      background: linear-gradient(160deg, #001a4d 0%, #0047BB 40%, #002a70 70%, #0a0f1e 100%);
      overflow: hidden;
    }

    .hero-pattern {
      position: absolute;
      inset: 0;
      background-image:
        radial-gradient(circle at 20% 30%, rgba(0,212,170,0.08) 0%, transparent 40%),
        radial-gradient(circle at 80% 70%, rgba(0,71,187,0.15) 0%, transparent 40%);
      pointer-events: none;
    }

    .hero-pattern::after {
      content: '';
      position: absolute;
      inset: 0;
      background-image:
        linear-gradient(30deg, rgba(255,255,255,0.02) 12%, transparent 12.5%, transparent 87%, rgba(255,255,255,0.02) 87.5%),
        linear-gradient(150deg, rgba(255,255,255,0.02) 12%, transparent 12.5%, transparent 87%, rgba(255,255,255,0.02) 87.5%);
      background-size: 60px 100px;
    }

    .hero-content {
      position: relative;
      z-index: 2;
      padding: 40px 24px;
      animation: fadeInUp 0.8s ease-out;
    }

    .hero-avatar {
      width: 120px;
      height: 120px;
      margin: 0 auto 28px;
      animation: float 6s ease-in-out infinite;
    }

    .hero-avatar svg {
      width: 100%;
      height: 100%;
      filter: drop-shadow(0 0 30px rgba(0,71,187,0.4));
    }

    .hero-name {
      font-size: 3rem;
      font-weight: 800;
      letter-spacing: -0.02em;
      line-height: 1.1;
      margin: 0 0 16px;
      background: linear-gradient(135deg, #ffffff 0%, #94a3b8 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }

    .hero-title {
      font-size: 1.15rem;
      color: var(--accent);
      margin: 0 0 20px;
      font-weight: 500;
      min-height: 1.6em;
    }

    .cursor {
      color: var(--accent);
      font-weight: 300;
      animation: none;
    }

    .cursor.blink {
      animation: blink 1s step-end infinite;
    }

    .hero-location {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
      color: var(--text-secondary);
      font-size: 0.9rem;
      margin-bottom: 28px;
    }

    .hero-links {
      display: flex;
      flex-wrap: wrap;
      justify-content: center;
      gap: 12px;
      margin-bottom: 48px;
    }

    .social-btn {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      padding: 10px 20px;
      border-radius: 50px;
      background: rgba(255,255,255,0.08);
      color: #fff;
      text-decoration: none;
      font-size: 0.85rem;
      font-weight: 500;
      border: 1px solid rgba(255,255,255,0.12);
      transition: all 0.3s ease;
      backdrop-filter: blur(8px);
    }

    .social-btn:hover {
      background: rgba(0,71,187,0.3);
      border-color: var(--blue);
      transform: translateY(-2px);
      box-shadow: 0 4px 20px rgba(0,71,187,0.3);
    }

    .social-btn svg {
      flex-shrink: 0;
    }

    .scroll-indicator {
      animation: bounce 2s ease-in-out infinite;
      color: var(--text-muted);
    }

    /* ========== Section Base ========== */
    .section {
      padding: 80px 0;
    }

    .section:nth-child(even) {
      background: var(--surface);
    }

    .section-title {
      font-size: 2rem;
      font-weight: 800;
      letter-spacing: -0.02em;
      margin: 0 0 12px;
      position: relative;
      display: inline-block;
    }

    .section-title::after {
      content: '';
      display: block;
      width: 48px;
      height: 3px;
      background: linear-gradient(90deg, var(--blue), var(--accent));
      border-radius: 2px;
      margin-top: 8px;
    }

    .section-subtitle {
      color: var(--text-secondary);
      font-size: 0.95rem;
      margin: 8px 0 40px;
    }

    /* ========== About Section ========== */
    .about-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 48px;
      margin-top: 40px;
      align-items: start;
    }

    .about-text p {
      color: var(--text-secondary);
      line-height: 1.75;
      margin: 0 0 16px;
      font-size: 0.95rem;
    }

    .about-text strong {
      color: var(--text);
      font-weight: 600;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .stat-card {
      background: var(--surface-light);
      border: 1px solid var(--border);
      border-radius: var(--radius-sm);
      padding: 24px 20px;
      text-align: center;
      transition: all 0.3s ease;
    }

    .stat-card:hover {
      border-color: var(--blue);
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(0,71,187,0.15);
    }

    .counter-value {
      font-size: 2.5rem;
      font-weight: 800;
      color: var(--blue-light);
      line-height: 1;
    }

    .counter-suffix {
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--blue-light);
    }

    .stat-label {
      display: block;
      font-size: 0.8rem;
      color: var(--text-muted);
      margin-top: 6px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      font-weight: 600;
    }

    /* ========== Tech Stack Section ========== */
    .tech-categories {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
      gap: 24px;
      margin-top: 40px;
    }

    .tech-category {
      background: var(--surface-light);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      padding: 24px;
      transition: all 0.3s ease;
    }

    .tech-category:hover {
      border-color: var(--blue);
      box-shadow: 0 4px 16px rgba(0,71,187,0.1);
    }

    .tech-category-title {
      font-size: 0.85rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: var(--text-secondary);
      margin: 0 0 16px;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .tech-icon {
      display: inline-flex;
      color: var(--blue-light);
    }

    .tech-grid {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .tech-badge {
      display: inline-block;
      padding: 6px 14px;
      border-radius: 20px;
      font-size: 0.8rem;
      font-weight: 600;
      border: 1px solid;
      background: rgba(0,71,187,0.06);
      transition: all 0.2s ease;
    }

    .tech-badge:hover {
      transform: translateY(-2px);
      background: rgba(0,71,187,0.12);
    }

    /* ========== Experience Timeline ========== */
    .timeline {
      position: relative;
      margin-top: 40px;
      padding-left: 32px;
    }

    .timeline::before {
      content: '';
      position: absolute;
      left: 7px;
      top: 8px;
      bottom: 8px;
      width: 2px;
      background: linear-gradient(180deg, var(--blue), var(--accent), transparent);
    }

    .timeline-item {
      position: relative;
      margin-bottom: 32px;
    }

    .timeline-item:last-child {
      margin-bottom: 0;
    }

    .timeline-dot {
      position: absolute;
      left: -32px;
      top: 8px;
      width: 16px;
      height: 16px;
      border-radius: 50%;
      background: var(--surface);
      border: 3px solid var(--blue);
      z-index: 1;
    }

    .timeline-dot.current {
      background: var(--blue);
      box-shadow: 0 0 0 4px rgba(0,71,187,0.3);
    }

    .timeline-content {
      background: var(--surface-light);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      padding: 24px;
      transition: all 0.3s ease;
    }

    .timeline-content:hover {
      border-color: var(--blue);
      box-shadow: 0 4px 16px rgba(0,71,187,0.1);
    }

    .timeline-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 12px;
      gap: 12px;
    }

    .timeline-company {
      font-size: 1.15rem;
      font-weight: 700;
      margin: 0;
      color: var(--text);
    }

    .timeline-role {
      font-size: 0.9rem;
      color: var(--blue-light);
      margin: 2px 0 0;
      font-weight: 500;
    }

    .timeline-period {
      font-size: 0.8rem;
      color: var(--text-muted);
      white-space: nowrap;
      text-align: right;
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 4px;
    }

    .current-badge {
      display: inline-block;
      padding: 2px 10px;
      border-radius: 12px;
      background: rgba(0,212,170,0.15);
      color: var(--accent);
      font-size: 0.7rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }

    .timeline-bullets {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .timeline-bullets li {
      position: relative;
      padding-left: 16px;
      margin-bottom: 6px;
      color: var(--text-secondary);
      font-size: 0.88rem;
      line-height: 1.6;
    }

    .timeline-bullets li::before {
      content: '';
      position: absolute;
      left: 0;
      top: 10px;
      width: 5px;
      height: 5px;
      border-radius: 50%;
      background: var(--blue-light);
    }

    /* ========== Projects Section ========== */
    .projects-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 24px;
      margin-top: 40px;
    }

    .project-card {
      border-radius: var(--radius);
      overflow: hidden;
      background: var(--surface-light);
      border: 1px solid var(--border);
      transition: all 0.3s ease;
    }

    .project-card:hover {
      border-color: var(--blue);
      transform: translateY(-4px);
      box-shadow: 0 12px 32px rgba(0,71,187,0.2);
    }

    .project-accent {
      height: 4px;
    }

    .project-body {
      padding: 28px;
    }

    .project-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 8px;
    }

    .project-name {
      font-size: 1.3rem;
      font-weight: 700;
      margin: 0;
    }

    .project-type {
      font-size: 0.7rem;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      font-weight: 700;
      color: var(--accent);
      background: rgba(0,212,170,0.12);
      padding: 4px 10px;
      border-radius: 12px;
      white-space: nowrap;
    }

    .project-subtitle {
      font-size: 0.9rem;
      color: var(--blue-light);
      margin: 0 0 8px;
      font-weight: 500;
    }

    .project-desc {
      font-size: 0.88rem;
      color: var(--text-secondary);
      line-height: 1.6;
      margin: 0 0 20px;
    }

    .project-metrics {
      display: flex;
      gap: 20px;
      margin-bottom: 20px;
      padding: 16px;
      background: rgba(0,71,187,0.06);
      border-radius: var(--radius-sm);
    }

    .metric {
      text-align: center;
      flex: 1;
    }

    .metric-value {
      display: block;
      font-size: 1.4rem;
      font-weight: 800;
      color: var(--blue-light);
    }

    .metric-label {
      display: block;
      font-size: 0.7rem;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.04em;
      margin-top: 2px;
    }

    .project-tech {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
      margin-bottom: 20px;
    }

    .project-tech-badge {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 6px;
      font-size: 0.72rem;
      font-weight: 600;
      background: rgba(0,71,187,0.1);
      color: var(--blue-light);
      border: 1px solid rgba(0,71,187,0.15);
    }

    .project-link {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      color: var(--accent);
      text-decoration: none;
      font-size: 0.88rem;
      font-weight: 600;
      transition: all 0.2s ease;
    }

    .project-link:hover {
      color: #fff;
      gap: 10px;
    }

    .ecosystem-badge {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
      margin-top: 24px;
      padding: 14px 24px;
      background: linear-gradient(135deg, rgba(0,71,187,0.1), rgba(0,212,170,0.1));
      border: 1px solid rgba(0,212,170,0.2);
      border-radius: var(--radius-sm);
      color: var(--accent);
      font-size: 0.88rem;
      font-weight: 500;
      text-align: center;
    }

    /* ========== Education Section ========== */
    .edu-grid {
      margin-top: 40px;
      display: grid;
      gap: 12px;
    }

    .edu-card {
      display: flex;
      align-items: center;
      gap: 14px;
      padding: 16px 20px;
      background: var(--surface-light);
      border: 1px solid var(--border);
      border-radius: var(--radius-sm);
      transition: all 0.3s ease;
    }

    .edu-card:hover {
      border-color: var(--blue);
      transform: translateX(4px);
    }

    .edu-card svg {
      color: var(--blue-light);
      flex-shrink: 0;
    }

    .edu-degree {
      font-size: 0.9rem;
      font-weight: 600;
      margin: 0;
      color: var(--text);
    }

    .edu-institution {
      font-size: 0.8rem;
      color: var(--text-muted);
      margin: 2px 0 0;
    }

    .languages {
      margin-top: 40px;
    }

    .languages h3 {
      font-size: 1rem;
      font-weight: 700;
      margin: 0 0 16px;
      color: var(--text);
    }

    .lang-badges {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
    }

    .lang-badge {
      padding: 8px 18px;
      background: var(--surface-light);
      border: 1px solid var(--border);
      border-radius: 20px;
      font-size: 0.88rem;
      font-weight: 500;
      color: var(--text);
    }

    .lang-badge small {
      color: var(--text-muted);
      font-weight: 400;
    }

    /* ========== Contact Section ========== */
    .contact {
      text-align: center;
      background: linear-gradient(180deg, var(--surface) 0%, var(--bg) 100%) !important;
    }

    .contact .section-title::after {
      margin-left: auto;
      margin-right: auto;
    }

    .contact-text {
      color: var(--text-secondary);
      font-size: 1.05rem;
      margin: 16px auto 36px;
      max-width: 500px;
      line-height: 1.6;
    }

    .contact-links {
      display: flex;
      justify-content: center;
      gap: 16px;
      flex-wrap: wrap;
    }

    .contact-btn {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      padding: 14px 28px;
      border-radius: 50px;
      font-size: 0.9rem;
      font-weight: 600;
      text-decoration: none;
      transition: all 0.3s ease;
      background: var(--surface-light);
      color: var(--text);
      border: 1px solid var(--border);
    }

    .contact-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 24px rgba(0,71,187,0.2);
      border-color: var(--blue);
    }

    .contact-btn.primary {
      background: linear-gradient(135deg, var(--blue), var(--blue-dark));
      color: #fff;
      border-color: transparent;
    }

    .contact-btn.primary:hover {
      box-shadow: 0 8px 24px rgba(0,71,187,0.4);
    }

    /* ========== Back Button ========== */
    .back-section {
      text-align: center;
      padding: 32px;
    }

    .back-btn {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      padding: 12px 28px;
      border-radius: 50px;
      background: var(--surface);
      border: 1px solid var(--border);
      color: var(--text-secondary);
      text-decoration: none;
      font-size: 0.88rem;
      font-weight: 500;
      transition: all 0.3s ease;
    }

    .back-btn:hover {
      color: var(--text);
      border-color: var(--blue);
      transform: translateX(-4px);
    }

    /* ========== Animations ========== */
    @keyframes fadeInUp {
      from { opacity: 0; transform: translateY(40px); }
      to { opacity: 1; transform: translateY(0); }
    }

    @keyframes float {
      0%, 100% { transform: translateY(0); }
      50% { transform: translateY(-10px); }
    }

    @keyframes bounce {
      0%, 20%, 50%, 80%, 100% { transform: translateY(0); }
      40% { transform: translateY(-8px); }
      60% { transform: translateY(-4px); }
    }

    @keyframes blink {
      0%, 100% { opacity: 1; }
      50% { opacity: 0; }
    }

    /* Scroll Reveal */
    .reveal {
      opacity: 0;
      transform: translateY(30px);
      transition: opacity 0.6s ease, transform 0.6s ease;
    }

    .reveal.revealed {
      opacity: 1;
      transform: translateY(0);
    }

    /* ========== Responsive ========== */
    @media (max-width: 900px) {
      .hero-name {
        font-size: 2.2rem;
      }

      .hero-title {
        font-size: 1rem;
      }

      .about-grid {
        grid-template-columns: 1fr;
        gap: 32px;
      }

      .projects-grid {
        grid-template-columns: 1fr;
      }

      .tech-categories {
        grid-template-columns: 1fr;
      }

      .section {
        padding: 60px 0;
      }
    }

    @media (max-width: 600px) {
      .hero {
        min-height: auto;
        padding: 80px 0 60px;
      }

      .hero-name {
        font-size: 1.8rem;
      }

      .hero-title {
        font-size: 0.85rem;
      }

      .hero-links {
        flex-direction: column;
        align-items: center;
      }

      .social-btn {
        width: 100%;
        max-width: 280px;
        justify-content: center;
      }

      .section {
        padding: 48px 0;
      }

      .section-title {
        font-size: 1.6rem;
      }

      .stats-grid {
        grid-template-columns: 1fr 1fr;
        gap: 12px;
      }

      .stat-card {
        padding: 16px 12px;
      }

      .counter-value {
        font-size: 2rem;
      }

      .timeline {
        padding-left: 24px;
      }

      .timeline-header {
        flex-direction: column;
      }

      .timeline-period {
        align-items: flex-start;
        flex-direction: row;
        gap: 8px;
      }

      .project-metrics {
        flex-wrap: wrap;
        gap: 12px;
      }

      .metric {
        flex: 0 0 calc(50% - 6px);
      }

      .contact-links {
        flex-direction: column;
        align-items: center;
      }

      .contact-btn {
        width: 100%;
        max-width: 280px;
        justify-content: center;
      }

      .container {
        padding: 0 16px;
      }
    }
  `]
})
export class PortfolioComponent implements AfterViewInit, OnDestroy {
  typedText = '';
  typingDone = false;
  private fullTitle = 'Senior Full Stack Engineer (.NET) | Sistemas Distribuídos | Arquitetura Backend';
  private typeTimer: any;
  private observer?: IntersectionObserver;
  private statsObserver?: IntersectionObserver;

  stats = [
    { value: 7, suffix: '+', label: 'Anos de Experiência' },
    { value: 2, suffix: '', label: 'Sistemas Completos' },
    { value: 25, suffix: '', label: 'Containers Docker' },
    { value: 389, suffix: '', label: 'Testes (0 falhas)' }
  ];

  techCategories: TechCategory[] = [
    {
      title: 'Backend',
      icon: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="3" width="20" height="14" rx="2"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/></svg>',
      items: [
        { name: '.NET 8', color: '#512BD4' },
        { name: 'ASP.NET Core', color: '#512BD4' },
        { name: 'C#', color: '#68217A' },
        { name: 'EF Core', color: '#512BD4' },
        { name: 'Dapper', color: '#2196F3' },
        { name: 'Minimal APIs', color: '#512BD4' },
        { name: 'Web API', color: '#512BD4' },
        { name: 'LINQ', color: '#68217A' }
      ]
    },
    {
      title: 'Arquitetura',
      icon: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="12 2 2 7 12 12 22 7 12 2"/><polyline points="2 17 12 22 22 17"/><polyline points="2 12 12 17 22 12"/></svg>',
      items: [
        { name: 'DDD', color: '#00D4AA' },
        { name: 'Clean Architecture', color: '#00D4AA' },
        { name: 'CQRS', color: '#00B894' },
        { name: 'Event-Driven', color: '#00B894' },
        { name: 'Microservices', color: '#0047BB' },
        { name: 'REST', color: '#3375d6' },
        { name: 'gRPC', color: '#244c5a' }
      ]
    },
    {
      title: 'Mensageria',
      icon: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>',
      items: [
        { name: 'RabbitMQ', color: '#FF6600' },
        { name: 'Kafka', color: '#231F20' }
      ]
    },
    {
      title: 'Bancos de Dados',
      icon: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><ellipse cx="12" cy="5" rx="9" ry="3"/><path d="M21 12c0 1.66-4 3-9 3s-9-1.34-9-3"/><path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5"/></svg>',
      items: [
        { name: 'SQL Server', color: '#CC2927' },
        { name: 'PostgreSQL', color: '#336791' },
        { name: 'MySQL', color: '#4479A1' },
        { name: 'MongoDB', color: '#47A248' },
        { name: 'Redis', color: '#DC382D' }
      ]
    },
    {
      title: 'Cloud & DevOps',
      icon: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z"/></svg>',
      items: [
        { name: 'Azure', color: '#0078D4' },
        { name: 'AWS', color: '#FF9900' },
        { name: 'Docker', color: '#2496ED' },
        { name: 'Kubernetes', color: '#326CE5' },
        { name: 'GitHub Actions', color: '#2088FF' },
        { name: 'GitLab CI/CD', color: '#FC6D26' },
        { name: 'Azure DevOps', color: '#0078D4' }
      ]
    },
    {
      title: 'Frontend',
      icon: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="16 18 22 12 16 6"/><polyline points="8 6 2 12 8 18"/></svg>',
      items: [
        { name: 'TypeScript', color: '#3178C6' },
        { name: 'React', color: '#61DAFB' },
        { name: 'Angular', color: '#DD0031' },
        { name: 'Vue.js', color: '#4FC08D' },
        { name: 'Tailwind', color: '#06B6D4' },
        { name: 'Bootstrap', color: '#7952B3' }
      ]
    }
  ];

  experiences: Experience[] = [
    {
      company: 'Qintess',
      role: 'Senior Software Engineer .NET',
      period: 'Mar 2025 - Atual',
      current: true,
      bullets: [
        'Liderança técnica na arquitetura e evolução de sistemas críticos governamentais e de saúde',
        'Modelagem DDD, APIs resilientes, workflows de mensageria assíncrona com RabbitMQ',
        'Definição de práticas arquiteturais, pipelines CI/CD, padrões de engenharia backend',
        'Redução de acoplamento sistêmico via refatorações arquiteturais estratégicas',
        'Design de serviços distribuídos escaláveis com SLA exigentes'
      ]
    },
    {
      company: 'G4F Soluções Corporativas',
      role: 'Senior Software Engineer .NET',
      period: 'Jun 2022 - Mar 2025',
      current: false,
      bullets: [
        'Liderança técnica em projetos enterprise, integração entre sistemas legados e modernos',
        'Definição de padrões arquiteturais para toda a organização',
        'Pipelines CI/CD, redução de tempo de deploy',
        'Mentoria de engenheiros juniores e plenos (code reviews, pair programming)',
        'APIs RESTful e microsserviços com .NET Core'
      ]
    },
    {
      company: 'Afixcode',
      role: 'Software Engineer .NET',
      period: 'Set 2021 - Jun 2022',
      current: false,
      bullets: [
        'APIs REST robustas e sistemas SaaS com .NET Core',
        'Arquiteturas orientadas a eventos com RabbitMQ',
        'Frontend com React, TypeScript, Redux',
        'Bancos SQL Server, PostgreSQL, MongoDB com EF Core e Dapper'
      ]
    },
    {
      company: 'Neotriad S/A',
      role: 'Software Engineer .NET',
      period: 'Nov 2019 - Jun 2022',
      current: false,
      bullets: [
        'Sistemas backend corporativos com foco em performance',
        'ASP.NET MVC e Web API',
        'Otimização de queries e modelagem de banco',
        'Testes unitários e de integração'
      ]
    },
    {
      company: 'IBGE',
      role: 'Agente de Pesquisa e Mapeamento',
      period: 'Nov 2016 - Nov 2019',
      current: false,
      bullets: [
        'Projetos estatísticos de grande escala',
        'Coleta, validação e análise de dados censitários'
      ]
    }
  ];

  projects: Project[] = [
    {
      name: 'KLL Platform',
      subtitle: 'AUREA Maison',
      type: 'E-commerce',
      description: 'E-commerce de joias de luxo com 15 microsserviços, Clean Architecture, CQRS, mensageria com Kafka e RabbitMQ, gateway API e painel administrativo completo.',
      tech: ['.NET 8', 'React', 'TypeScript', 'PostgreSQL', 'Redis', 'RabbitMQ', 'Kafka', 'Docker', 'CQRS', 'MediatR'],
      metrics: [
        { value: '15', label: 'Containers' },
        { value: '244', label: 'Testes' },
        { value: '5', label: 'Microsserviços' }
      ],
      github: 'https://github.com/KlistenesLima/kll-platform',
      accent: 'linear-gradient(90deg, #c9a962, #b08942)'
    },
    {
      name: 'KRT Bank',
      subtitle: 'Plataforma Bancária Digital',
      type: 'FinTech',
      description: 'Plataforma bancária digital completa com PIX, Boleto, Cartão, KYC, extrato e investimentos. Arquitetura DDD com microsserviços e autenticação Keycloak.',
      tech: ['.NET 8', 'Angular 17', 'TypeScript', 'PostgreSQL', 'Redis', 'RabbitMQ', 'Kafka', 'Docker', 'DDD', 'Keycloak'],
      metrics: [
        { value: '10', label: 'Containers' },
        { value: '145', label: 'Testes' },
        { value: '3', label: 'Microsserviços' }
      ],
      github: 'https://github.com/KlistenesLima/krt-bank',
      accent: 'linear-gradient(90deg, #0047BB, #002a70)'
    }
  ];

  education: Education[] = [
    { degree: 'Pós-Graduação em Desenvolvimento de Aplicações .NET' },
    { degree: 'Pós-Graduação em Engenharia de Software' },
    { degree: 'Pós-Graduação em Business Intelligence, Big Data e IA' },
    { degree: 'Pós-Graduação em Administração de Banco de Dados' },
    { degree: 'Pós-Graduação em Ciência de Dados' },
    { degree: 'Pós-Graduação em Desenvolvimento Web' },
    { degree: 'Pós-Graduação em Algoritmos e Estruturas de Dados' },
    { degree: 'Pós-Graduação em Product Management' },
    { degree: 'Tecnólogo em Análise e Desenvolvimento de Sistemas', institution: 'UNIP' }
  ];

  constructor(private cdr: ChangeDetectorRef) {}

  ngAfterViewInit() {
    this.startTyping();
    this.initScrollReveal();
    this.initCounterAnimation();
  }

  private startTyping() {
    let i = 0;
    const type = () => {
      if (i < this.fullTitle.length) {
        this.typedText += this.fullTitle.charAt(i);
        i++;
        this.cdr.detectChanges();
        this.typeTimer = setTimeout(type, 35);
      } else {
        this.typingDone = true;
        this.cdr.detectChanges();
      }
    };
    this.typeTimer = setTimeout(type, 600);
  }

  private initScrollReveal() {
    this.observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('revealed');
            this.observer?.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.1, rootMargin: '0px 0px -40px 0px' }
    );

    const revealElements = document.querySelectorAll('.reveal');
    revealElements.forEach((el) => this.observer?.observe(el));
  }

  private initCounterAnimation() {
    this.statsObserver = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            this.animateCounters();
            this.statsObserver?.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.3 }
    );

    const statsEl = document.querySelector('.stats-grid');
    if (statsEl) this.statsObserver.observe(statsEl);
  }

  private animateCounters() {
    const counters = document.querySelectorAll('.counter-value');
    counters.forEach((counter) => {
      const target = parseInt(counter.getAttribute('data-target') || '0', 10);
      if (target === 0) return;
      const duration = 2000;
      const startTime = performance.now();

      const animate = (currentTime: number) => {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3);
        counter.textContent = Math.floor(eased * target).toString();
        if (progress < 1) requestAnimationFrame(animate);
      };

      requestAnimationFrame(animate);
    });
  }

  ngOnDestroy() {
    clearTimeout(this.typeTimer);
    this.observer?.disconnect();
    this.statsObserver?.disconnect();
  }
}
