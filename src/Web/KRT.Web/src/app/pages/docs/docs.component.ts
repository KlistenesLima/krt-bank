import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface DocSection {
  id: string;
  label: string;
}

interface Endpoint {
  method: string;
  path: string;
  desc: string;
}

@Component({
  selector: 'app-docs',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="docs-page" [class.visible]="visible">
      <!-- Hero -->
      <section class="hero">
        <div class="hero-bg"></div>
        <div class="hero-content">
          <h1>Documentação <span class="accent">KRT Bank</span></h1>
          <p class="subtitle">Manual de uso do sistema</p>
          <button class="btn btn-primary" (click)="downloadPdf()" [disabled]="generating">
            {{ generating ? 'Gerando PDF...' : 'Download Manual (PDF)' }}
          </button>
        </div>
      </section>

      <!-- Content -->
      <div class="docs-layout">
        <!-- Mobile menu toggle -->
        <button class="mobile-toggle" (click)="mobileMenuOpen = !mobileMenuOpen">
          <span>{{ getActiveLabel() }}</span>
          <span class="toggle-arrow" [class.open]="mobileMenuOpen">&#9662;</span>
        </button>

        <!-- Sidebar -->
        <nav class="sidebar" [class.open]="mobileMenuOpen">
          <button *ngFor="let s of sections"
                  [class.active]="active === s.id"
                  (click)="active = s.id; mobileMenuOpen = false"
                  class="nav-item">
            {{ s.label }}
          </button>
        </nav>

        <!-- Content -->
        <div class="content" id="docs-content">
          <!-- Visão Geral -->
          <ng-container *ngIf="active === 'overview'">
            <h2>Visão Geral</h2>
            <p>O <strong>KRT Bank</strong> é uma plataforma bancária digital construída como case de portfólio.
            Demonstra uma arquitetura completa de microsserviços com frontend Angular 17 e backend .NET 8.</p>
            <p>O sistema oferece abertura de conta, dashboard com saldo, transferência PIX, pagamento de boletos,
            cartão de crédito virtual, extrato detalhado e gerenciamento de chaves PIX.</p>
            <div class="info-box">
              <strong>Importante:</strong> Este é um projeto de demonstração. Nenhuma transação é real e nenhum dado financeiro é processado.
            </div>
          </ng-container>

          <!-- Abertura de Conta -->
          <ng-container *ngIf="active === 'account'">
            <h2>Abertura de Conta</h2>
            <div class="step" *ngFor="let step of accountSteps; let i = index">
              <span class="step-num">{{ i + 1 }}</span>
              <div>
                <h4>{{ step.title }}</h4>
                <p>{{ step.text }}</p>
              </div>
            </div>
          </ng-container>

          <!-- Dashboard -->
          <ng-container *ngIf="active === 'dashboard'">
            <h2>Dashboard</h2>
            <p>O dashboard é a tela principal após o login. Apresenta:</p>
            <ul>
              <li>Saldo atual da conta</li>
              <li>Atalhos rápidos para PIX, Boleto e Cartões</li>
              <li>Últimas transações realizadas</li>
              <li>Gráfico de movimentação mensal</li>
            </ul>
          </ng-container>

          <!-- PIX -->
          <ng-container *ngIf="active === 'pix'">
            <h2>Transferência PIX</h2>
            <p>O KRT Bank suporta 4 tipos de chave PIX:</p>
            <div class="step" *ngFor="let step of pixSteps; let i = index">
              <span class="step-num">{{ i + 1 }}</span>
              <div>
                <h4>{{ step.title }}</h4>
                <p>{{ step.text }}</p>
              </div>
            </div>
          </ng-container>

          <!-- PIX Copia e Cola -->
          <ng-container *ngIf="active === 'pix-copy'">
            <h2>PIX Copia e Cola</h2>
            <div class="step" *ngFor="let step of pixCopySteps; let i = index">
              <span class="step-num">{{ i + 1 }}</span>
              <div>
                <h4>{{ step.title }}</h4>
                <p>{{ step.text }}</p>
              </div>
            </div>
          </ng-container>

          <!-- Boleto -->
          <ng-container *ngIf="active === 'boleto'">
            <h2>Pagar Boleto</h2>
            <div class="step" *ngFor="let step of boletoSteps; let i = index">
              <span class="step-num">{{ i + 1 }}</span>
              <div>
                <h4>{{ step.title }}</h4>
                <p>{{ step.text }}</p>
              </div>
            </div>
          </ng-container>

          <!-- Cartões -->
          <ng-container *ngIf="active === 'cards'">
            <h2>Cartões</h2>
            <p>O KRT Bank oferece cartão de crédito virtual com as seguintes funcionalidades:</p>
            <ul>
              <li>Cartão virtual com número, validade e CVV</li>
              <li>Limite de crédito gerenciado</li>
              <li>Compras debitadas automaticamente do limite</li>
              <li>Visualização dos dados do cartão na tela</li>
            </ul>
            <div class="info-box">
              Use o cartão virtual para pagar no AUREA Maison (KLL Platform). O valor é debitado do limite.
            </div>
          </ng-container>

          <!-- Extrato -->
          <ng-container *ngIf="active === 'statement'">
            <h2>Extrato</h2>
            <p>O extrato mostra todas as transações da conta com:</p>
            <ul>
              <li>Filtro por data (período personalizado)</li>
              <li>Filtro por tipo (crédito/débito)</li>
              <li>Descrição detalhada de cada transação</li>
              <li>Saldo após cada transação</li>
              <li>Download de comprovante PDF</li>
            </ul>
          </ng-container>

          <!-- Chaves PIX -->
          <ng-container *ngIf="active === 'pix-keys'">
            <h2>Chaves PIX</h2>
            <p>Gerencie suas chaves PIX:</p>
            <div class="key-grid">
              <div class="key-card" *ngFor="let key of pixKeyTypes">
                <h4>{{ key.type }}</h4>
                <p>{{ key.desc }}</p>
              </div>
            </div>
            <p style="margin-top: 16px;">Acesse PIX > Minhas Chaves para registrar, visualizar e remover chaves.</p>
          </ng-container>

          <!-- API Endpoints -->
          <ng-container *ngIf="active === 'api'">
            <h2>API Endpoints</h2>
            <p>Principais endpoints disponíveis no KRT Gateway:</p>
            <div class="endpoint" *ngFor="let ep of endpoints">
              <span class="method" [class.get]="ep.method === 'GET'" [class.post]="ep.method === 'POST'">{{ ep.method }}</span>
              <code>{{ ep.path }}</code>
              <span class="ep-desc">{{ ep.desc }}</span>
            </div>
          </ng-container>

          <!-- Integração E-commerce -->
          <ng-container *ngIf="active === 'integration'">
            <h2>Integração E-commerce</h2>
            <p>O KLL Platform (AUREA Maison) utiliza o KRT Bank como processador de pagamentos.</p>
            <h3>Fluxo de Pagamento</h3>
            <div class="step" *ngFor="let step of integrationSteps; let i = index">
              <span class="step-num">{{ i + 1 }}</span>
              <div>
                <h4>{{ step.from }} → {{ step.to }}</h4>
                <p>{{ step.desc }}</p>
              </div>
            </div>
            <h3 style="margin-top: 24px;">Autenticação</h3>
            <p>A integração utiliza API Key Authentication. O KLL Platform envia o header <code>X-Api-Key</code>
            em cada requisição. O KRT Gateway valida a chave antes de processar.</p>
            <h3 style="margin-top: 24px;">Serviço Principal</h3>
            <p><code>ChargePaymentService</code> — orquestra o fluxo débito → crédito → extrato para cada pagamento recebido do e-commerce.</p>
          </ng-container>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .docs-page { opacity: 0; transition: opacity 0.7s ease; }
    .docs-page.visible { opacity: 1; }

    .hero { position: relative; padding: 60px 16px 40px; text-align: center; overflow: hidden; }
    .hero-bg { position: absolute; inset: 0; background: linear-gradient(180deg, rgba(0,47,98,0.3) 0%, transparent 100%); pointer-events: none; }
    .hero-content { position: relative; max-width: 700px; margin: 0 auto; }
    h1 { font-size: 32px; font-weight: 800; color: #fff; margin: 0 0 8px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .accent { color: #4d9fff; }
    .subtitle { color: #94a3b8; font-size: 16px; margin: 0 0 20px; }
    .btn { padding: 12px 24px; border-radius: 12px; font-size: 14px; font-weight: 600; text-decoration: none; transition: all 0.3s; font-family: 'Plus Jakarta Sans', sans-serif; cursor: pointer; border: none; }
    .btn-primary { background: #0047BB; color: #fff; }
    .btn-primary:hover { background: #0055DD; }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

    .docs-layout { max-width: 1100px; margin: 0 auto; padding: 0 16px 80px; display: flex; gap: 32px; }

    .mobile-toggle { display: none; width: 100%; padding: 12px 16px; border-radius: 12px; background: rgba(26,35,50,0.8); border: 1px solid rgba(0,71,187,0.2); color: #4d9fff; font-size: 14px; font-family: 'Plus Jakarta Sans', sans-serif; font-weight: 500; cursor: pointer; justify-content: space-between; align-items: center; }
    .toggle-arrow { transition: transform 0.3s; }
    .toggle-arrow.open { transform: rotate(180deg); }

    .sidebar { width: 240px; flex-shrink: 0; position: sticky; top: 56px; align-self: flex-start; display: flex; flex-direction: column; gap: 4px; }
    .nav-item { text-align: left; padding: 10px 16px; border-radius: 8px; font-size: 13px; font-family: 'Plus Jakarta Sans', sans-serif; color: #94a3b8; background: transparent; border: none; cursor: pointer; transition: all 0.2s; }
    .nav-item:hover { color: #fff; background: rgba(26,35,50,0.5); }
    .nav-item.active { color: #4d9fff; background: rgba(0,71,187,0.15); font-weight: 600; }

    .content { flex: 1; min-width: 0; }
    .content h2 { font-size: 24px; font-weight: 700; color: #fff; margin: 0 0 16px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .content h3 { font-size: 18px; font-weight: 600; color: #fff; margin: 0 0 12px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .content p { color: #94a3b8; font-size: 14px; line-height: 1.7; margin: 0 0 12px; }
    .content strong { color: #4d9fff; }
    .content code { color: #4d9fff; background: rgba(0,71,187,0.1); padding: 2px 8px; border-radius: 4px; font-size: 13px; }
    .content ul { list-style: none; padding: 0; margin: 0 0 16px; }
    .content li { color: #94a3b8; font-size: 14px; padding: 6px 0 6px 20px; position: relative; }
    .content li::before { content: ''; position: absolute; left: 0; top: 14px; width: 6px; height: 6px; border-radius: 50%; background: #4d9fff; }

    .step { display: flex; align-items: flex-start; gap: 16px; padding: 16px; border-radius: 12px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); margin-bottom: 12px; }
    .step-num { background: rgba(0,71,187,0.2); color: #4d9fff; font-size: 13px; font-weight: 700; width: 32px; height: 32px; display: flex; align-items: center; justify-content: center; border-radius: 50%; flex-shrink: 0; }
    .step h4 { color: #fff; font-size: 14px; font-weight: 600; margin: 0 0 4px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .step p { margin: 0; }

    .info-box { padding: 16px; border-radius: 12px; background: rgba(0,71,187,0.08); border: 1px solid rgba(0,71,187,0.2); margin-top: 16px; }
    .info-box strong { color: #4d9fff; }

    .key-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px; }
    .key-card { padding: 16px; border-radius: 12px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); }
    .key-card h4 { color: #4d9fff; font-size: 13px; font-weight: 600; margin: 0 0 4px; }
    .key-card p { margin: 0; font-size: 13px; }

    .endpoint { display: flex; align-items: center; gap: 12px; padding: 12px; border-radius: 12px; background: rgba(26,35,50,0.6); border: 1px solid rgba(0,71,187,0.15); margin-bottom: 8px; flex-wrap: wrap; }
    .method { padding: 2px 10px; border-radius: 4px; font-size: 11px; font-weight: 700; font-family: monospace; flex-shrink: 0; }
    .method.get { background: rgba(0,200,83,0.2); color: #00C853; }
    .method.post { background: rgba(0,71,187,0.2); color: #4d9fff; }
    .endpoint code { flex: 1; background: transparent; padding: 0; color: #fff; font-size: 13px; }
    .ep-desc { color: #94a3b8; font-size: 13px; }

    @media (max-width: 768px) {
      .docs-layout { flex-direction: column; gap: 16px; }
      .mobile-toggle { display: flex; }
      .sidebar { position: static; width: 100%; display: none; }
      .sidebar.open { display: flex; }
      h1 { font-size: 24px; }
      .key-grid { grid-template-columns: 1fr; }
      .endpoint { flex-direction: column; align-items: flex-start; gap: 4px; }
      .ep-desc { display: block; }
    }
  `]
})
export class DocsComponent implements OnInit {
  visible = false;
  active = 'overview';
  mobileMenuOpen = false;
  generating = false;

  sections: DocSection[] = [
    { id: 'overview', label: 'Visão Geral' },
    { id: 'account', label: 'Abertura de Conta' },
    { id: 'dashboard', label: 'Dashboard' },
    { id: 'pix', label: 'Transferência PIX' },
    { id: 'pix-copy', label: 'PIX Copia e Cola' },
    { id: 'boleto', label: 'Pagar Boleto' },
    { id: 'cards', label: 'Cartões' },
    { id: 'statement', label: 'Extrato' },
    { id: 'pix-keys', label: 'Chaves PIX' },
    { id: 'api', label: 'API Endpoints' },
    { id: 'integration', label: 'Integração E-commerce' },
  ];

  accountSteps = [
    { title: 'Login via Keycloak', text: 'Acesse a tela de login e clique em "Criar conta" para se registrar via Keycloak (OAuth2/OIDC).' },
    { title: 'Registro', text: 'Preencha nome, e-mail e senha no formulário de registro do Keycloak.' },
    { title: 'Dados Pessoais', text: 'Após o registro, preencha seus dados pessoais (CPF, telefone, data de nascimento).' },
    { title: 'Conta Criada', text: 'A conta bancária é criada automaticamente com saldo inicial. Você é redirecionado ao dashboard.' },
  ];

  pixSteps = [
    { title: 'Escolher tipo de chave', text: 'Selecione entre CPF, Email, Celular ou Chave Aleatória.' },
    { title: 'Resolver chave', text: 'O sistema busca a conta associada à chave e exibe os dados do destinatário.' },
    { title: 'Informar valor', text: 'Digite o valor da transferência e opcionalmente uma descrição.' },
    { title: 'Confirmar transferência', text: 'Revise os dados e confirme. A transferência é processada atomicamente.' },
    { title: 'Comprovante PDF', text: 'Após a confirmação, um comprovante PDF é gerado automaticamente.' },
  ];

  pixCopySteps = [
    { title: 'Acessar PIX Copia e Cola', text: 'Na tela PIX, selecione a 5ª opção "Copia e Cola".' },
    { title: 'Colar código EMV', text: 'Cole o código EMV (BRCode) recebido do recebedor.' },
    { title: 'Parse automático', text: 'O sistema faz o parse do BRCode e exibe os dados da cobrança.' },
    { title: 'Confirmar pagamento', text: 'Revise o valor e destinatário, e confirme o pagamento.' },
  ];

  boletoSteps = [
    { title: 'Digitar linha digitável', text: 'Na tela de Boleto, digite ou cole a linha digitável do boleto.' },
    { title: 'Buscar dados', text: 'O sistema busca os dados do boleto (beneficiário, valor, vencimento).' },
    { title: 'Confirmar pagamento', text: 'Revise os dados e confirme o pagamento. O débito é automático na conta.' },
  ];

  pixKeyTypes = [
    { type: 'CPF', desc: 'Cadastre seu CPF como chave PIX para receber transferências.' },
    { type: 'Email', desc: 'Use seu e-mail como chave PIX.' },
    { type: 'Celular', desc: 'Registre seu número de celular como chave.' },
    { type: 'Aleatória', desc: 'Gere uma chave aleatória UUID para receber PIX.' },
  ];

  endpoints: Endpoint[] = [
    { method: 'POST', path: '/api/v1/accounts', desc: 'Criar conta' },
    { method: 'POST', path: '/api/v1/auth/login', desc: 'Login' },
    { method: 'POST', path: '/api/v1/pix', desc: 'Transferência PIX P2P' },
    { method: 'POST', path: '/api/v1/pix/charges', desc: 'Criar cobrança PIX' },
    { method: 'POST', path: '/api/v1/pix/charges/{id}/simulate-payment', desc: 'Pagar cobrança' },
    { method: 'POST', path: '/api/v1/boletos/charges', desc: 'Criar cobrança Boleto' },
    { method: 'POST', path: '/api/v1/boletos/charges/find-by-digitable-line', desc: 'Buscar boleto' },
    { method: 'POST', path: '/api/v1/cards/charges', desc: 'Criar cobrança Cartão' },
    { method: 'GET', path: '/api/v1/pix-keys/account/{id}', desc: 'Listar chaves PIX' },
  ];

  integrationSteps = [
    { from: 'KLL Pay Service', to: 'KRT Gateway', desc: 'Envio da cobrança via API Key Authentication.' },
    { from: 'KRT Gateway', to: 'Payments API', desc: 'Processamento do pagamento (PIX, Boleto ou Cartão).' },
    { from: 'Payments API', to: 'Conta Bancária', desc: 'Operação atômica: débito → crédito → registro no extrato.' },
    { from: 'KRT Bank', to: 'KLL Pay', desc: 'Confirmação via webhook/polling do status do pagamento.' },
  ];

  getActiveLabel(): string {
    return this.sections.find(s => s.id === this.active)?.label || 'Menu';
  }

  ngOnInit() {
    window.scrollTo(0, 0);
    setTimeout(() => this.visible = true, 100);
  }

  async downloadPdf() {
    this.generating = true;
    try {
      const html2pdf = (await import('html2pdf.js' as any)).default;
      const content = document.getElementById('docs-content');
      if (!content) return;

      const clone = content.cloneNode(true) as HTMLElement;
      clone.style.cssText = 'color: #1a1a1a; background: #fff; padding: 30px;';
      clone.querySelectorAll('*').forEach(el => {
        const h = el as HTMLElement;
        if (h.style) {
          h.style.color = '#1a1a1a';
          h.style.borderColor = '#ddd';
          h.style.background = h.tagName === 'CODE' ? '#f5f5f5' : 'transparent';
        }
      });
      clone.querySelectorAll('h1, h2, h3, h4').forEach(el => {
        (el as HTMLElement).style.color = '#0047BB';
      });

      const wrapper = document.createElement('div');
      wrapper.style.fontFamily = 'Helvetica, Arial, sans-serif';
      wrapper.innerHTML = '<h1 style="font-size:24px;color:#0047BB;border-bottom:2px solid #0047BB;padding-bottom:10px;">KRT Bank — Manual de Uso</h1>';
      wrapper.appendChild(clone);

      await html2pdf().set({
        margin: [10, 10, 10, 10],
        filename: 'KRT-Bank-Manual.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
      }).from(wrapper).save();
    } catch (err) {
      console.error('Erro ao gerar PDF:', err);
    } finally {
      this.generating = false;
    }
  }
}
