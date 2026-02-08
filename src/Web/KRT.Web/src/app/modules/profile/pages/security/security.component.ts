import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-security',
  template: `
    <div class="security-container page-with-nav">
      <header class="page-header">
        <button class="back-btn" (click)="router.navigate(['/profile'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Seguranca</h1>
        <div style="width:40px"></div>
      </header>

      <div class="content fade-in">
        <div class="section-label">Acesso</div>

        <div class="option-card">
          <div class="option-icon" style="background:rgba(124,58,237,0.1)">
            <mat-icon style="color:#7C3AED">fingerprint</mat-icon>
          </div>
          <div class="option-info">
            <span class="option-title">Biometria / FaceID</span>
            <span class="option-desc">Usar para entrar no app</span>
          </div>
          <label class="toggle">
            <input type="checkbox" [(ngModel)]="biometric">
            <span class="slider"></span>
          </label>
        </div>

        <div class="option-card">
          <div class="option-icon" style="background:rgba(0,71,187,0.1)">
            <mat-icon style="color:#0047BB">phonelink_lock</mat-icon>
          </div>
          <div class="option-info">
            <span class="option-title">Autenticacao em 2 fatores</span>
            <span class="option-desc">SMS ou app autenticador</span>
          </div>
          <label class="toggle">
            <input type="checkbox" [(ngModel)]="twoFactor">
            <span class="slider"></span>
          </label>
        </div>

        <div class="option-card">
          <div class="option-icon" style="background:rgba(0,212,170,0.1)">
            <mat-icon style="color:#00D4AA">notifications_active</mat-icon>
          </div>
          <div class="option-info">
            <span class="option-title">Alertas de login</span>
            <span class="option-desc">Notificar novos acessos</span>
          </div>
          <label class="toggle">
            <input type="checkbox" [(ngModel)]="loginAlerts" checked>
            <span class="slider"></span>
          </label>
        </div>

        <div class="section-label" style="margin-top:32px">Senha do app</div>

        <div class="field">
          <label>Senha atual</label>
          <div class="input-wrap">
            <input [type]="show.current ? 'text' : 'password'" [(ngModel)]="currentPass" placeholder="Digite sua senha atual" autocomplete="off">
            <mat-icon class="clickable" (click)="show.current = !show.current">{{ show.current ? 'visibility_off' : 'visibility' }}</mat-icon>
          </div>
        </div>

        <div class="field">
          <label>Nova senha</label>
          <div class="input-wrap">
            <input [type]="show.newPass ? 'text' : 'password'" [(ngModel)]="newPass" placeholder="Minimo 8 caracteres" autocomplete="off">
            <mat-icon class="clickable" (click)="show.newPass = !show.newPass">{{ show.newPass ? 'visibility_off' : 'visibility' }}</mat-icon>
          </div>
          <div class="strength-bar" *ngIf="newPass">
            <div class="strength-fill" [style.width]="getStrength() + '%'" [style.background]="getStrengthColor()"></div>
          </div>
          <span class="hint" *ngIf="newPass">{{ getStrengthLabel() }}</span>
        </div>

        <div class="field">
          <label>Confirmar nova senha</label>
          <div class="input-wrap" [class.error]="confirmPass && confirmPass !== newPass">
            <input [type]="show.confirm ? 'text' : 'password'" [(ngModel)]="confirmPass" placeholder="Repita a nova senha" autocomplete="off">
            <mat-icon class="clickable" (click)="show.confirm = !show.confirm">{{ show.confirm ? 'visibility_off' : 'visibility' }}</mat-icon>
          </div>
          <span class="hint error-text" *ngIf="confirmPass && confirmPass !== newPass">Senhas nao coincidem</span>
        </div>

        <button class="btn-primary" [disabled]="!canSave()" (click)="changePassword()">
          ALTERAR SENHA
        </button>

        <div class="section-label" style="margin-top:32px">Dispositivos conectados</div>

        <div class="device-card" *ngFor="let d of devices">
          <div class="device-icon" [style.background]="d.color + '12'">
            <mat-icon [style.color]="d.color">{{ d.icon }}</mat-icon>
          </div>
          <div class="device-info">
            <span class="device-name">{{ d.name }}</span>
            <span class="device-detail">{{ d.detail }}</span>
          </div>
          <button class="icon-btn" *ngIf="!d.current" (click)="removeDevice(d)">
            <mat-icon>close</mat-icon>
          </button>
          <span class="current-badge" *ngIf="d.current">Este</span>
        </div>
      </div>
    </div>
    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .security-container { min-height: 100vh; background: var(--krt-bg); }
    .page-header { display: flex; align-items: center; justify-content: space-between; padding: 16px 20px; background: #fff; border-bottom: 1px solid #F0F0F0; }
    .page-header h1 { font-size: 1.05rem; font-weight: 700; margin: 0; color: #1A1A2E; }
    .back-btn { background: none; border: none; cursor: pointer; padding: 4px; color: #1A1A2E; display: flex; }
    .content { padding: 24px 20px; max-width: 480px; margin: 0 auto; padding-bottom: 100px; }
    .section-label { font-size: 0.85rem; font-weight: 700; color: #9CA3AF; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 14px; }

    .option-card { display: flex; align-items: center; gap: 14px; background: #fff; border-radius: 16px; padding: 16px; margin-bottom: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.04); }
    .option-icon { width: 46px; height: 46px; border-radius: 14px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .option-icon mat-icon { font-size: 22px; width: 22px; height: 22px; }
    .option-info { flex: 1; display: flex; flex-direction: column; gap: 2px; }
    .option-title { font-size: 0.92rem; font-weight: 600; color: #1A1A2E; }
    .option-desc { font-size: 0.78rem; color: #9CA3AF; }

    .toggle { position: relative; width: 48px; height: 28px; flex-shrink: 0; }
    .toggle input { opacity: 0; width: 0; height: 0; }
    .slider { position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: #E0E0E0; border-radius: 28px; cursor: pointer; transition: 0.3s; }
    .slider:before { content: ''; position: absolute; width: 22px; height: 22px; left: 3px; bottom: 3px; background: #fff; border-radius: 50%; transition: 0.3s; }
    .toggle input:checked + .slider { background: #0047BB; }
    .toggle input:checked + .slider:before { transform: translateX(20px); }

    .field { margin-bottom: 18px; }
    .field label { display: block; font-size: 0.82rem; font-weight: 600; color: #555; margin-bottom: 8px; }
    .input-wrap { display: flex; align-items: center; gap: 10px; border: 2px solid #E5E7EB; border-radius: 14px; padding: 0 16px; height: 52px; transition: all 0.2s; background: #fff; }
    .input-wrap:focus-within { border-color: #0047BB; }
    .input-wrap.error { border-color: #E53935; }
    .input-wrap input { flex: 1; border: none; outline: none; background: transparent; font-size: 0.95rem; font-family: 'Plus Jakarta Sans', sans-serif; color: #1A1A2E; }
    .input-wrap input::placeholder { color: #B0B8C4; }
    .input-wrap mat-icon { color: #9CA3AF; font-size: 20px; width: 20px; height: 20px; }
    .input-wrap .clickable { cursor: pointer; }
    .hint { font-size: 0.75rem; color: #9CA3AF; margin-top: 4px; display: block; }
    .error-text { color: #E53935; }

    .strength-bar { height: 4px; background: #E5E7EB; border-radius: 2px; margin-top: 8px; overflow: hidden; }
    .strength-fill { height: 100%; border-radius: 2px; transition: all 0.3s; }

    .btn-primary { width: 100%; height: 54px; border: none; border-radius: 14px; background: linear-gradient(135deg, #0047BB, #002a70); color: #fff; font-size: 0.95rem; font-weight: 700; cursor: pointer; display: flex; align-items: center; justify-content: center; box-shadow: 0 8px 24px rgba(0,71,187,0.3); transition: all 0.3s; font-family: 'Plus Jakarta Sans', sans-serif; margin-top: 8px; }
    .btn-primary:hover:not(:disabled) { transform: translateY(-1px); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

    .device-card { display: flex; align-items: center; gap: 14px; background: #fff; border-radius: 16px; padding: 16px; margin-bottom: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.04); }
    .device-icon { width: 42px; height: 42px; border-radius: 12px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .device-icon mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .device-info { flex: 1; display: flex; flex-direction: column; gap: 2px; }
    .device-name { font-size: 0.88rem; font-weight: 600; color: #1A1A2E; }
    .device-detail { font-size: 0.75rem; color: #9CA3AF; }
    .icon-btn { background: none; border: none; cursor: pointer; padding: 6px; border-radius: 8px; display: flex; }
    .icon-btn:hover { background: #FFF0F0; }
    .icon-btn mat-icon { font-size: 18px; width: 18px; height: 18px; color: #9CA3AF; }
    .current-badge { font-size: 0.72rem; font-weight: 600; color: #00C853; background: rgba(0,200,83,0.1); padding: 4px 10px; border-radius: 20px; }
  `]
})
export class SecurityComponent {
  biometric = false;
  twoFactor = false;
  loginAlerts = true;
  currentPass = '';
  newPass = '';
  confirmPass = '';
  show = { current: false, newPass: false, confirm: false };

  devices = [
    { name: 'Chrome - Windows', detail: 'Ativo agora', icon: 'computer', color: '#0047BB', current: true },
    { name: 'iPhone 14', detail: 'Ultimo acesso: ontem', icon: 'smartphone', color: '#7C3AED', current: false },
  ];

  constructor(public router: Router, private snackBar: MatSnackBar) {}

  getStrength(): number {
    let s = 0;
    if (this.newPass.length >= 8) s += 25;
    if (/[A-Z]/.test(this.newPass)) s += 25;
    if (/[0-9]/.test(this.newPass)) s += 25;
    if (/[^A-Za-z0-9]/.test(this.newPass)) s += 25;
    return s;
  }
  getStrengthColor(): string {
    const s = this.getStrength();
    if (s <= 25) return '#E53935';
    if (s <= 50) return '#FF6B35';
    if (s <= 75) return '#FFB300';
    return '#00C853';
  }
  getStrengthLabel(): string {
    const s = this.getStrength();
    if (s <= 25) return 'Fraca';
    if (s <= 50) return 'Razoavel';
    if (s <= 75) return 'Boa';
    return 'Forte';
  }

  canSave(): boolean {
    return this.currentPass.length > 0 && this.newPass.length >= 8 && this.newPass === this.confirmPass;
  }

  changePassword() {
    this.snackBar.open('Senha alterada com sucesso!', '', { duration: 2000 });
    this.currentPass = ''; this.newPass = ''; this.confirmPass = '';
  }

  removeDevice(d: any) {
    this.devices = this.devices.filter(x => x !== d);
    this.snackBar.open('Dispositivo removido', '', { duration: 1500 });
  }
}
