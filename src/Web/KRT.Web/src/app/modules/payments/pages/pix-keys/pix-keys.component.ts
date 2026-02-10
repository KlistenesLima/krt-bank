import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-pix-keys',
  template: `
    <div class="keys-container page-with-nav">
      <header class="page-header">
        <button class="back-btn" (click)="router.navigate(['/dashboard'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Minhas Chaves Pix</h1>
        <div style="width:40px"></div>
      </header>

      <div class="content fade-in">
        <div class="info-banner">
          <mat-icon>info</mat-icon>
          <span>Voce pode cadastrar ate 5 chaves Pix. Toque para copiar.</span>
        </div>

        <div class="section-label">Chaves cadastradas</div>

        <div class="key-card" *ngFor="let key of keys" (click)="copyKey(key)">
          <div class="key-icon" [style.background]="key.color + '12'">
            <mat-icon [style.color]="key.color">{{ key.icon }}</mat-icon>
          </div>
          <div class="key-info">
            <span class="key-type">{{ key.type }}</span>
            <span class="key-value">{{ key.value }}</span>
          </div>
          <div class="key-actions">
            <button class="icon-btn" (click)="copyKey(key); $event.stopPropagation()">
              <mat-icon>content_copy</mat-icon>
            </button>
            <button class="icon-btn danger" (click)="removeKey(key); $event.stopPropagation()">
              <mat-icon>delete_outline</mat-icon>
            </button>
          </div>
        </div>

        <div class="empty-state" *ngIf="keys.length === 0">
          <mat-icon>vpn_key_off</mat-icon>
          <p>Nenhuma chave cadastrada</p>
        </div>

        <button class="add-btn" *ngIf="!showForm && keys.length < 5" (click)="showForm = true">
          <div class="add-icon"><mat-icon>add</mat-icon></div>
          <span>Cadastrar nova chave</span>
          <mat-icon class="chevron">chevron_right</mat-icon>
        </button>

        <div class="form-area fade-in" *ngIf="showForm">
          <div class="section-label">Tipo da chave</div>
          <div class="type-grid">
            <button class="type-btn" *ngFor="let t of types" [class.selected]="newType === t.id" (click)="newType = t.id; newValue = ''">
              <div class="type-icon" [style.background]="t.color + '12'">
                <mat-icon [style.color]="t.color">{{ t.icon }}</mat-icon>
              </div>
              <span>{{ t.label }}</span>
            </button>
          </div>

          <div class="field" *ngIf="newType !== 'random'">
            <label>{{ getNewLabel() }}</label>
            <div class="input-wrap">
              <input type="text" [(ngModel)]="newValue" [placeholder]="getNewPlaceholder()"
                     (input)="maskNewValue($event)" [maxlength]="getNewMaxLen()" autocomplete="off">
              <mat-icon>{{ getNewIcon() }}</mat-icon>
            </div>
          </div>

          <div class="btn-row">
            <button class="btn-secondary" (click)="showForm = false">CANCELAR</button>
            <button class="btn-primary flex-btn" [disabled]="!isNewValid()" (click)="addKey()">CADASTRAR</button>
          </div>
        </div>

        <div class="limit-info">
          <span>{{ keys.length }}/5 chaves cadastradas</span>
          <div class="limit-bar"><div class="limit-fill" [style.width]="(keys.length / 5 * 100) + '%'"></div></div>
        </div>
      </div>
    </div>
    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .keys-container { min-height: 100vh; background: var(--krt-bg); }
    .page-header { display: flex; align-items: center; justify-content: space-between; padding: 16px 20px; background: #fff; border-bottom: 1px solid #F0F0F0; }
    .page-header h1 { font-size: 1.05rem; font-weight: 700; margin: 0; color: #1A1A2E; }
    .back-btn { background: none; border: none; cursor: pointer; padding: 4px; color: #1A1A2E; display: flex; }
    .content { padding: 24px 20px; max-width: 480px; margin: 0 auto; padding-bottom: 100px; }
    .info-banner { display: flex; align-items: center; gap: 10px; background: rgba(0,71,187,0.06); border: 1px solid rgba(0,71,187,0.12); border-radius: 14px; padding: 14px 16px; margin-bottom: 24px; }
    .info-banner mat-icon { color: #0047BB; font-size: 20px; width: 20px; height: 20px; }
    .info-banner span { font-size: 0.82rem; color: #555; line-height: 1.4; }
    .section-label { font-size: 1rem; font-weight: 700; color: #1A1A2E; margin-bottom: 14px; }
    .key-card { display: flex; align-items: center; gap: 14px; background: #fff; border-radius: 16px; padding: 18px 16px; margin-bottom: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.04); cursor: pointer; transition: all 0.2s; }
    .key-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.08); }
    .key-icon { width: 46px; height: 46px; border-radius: 14px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .key-icon mat-icon { font-size: 22px; width: 22px; height: 22px; }
    .key-info { flex: 1; display: flex; flex-direction: column; gap: 2px; }
    .key-type { font-size: 0.82rem; color: #9CA3AF; font-weight: 500; }
    .key-value { font-size: 0.95rem; font-weight: 600; color: #1A1A2E; }
    .key-actions { display: flex; gap: 4px; }
    .icon-btn { background: none; border: none; cursor: pointer; padding: 8px; border-radius: 10px; transition: all 0.2s; display: flex; }
    .icon-btn:hover { background: #F0F4FF; }
    .icon-btn mat-icon { font-size: 20px; width: 20px; height: 20px; color: #9CA3AF; }
    .icon-btn.danger:hover { background: #FFF0F0; }
    .icon-btn.danger:hover mat-icon { color: #E53935; }
    .empty-state { text-align: center; padding: 40px 20px; background: #fff; border-radius: 16px; }
    .empty-state mat-icon { font-size: 48px; width: 48px; height: 48px; color: #D0D5DD; }
    .empty-state p { color: #9CA3AF; margin-top: 8px; }
    .add-btn { display: flex; align-items: center; gap: 14px; width: 100%; background: #fff; border: 2px dashed #E5E7EB; border-radius: 16px; padding: 18px 16px; cursor: pointer; transition: all 0.2s; font-family: 'Plus Jakarta Sans', sans-serif; margin-top: 16px; }
    .add-btn:hover { border-color: #0047BB; background: rgba(0,71,187,0.02); }
    .add-icon { width: 46px; height: 46px; border-radius: 14px; background: #F0F4FF; display: flex; align-items: center; justify-content: center; }
    .add-icon mat-icon { color: #0047BB; }
    .add-btn span { flex: 1; text-align: left; font-weight: 600; color: #0047BB; font-size: 0.92rem; }
    .add-btn .chevron { color: #B0B8C4; }
    .type-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin-bottom: 24px; }
    .type-btn { background: #fff; border: 2px solid #E5E7EB; border-radius: 14px; padding: 14px 8px; display: flex; flex-direction: column; align-items: center; gap: 8px; cursor: pointer; transition: all 0.2s; font-family: 'Plus Jakarta Sans', sans-serif; }
    .type-btn:hover { border-color: #B0C4DE; }
    .type-btn.selected { border-color: #0047BB; background: rgba(0,71,187,0.04); }
    .type-icon { width: 40px; height: 40px; border-radius: 12px; display: flex; align-items: center; justify-content: center; }
    .type-icon mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .type-btn span { font-size: 0.72rem; font-weight: 600; color: #555; }
    .field { margin-bottom: 20px; }
    .field label { display: block; font-size: 0.82rem; font-weight: 600; color: #555; margin-bottom: 8px; }
    .input-wrap { display: flex; align-items: center; gap: 10px; border: 2px solid #E5E7EB; border-radius: 14px; padding: 0 16px; height: 52px; transition: border-color 0.2s; background: #FAFBFC; }
    .input-wrap:focus-within { border-color: #0047BB; background: #fff; }
    .input-wrap input { flex: 1; border: none; outline: none; background: transparent; font-size: 1rem; font-family: 'Plus Jakarta Sans', sans-serif; color: #1A1A2E; }
    .input-wrap input::placeholder { color: #B0B8C4; }
    .input-wrap mat-icon { color: #9CA3AF; font-size: 22px; width: 22px; height: 22px; }
    .btn-row { display: flex; gap: 12px; margin-top: 8px; }
    .btn-primary { height: 50px; border: none; border-radius: 14px; background: linear-gradient(135deg, #0047BB, #002a70); color: #fff; font-size: 0.9rem; font-weight: 700; cursor: pointer; display: flex; align-items: center; justify-content: center; font-family: 'Plus Jakarta Sans', sans-serif; padding: 0 24px; }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .flex-btn { flex: 1; }
    .btn-secondary { height: 50px; border: 2px solid #E5E7EB; border-radius: 14px; background: #fff; color: #555; font-size: 0.9rem; font-weight: 600; cursor: pointer; padding: 0 24px; font-family: 'Plus Jakarta Sans', sans-serif; }
    .limit-info { margin-top: 32px; text-align: center; }
    .limit-info span { font-size: 0.82rem; color: #9CA3AF; }
    .limit-bar { height: 4px; background: #E5E7EB; border-radius: 2px; margin-top: 8px; overflow: hidden; }
    .limit-fill { height: 100%; background: #0047BB; border-radius: 2px; transition: width 0.3s; }
  `]
})
export class PixKeysComponent implements OnInit {
  showForm = false;
  newType = 'cpf';
  newValue = '';
  keys: any[] = [];

  types = [
    { id: 'cpf', label: 'CPF', icon: 'badge', color: '#0047BB' },
    { id: 'email', label: 'Email', icon: 'email', color: '#00C853' },
    { id: 'phone', label: 'Celular', icon: 'phone', color: '#FF6B35' },
    { id: 'random', label: 'Aleatoria', icon: 'shuffle', color: '#7C3AED' },
  ];

  constructor(public router: Router, private snackBar: MatSnackBar, private http: HttpClient) {}

  ngOnInit() {
    this.loadKeys();
  }

  private getHeaders() {
    const token = localStorage.getItem('krt_token');
    return { headers: { Authorization: 'Bearer ' + token } };
  }

  private getAccountId(): string {
    return localStorage.getItem('krt_account_id') || '';
  }

  loadKeys() {
    const accountId = this.getAccountId();
    if (!accountId) return;
    this.http.get<any[]>('http://localhost:5000/api/v1/pix-keys/account/' + accountId, this.getHeaders()).subscribe({
      next: (keys) => {
        const typeMap: any = {
          Cpf: { label: 'CPF', icon: 'badge', color: '#0047BB' },
          Email: { label: 'Email', icon: 'email', color: '#00C853' },
          Phone: { label: 'Celular', icon: 'phone', color: '#FF6B35' },
          Random: { label: 'Aleatoria', icon: 'shuffle', color: '#7C3AED' }
        };
        this.keys = keys.map((k: any) => {
          const t = typeMap[k.keyType] || typeMap['Cpf'];
          return { id: k.id, type: t.label, value: k.keyValue, icon: t.icon, color: t.color, keyType: k.keyType };
        });
      },
      error: () => { this.snackBar.open('Erro ao carregar chaves', '', { duration: 3000 }); }
    });
  }

  getNewLabel(): string { const m: any = { cpf: 'CPF', email: 'Seu email', phone: 'Seu celular' }; return m[this.newType] || ''; }
  getNewPlaceholder(): string { const m: any = { cpf: '000.000.000-00', email: 'email@exemplo.com', phone: '(00) 00000-0000' }; return m[this.newType] || ''; }
  getNewIcon(): string { const m: any = { cpf: 'badge', email: 'email', phone: 'phone' }; return m[this.newType] || 'key'; }
  getNewMaxLen(): number { const m: any = { cpf: 14, email: 100, phone: 15 }; return m[this.newType] || 50; }

  maskNewValue(event: any) {
    if (this.newType === 'cpf') {
      let v = event.target.value.replace(/\D/g, '');
      if (v.length > 11) v = v.slice(0, 11);
      if (v.length > 9) v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{1,2})/, '$1.$2.$3-$4');
      else if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d{1,3})/, '$1.$2.$3');
      else if (v.length > 3) v = v.replace(/(\d{3})(\d{1,3})/, '$1.$2');
      this.newValue = v; event.target.value = v;
    } else if (this.newType === 'phone') {
      let v = event.target.value.replace(/\D/g, '');
      if (v.length > 11) v = v.slice(0, 11);
      if (v.length > 6) v = v.replace(/(\d{2})(\d{5})(\d{1,4})/, '($1) $2-$3');
      else if (v.length > 2) v = v.replace(/(\d{2})(\d{1,5})/, '($1) $2');
      this.newValue = v; event.target.value = v;
    }
  }

  isNewValid(): boolean {
    if (this.newType === 'cpf') return this.newValue.replace(/\D/g, '').length === 11;
    if (this.newType === 'email') return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.newValue);
    if (this.newType === 'phone') return this.newValue.replace(/\D/g, '').length >= 10;
    if (this.newType === 'random') return true;
    return false;
  }

  addKey() {
    const accountId = this.getAccountId();
    if (!accountId) return;
    const body: any = { accountId: accountId, keyType: this.newType };
    if (this.newType !== 'random') { body.keyValue = this.newValue; }
    this.http.post<any>('http://localhost:5000/api/v1/pix-keys/register', body, this.getHeaders()).subscribe({
      next: () => {
        this.snackBar.open('Chave PIX registrada!', '', { duration: 2000 });
        this.showForm = false; this.newValue = '';
        this.loadKeys();
      },
      error: (err: any) => {
        const msg = err?.error?.error || 'Erro ao registrar chave PIX';
        this.snackBar.open(msg, '', { duration: 3000 });
      }
    });
  }

  copyKey(key: any) {
    navigator.clipboard.writeText(key.value);
    this.snackBar.open('Chave copiada!', '', { duration: 1500 });
  }

  removeKey(key: any) {
    if (!confirm('Tem certeza que deseja remover esta chave?')) return;
    this.http.delete('http://localhost:5000/api/v1/pix-keys/' + key.id, this.getHeaders()).subscribe({
      next: () => {
        this.snackBar.open('Chave removida!', '', { duration: 2000 });
        this.loadKeys();
      },
      error: () => {
        this.snackBar.open('Erro ao remover chave', '', { duration: 3000 });
      }
    });
  }
}
