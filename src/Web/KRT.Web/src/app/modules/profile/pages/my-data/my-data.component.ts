import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-my-data',
  template: `
    <div class="data-container page-with-nav">
      <header class="page-header">
        <button class="back-btn" (click)="router.navigate(['/profile'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Meus Dados</h1>
        <div style="width:40px"></div>
      </header>

      <div class="content fade-in">
        <div class="avatar-section">
          <div class="avatar">{{ getInitials() }}</div>
          <button class="photo-btn">Alterar foto</button>
        </div>

        <div class="field">
          <label>Nome Completo</label>
          <div class="input-wrap" [class.locked]="!editing.name">
            <input type="text" [(ngModel)]="name" [disabled]="!editing.name" autocomplete="off">
            <mat-icon class="clickable" (click)="editing.name = !editing.name">{{ editing.name ? 'check' : 'edit' }}</mat-icon>
          </div>
        </div>

        <div class="field">
          <label>CPF</label>
          <div class="input-wrap locked">
            <input type="text" [value]="cpf" disabled>
            <mat-icon>lock</mat-icon>
          </div>
          <span class="hint">CPF nao pode ser alterado</span>
        </div>

        <div class="field">
          <label>E-mail</label>
          <div class="input-wrap" [class.locked]="!editing.email">
            <input type="email" [(ngModel)]="email" [disabled]="!editing.email" autocomplete="off">
            <mat-icon class="clickable" (click)="editing.email = !editing.email">{{ editing.email ? 'check' : 'edit' }}</mat-icon>
          </div>
        </div>

        <div class="field">
          <label>Celular</label>
          <div class="input-wrap" [class.locked]="!editing.phone">
            <input type="text" [(ngModel)]="phone" [disabled]="!editing.phone" (input)="maskPhone($event)" maxlength="15" autocomplete="off">
            <mat-icon class="clickable" (click)="editing.phone = !editing.phone">{{ editing.phone ? 'check' : 'edit' }}</mat-icon>
          </div>
        </div>

        <div class="field">
          <label>Data de Nascimento</label>
          <div class="input-wrap locked">
            <input type="text" [value]="birthDate" disabled>
            <mat-icon>lock</mat-icon>
          </div>
        </div>

        <button class="btn-primary" (click)="save()">
          SALVAR ALTERACOES
        </button>

        <div class="danger-zone">
          <div class="section-label">Zona de perigo</div>
          <button class="btn-danger" (click)="confirmDelete = true">
            <mat-icon>delete_forever</mat-icon>
            Encerrar minha conta
          </button>
          <div class="confirm-box" *ngIf="confirmDelete">
            <p>Tem certeza? Esta acao e irreversivel.</p>
            <div class="btn-row">
              <button class="btn-secondary" (click)="confirmDelete = false">Cancelar</button>
              <button class="btn-danger-fill">Confirmar</button>
            </div>
          </div>
        </div>
      </div>
    </div>
    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .data-container { min-height: 100vh; background: var(--krt-bg); }
    .page-header { display: flex; align-items: center; justify-content: space-between; padding: 16px 20px; background: #fff; border-bottom: 1px solid #F0F0F0; }
    .page-header h1 { font-size: 1.05rem; font-weight: 700; margin: 0; color: #1A1A2E; }
    .back-btn { background: none; border: none; cursor: pointer; padding: 4px; color: #1A1A2E; display: flex; }
    .content { padding: 24px 20px; max-width: 480px; margin: 0 auto; padding-bottom: 100px; }

    .avatar-section { text-align: center; margin-bottom: 32px; }
    .avatar { width: 80px; height: 80px; border-radius: 50%; background: linear-gradient(135deg, #0047BB, #002a70); color: #fff; font-size: 1.6rem; font-weight: 800; display: flex; align-items: center; justify-content: center; margin: 0 auto 10px; }
    .photo-btn { background: none; border: none; color: #0047BB; font-weight: 600; font-size: 0.85rem; cursor: pointer; font-family: 'Plus Jakarta Sans', sans-serif; }

    .field { margin-bottom: 20px; }
    .field label { display: block; font-size: 0.82rem; font-weight: 600; color: #555; margin-bottom: 8px; }
    .input-wrap { display: flex; align-items: center; gap: 10px; border: 2px solid #E5E7EB; border-radius: 14px; padding: 0 16px; height: 52px; transition: all 0.2s; background: #fff; }
    .input-wrap:focus-within { border-color: #0047BB; }
    .input-wrap.locked { background: #F9FAFB; }
    .input-wrap input { flex: 1; border: none; outline: none; background: transparent; font-size: 0.95rem; font-family: 'Plus Jakarta Sans', sans-serif; color: #1A1A2E; }
    .input-wrap input:disabled { color: #6B7280; }
    .input-wrap input::placeholder { color: #B0B8C4; }
    .input-wrap mat-icon { color: #9CA3AF; font-size: 20px; width: 20px; height: 20px; }
    .input-wrap .clickable { cursor: pointer; color: #0047BB; }
    .hint { font-size: 0.75rem; color: #B0B8C4; margin-top: 4px; display: block; }

    .btn-primary { width: 100%; height: 54px; border: none; border-radius: 14px; background: linear-gradient(135deg, #0047BB, #002a70); color: #fff; font-size: 0.95rem; font-weight: 700; cursor: pointer; display: flex; align-items: center; justify-content: center; box-shadow: 0 8px 24px rgba(0,71,187,0.3); transition: all 0.3s; font-family: 'Plus Jakarta Sans', sans-serif; margin-top: 8px; }
    .btn-primary:hover { transform: translateY(-1px); }

    .danger-zone { margin-top: 40px; padding-top: 24px; border-top: 1px solid #F0F0F0; }
    .section-label { font-size: 0.85rem; font-weight: 700; color: #E53935; margin-bottom: 14px; }
    .btn-danger { display: flex; align-items: center; gap: 10px; width: 100%; background: #fff; border: 2px solid #FFCDD2; border-radius: 14px; padding: 16px; color: #E53935; font-weight: 600; font-size: 0.9rem; cursor: pointer; transition: all 0.2s; font-family: 'Plus Jakarta Sans', sans-serif; }
    .btn-danger:hover { background: #FFF0F0; border-color: #E53935; }
    .btn-danger mat-icon { font-size: 22px; width: 22px; height: 22px; }
    .confirm-box { background: #FFF0F0; border-radius: 14px; padding: 16px; margin-top: 12px; }
    .confirm-box p { font-size: 0.88rem; color: #C62828; margin: 0 0 12px; }
    .btn-row { display: flex; gap: 10px; }
    .btn-secondary { flex: 1; height: 44px; border: 2px solid #E5E7EB; border-radius: 12px; background: #fff; color: #555; font-weight: 600; font-size: 0.85rem; cursor: pointer; font-family: 'Plus Jakarta Sans', sans-serif; }
    .btn-danger-fill { flex: 1; height: 44px; border: none; border-radius: 12px; background: #E53935; color: #fff; font-weight: 600; font-size: 0.85rem; cursor: pointer; font-family: 'Plus Jakarta Sans', sans-serif; }
  `]
})
export class MyDataComponent implements OnInit {
  name = '';
  cpf = '';
  email = '';
  phone = '';
  birthDate = '15/03/1995';
  confirmDelete = false;
  editing = { name: false, email: false, phone: false };

  constructor(public router: Router, private snackBar: MatSnackBar) {}

  ngOnInit() {
    this.name = localStorage.getItem('krt_account_name') || 'Usuario';
    this.cpf = this.maskCpfDisplay(localStorage.getItem('krt_account_doc') || '');
    this.email = localStorage.getItem('krt_account_email') || '';
    this.phone = localStorage.getItem('krt_account_phone') || '(11) 99999-9999';
  }

  getInitials(): string {
    const parts = this.name.split(' ');
    return parts.length > 1 ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase() : this.name.substring(0, 2).toUpperCase();
  }

  maskCpfDisplay(cpf: string): string {
    const d = cpf.replace(/\D/g, '');
    if (d.length === 11) return '***.' + d.substring(3, 6) + '.' + d.substring(6, 9) + '-**';
    return cpf;
  }

  maskPhone(event: any) {
    let v = event.target.value.replace(/\D/g, '');
    if (v.length > 11) v = v.slice(0, 11);
    if (v.length > 6) v = v.replace(/(\d{2})(\d{5})(\d{1,4})/, '($1) $2-$3');
    else if (v.length > 2) v = v.replace(/(\d{2})(\d{1,5})/, '($1) $2');
    this.phone = v; event.target.value = v;
  }

  save() {
    localStorage.setItem('krt_account_name', this.name);
    localStorage.setItem('krt_account_phone', this.phone);
    this.editing = { name: false, email: false, phone: false };
    this.snackBar.open('Dados salvos!', '', { duration: 2000 });
  }
}

