import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  template: `
    <div class="admin-users-shell">
      <!-- Header -->
      <header class="page-header">
        <div class="header-inner">
          <div class="header-left">
            <button class="back-btn" (click)="goBack()">
              <mat-icon>arrow_back</mat-icon>
            </button>
            <div>
              <h1>Gerenciamento de Usuarios</h1>
              <p>Gerencie contas e permissoes</p>
            </div>
          </div>
          <div class="pending-badge" *ngIf="pendingCount > 0">
            <mat-icon>pending_actions</mat-icon>
            <span>{{ pendingCount }} pendente{{ pendingCount > 1 ? 's' : '' }}</span>
          </div>
        </div>
      </header>

      <!-- Tabs -->
      <div class="tabs-container">
        <div class="tabs-inner">
          <button class="tab" [class.active]="activeTab === 'all'" (click)="setTab('all')">
            Todos <span class="tab-count">{{ users.length }}</span>
          </button>
          <button class="tab" [class.active]="activeTab === 'pending'" (click)="setTab('pending')">
            Pendentes
            <span class="tab-count pending" *ngIf="pendingCount > 0">{{ pendingCount }}</span>
          </button>
          <button class="tab" [class.active]="activeTab === 'active'" (click)="setTab('active')">
            Ativos <span class="tab-count">{{ getCountByStatus('Active') }}</span>
          </button>
          <button class="tab" [class.active]="activeTab === 'inactive'" (click)="setTab('inactive')">
            Inativos <span class="tab-count">{{ getCountByStatus('Inactive') }}</span>
          </button>
        </div>
      </div>

      <!-- Content -->
      <div class="content-area">
        <!-- Loading -->
        <div class="loading-state" *ngIf="isLoading">
          <div class="skel skel-row" *ngFor="let s of [1,2,3,4,5]"></div>
        </div>

        <!-- Empty State -->
        <div class="empty-state" *ngIf="!isLoading && filteredUsers.length === 0">
          <div class="empty-icon"><mat-icon>people_outline</mat-icon></div>
          <h3>Nenhum usuario encontrado</h3>
          <p>Nao ha usuarios nesta categoria.</p>
        </div>

        <!-- Users List (cards on mobile, table on desktop) -->
        <div class="users-list" *ngIf="!isLoading && filteredUsers.length > 0">
          <div class="user-card" *ngFor="let user of filteredUsers" [class]="'status-' + getStatusClass(user.status)">
            <div class="user-card-header">
              <div class="user-avatar">{{ getInitials(user.fullName || user.name) }}</div>
              <div class="user-info">
                <strong>{{ user.fullName || user.name }}</strong>
                <span class="user-email">{{ user.email }}</span>
              </div>
              <span class="status-badge" [class]="'badge-' + getStatusClass(user.status)">
                {{ translateStatus(user.status) }}
              </span>
            </div>

            <div class="user-card-details">
              <div class="detail">
                <mat-icon>badge</mat-icon>
                <span>{{ formatCpf(user.document || user.cpf) }}</span>
              </div>
              <div class="detail">
                <mat-icon>admin_panel_settings</mat-icon>
                <span>{{ user.role || 'Cliente' }}</span>
              </div>
              <div class="detail">
                <mat-icon>calendar_today</mat-icon>
                <span>{{ formatDate(user.createdAt) }}</span>
              </div>
            </div>

            <div class="user-card-actions">
              <!-- Pending: Approve / Reject -->
              <ng-container *ngIf="isPending(user.status)">
                <button class="action-btn approve" (click)="confirmAction('aprovar', user)">
                  <mat-icon>check_circle</mat-icon> Aprovar
                </button>
                <button class="action-btn reject" (click)="confirmAction('rejeitar', user)">
                  <mat-icon>cancel</mat-icon> Rejeitar
                </button>
              </ng-container>

              <!-- Active: Deactivate / Change Role -->
              <ng-container *ngIf="user.status === 'Active'">
                <button class="action-btn role" (click)="openRoleDialog(user)">
                  <mat-icon>swap_horiz</mat-icon> Alterar Role
                </button>
                <button class="action-btn deactivate" (click)="confirmAction('desativar', user)">
                  <mat-icon>block</mat-icon> Desativar
                </button>
              </ng-container>

              <!-- Inactive: Activate -->
              <ng-container *ngIf="user.status === 'Inactive'">
                <button class="action-btn approve" (click)="confirmAction('ativar', user)">
                  <mat-icon>check_circle</mat-icon> Ativar
                </button>
              </ng-container>

              <!-- Rejected: Approve (second chance) -->
              <ng-container *ngIf="user.status === 'Rejected'">
                <button class="action-btn approve" (click)="confirmAction('aprovar', user)">
                  <mat-icon>check_circle</mat-icon> Aprovar
                </button>
              </ng-container>
            </div>
          </div>
        </div>
      </div>

      <!-- Confirmation Dialog -->
      <div class="dialog-overlay" *ngIf="showConfirmDialog" (click)="closeDialogs()">
        <div class="dialog-card fade-in" (click)="$event.stopPropagation()">
          <div class="dialog-icon" [class]="'dialog-' + confirmDialogAction">
            <mat-icon>{{ getDialogIcon() }}</mat-icon>
          </div>
          <h3>Confirmar acao</h3>
          <p>Tem certeza que deseja <strong>{{ confirmDialogAction }}</strong> o usuario <strong>{{ confirmDialogUser?.fullName || confirmDialogUser?.name }}</strong>?</p>
          <div class="dialog-actions">
            <button class="dialog-btn cancel" (click)="closeDialogs()">Cancelar</button>
            <button class="dialog-btn confirm" [class]="'confirm-' + confirmDialogAction"
                    (click)="executeAction()" [disabled]="actionLoading">
              <span *ngIf="!actionLoading">Confirmar</span>
              <div class="spinner-sm" *ngIf="actionLoading">
                <div class="dot dot1"></div>
                <div class="dot dot2"></div>
                <div class="dot dot3"></div>
              </div>
            </button>
          </div>
        </div>
      </div>

      <!-- Role Dialog -->
      <div class="dialog-overlay" *ngIf="showRoleDialog" (click)="closeDialogs()">
        <div class="dialog-card fade-in" (click)="$event.stopPropagation()">
          <div class="dialog-icon dialog-role">
            <mat-icon>admin_panel_settings</mat-icon>
          </div>
          <h3>Alterar Role</h3>
          <p>Selecione a nova role para <strong>{{ roleDialogUser?.fullName || roleDialogUser?.name }}</strong></p>
          <div class="role-options">
            <label class="role-option" *ngFor="let r of roles"
                   [class.selected]="selectedRole === r.value">
              <input type="radio" [value]="r.value" [(ngModel)]="selectedRole" name="role">
              <div class="role-content">
                <mat-icon>{{ r.icon }}</mat-icon>
                <div>
                  <strong>{{ r.label }}</strong>
                  <span>{{ r.desc }}</span>
                </div>
              </div>
            </label>
          </div>
          <div class="dialog-actions">
            <button class="dialog-btn cancel" (click)="closeDialogs()">Cancelar</button>
            <button class="dialog-btn confirm confirm-role" (click)="saveRole()" [disabled]="actionLoading || !selectedRole">
              <span *ngIf="!actionLoading">Salvar</span>
              <div class="spinner-sm" *ngIf="actionLoading">
                <div class="dot dot1"></div>
                <div class="dot dot2"></div>
                <div class="dot dot3"></div>
              </div>
            </button>
          </div>
        </div>
      </div>

      <!-- Success Toast -->
      <div class="toast" *ngIf="toastMsg" [class.show]="toastMsg">
        <mat-icon>check_circle</mat-icon>
        {{ toastMsg }}
      </div>
    </div>
  `,
  styles: [`
    .admin-users-shell {
      min-height: 100vh; background: #f0f2f5;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }

    /* Header */
    .page-header {
      background: linear-gradient(135deg, #0047BB 0%, #0035a0 40%, #002a70 100%);
      padding: 0 0 20px; position: relative;
    }
    .page-header::after {
      content: ''; position: absolute; bottom: -1px; left: 0; right: 0; height: 20px;
      background: #f0f2f5; border-radius: 20px 20px 0 0;
    }
    .header-inner {
      max-width: 900px; margin: 0 auto; padding: 20px 20px 0;
      display: flex; justify-content: space-between; align-items: center;
      position: relative; z-index: 2;
    }
    .header-left { display: flex; align-items: center; gap: 14px; }
    .back-btn {
      width: 42px; height: 42px; border-radius: 14px; border: none;
      background: rgba(255,255,255,0.08); color: rgba(255,255,255,0.8);
      display: flex; align-items: center; justify-content: center;
      cursor: pointer; transition: all 0.2s; backdrop-filter: blur(8px);
    }
    .back-btn:hover { background: rgba(255,255,255,0.16); color: white; }
    .header-left h1 { font-size: 1.3rem; font-weight: 800; color: white; margin: 0; }
    .header-left p { font-size: 0.82rem; color: rgba(255,255,255,0.6); margin: 4px 0 0; }

    .pending-badge {
      display: flex; align-items: center; gap: 6px;
      background: rgba(255,107,53,0.2); color: #FF6B35;
      padding: 8px 14px; border-radius: 12px; font-size: 0.82rem; font-weight: 700;
    }
    .pending-badge mat-icon { font-size: 18px; width: 18px; height: 18px; }

    /* Tabs */
    .tabs-container { max-width: 900px; margin: 0 auto; padding: 0 20px; }
    .tabs-inner {
      display: flex; gap: 4px; background: #ffffff;
      border-radius: 16px; padding: 4px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.03);
      margin-bottom: 16px;
    }
    .tab {
      flex: 1; padding: 12px 8px; border: none; background: transparent;
      border-radius: 12px; font-size: 0.82rem; font-weight: 700;
      color: #6B7280; cursor: pointer; transition: all 0.2s;
      display: flex; align-items: center; justify-content: center; gap: 6px;
      font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .tab:hover { background: #f3f4f6; }
    .tab.active { background: #0047BB; color: white; box-shadow: 0 4px 12px rgba(0,71,187,0.3); }
    .tab-count {
      font-size: 0.72rem; background: rgba(0,0,0,0.08); padding: 2px 8px;
      border-radius: 8px; font-weight: 800;
    }
    .tab.active .tab-count { background: rgba(255,255,255,0.2); }
    .tab-count.pending { background: #FF6B35; color: white; }

    /* Content */
    .content-area { max-width: 900px; margin: 0 auto; padding: 0 20px 40px; }

    .loading-state { display: flex; flex-direction: column; gap: 12px; }
    .skel {
      background: linear-gradient(90deg, #e5e7eb 25%, #f3f4f6 50%, #e5e7eb 75%);
      background-size: 200% 100%; animation: shimmer 1.5s infinite; border-radius: 16px;
    }
    .skel-row { height: 120px; }
    @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }

    .empty-state { text-align: center; padding: 60px 20px; }
    .empty-icon {
      width: 64px; height: 64px; border-radius: 20px;
      background: #f3f4f6; display: flex; align-items: center; justify-content: center;
      margin: 0 auto 16px;
    }
    .empty-icon mat-icon { font-size: 32px; width: 32px; height: 32px; color: #9ca3af; }
    .empty-state h3 { font-size: 1.1rem; font-weight: 800; color: #1a1a2e; margin: 0 0 6px; }
    .empty-state p { color: #9ca3af; font-size: 0.88rem; margin: 0; }

    /* User Cards */
    .users-list { display: flex; flex-direction: column; gap: 12px; }
    .user-card {
      background: #ffffff; border-radius: 18px; padding: 20px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.03);
      transition: all 0.2s; border-left: 4px solid transparent;
    }
    .user-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.08); }
    .user-card.status-pending { border-left-color: #FF6B35; }
    .user-card.status-active { border-left-color: #00C853; }
    .user-card.status-inactive { border-left-color: #9CA3AF; }
    .user-card.status-rejected { border-left-color: #E53935; }

    .user-card-header { display: flex; align-items: center; gap: 14px; margin-bottom: 14px; }
    .user-avatar {
      width: 46px; height: 46px; border-radius: 14px;
      background: linear-gradient(135deg, #0047BB, #002a70); color: white;
      display: flex; align-items: center; justify-content: center;
      font-weight: 800; font-size: 0.85rem; flex-shrink: 0;
    }
    .user-info { flex: 1; min-width: 0; }
    .user-info strong { display: block; font-size: 0.95rem; color: #1a1a2e; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .user-email { font-size: 0.8rem; color: #9ca3af; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; display: block; }

    .status-badge {
      padding: 5px 12px; border-radius: 8px; font-size: 0.72rem;
      font-weight: 800; text-transform: uppercase; letter-spacing: 0.5px;
      flex-shrink: 0;
    }
    .badge-pending { background: #FFF3E0; color: #E65100; }
    .badge-active { background: #E8F5E9; color: #2E7D32; }
    .badge-inactive { background: #F5F5F5; color: #757575; }
    .badge-rejected { background: #FFEBEE; color: #C62828; }
    .badge-email { background: #E3F2FD; color: #1565C0; }

    .user-card-details {
      display: flex; gap: 20px; margin-bottom: 14px; flex-wrap: wrap;
    }
    .detail {
      display: flex; align-items: center; gap: 6px; font-size: 0.82rem; color: #6B7280;
    }
    .detail mat-icon { font-size: 16px; width: 16px; height: 16px; color: #9CA3AF; }

    .user-card-actions { display: flex; gap: 8px; flex-wrap: wrap; }
    .action-btn {
      display: flex; align-items: center; gap: 6px;
      padding: 8px 16px; border-radius: 10px; border: none;
      font-size: 0.82rem; font-weight: 700; cursor: pointer;
      transition: all 0.2s; font-family: 'Plus Jakarta Sans', sans-serif;
    }
    .action-btn mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .action-btn.approve { background: #E8F5E9; color: #2E7D32; }
    .action-btn.approve:hover { background: #C8E6C9; }
    .action-btn.reject { background: #FFEBEE; color: #C62828; }
    .action-btn.reject:hover { background: #FFCDD2; }
    .action-btn.deactivate { background: #FFF3E0; color: #E65100; }
    .action-btn.deactivate:hover { background: #FFE0B2; }
    .action-btn.role { background: #E3F2FD; color: #1565C0; }
    .action-btn.role:hover { background: #BBDEFB; }

    /* Dialogs */
    .dialog-overlay {
      position: fixed; inset: 0; z-index: 9999;
      background: rgba(0,0,0,0.5); backdrop-filter: blur(4px);
      display: flex; align-items: center; justify-content: center;
      padding: 20px;
    }
    .dialog-card {
      background: #ffffff; border-radius: 24px; padding: 32px;
      max-width: 420px; width: 100%;
      box-shadow: 0 24px 64px rgba(0,0,0,0.2);
      text-align: center;
    }
    .dialog-icon {
      width: 64px; height: 64px; border-radius: 20px;
      display: flex; align-items: center; justify-content: center;
      margin: 0 auto 20px;
    }
    .dialog-icon mat-icon { font-size: 32px; width: 32px; height: 32px; }
    .dialog-aprovar, .dialog-ativar { background: #E8F5E9; }
    .dialog-aprovar mat-icon, .dialog-ativar mat-icon { color: #2E7D32; }
    .dialog-rejeitar { background: #FFEBEE; }
    .dialog-rejeitar mat-icon { color: #C62828; }
    .dialog-desativar { background: #FFF3E0; }
    .dialog-desativar mat-icon { color: #E65100; }
    .dialog-role { background: #E3F2FD; }
    .dialog-role mat-icon { color: #1565C0; }

    .dialog-card h3 { font-size: 1.2rem; font-weight: 800; color: #1a1a2e; margin: 0 0 8px; }
    .dialog-card p { font-size: 0.9rem; color: #6B7280; margin: 0 0 24px; line-height: 1.5; }

    .dialog-actions { display: flex; gap: 10px; }
    .dialog-btn {
      flex: 1; padding: 14px; border-radius: 12px; border: none;
      font-size: 0.92rem; font-weight: 700; cursor: pointer;
      font-family: 'Plus Jakarta Sans', sans-serif; transition: all 0.2s;
      display: flex; align-items: center; justify-content: center;
    }
    .dialog-btn.cancel { background: #f3f4f6; color: #6B7280; }
    .dialog-btn.cancel:hover { background: #e5e7eb; }
    .dialog-btn.confirm { background: #0047BB; color: white; box-shadow: 0 4px 12px rgba(0,71,187,0.3); }
    .dialog-btn.confirm:hover { background: #0035a0; }
    .dialog-btn.confirm:disabled { opacity: 0.5; cursor: not-allowed; }
    .confirm-rejeitar, .confirm-desativar { background: #E53935 !important; box-shadow: 0 4px 12px rgba(229,57,53,0.3) !important; }
    .confirm-rejeitar:hover, .confirm-desativar:hover { background: #C62828 !important; }

    /* Role Options */
    .role-options { display: flex; flex-direction: column; gap: 8px; margin-bottom: 24px; text-align: left; }
    .role-option {
      display: flex; align-items: center; gap: 12px;
      padding: 14px 16px; border-radius: 14px; cursor: pointer;
      border: 2px solid #E5E7EB; transition: all 0.2s;
    }
    .role-option input { display: none; }
    .role-option.selected { border-color: #0047BB; background: rgba(0,71,187,0.04); }
    .role-content { display: flex; align-items: center; gap: 12px; }
    .role-content mat-icon { font-size: 24px; width: 24px; height: 24px; color: #0047BB; }
    .role-content strong { display: block; font-size: 0.9rem; color: #1a1a2e; }
    .role-content span { font-size: 0.78rem; color: #9ca3af; }

    /* Toast */
    .toast {
      position: fixed; bottom: 30px; left: 50%; transform: translateX(-50%) translateY(100px);
      background: #1a1a2e; color: white; padding: 14px 24px;
      border-radius: 14px; font-size: 0.88rem; font-weight: 600;
      display: flex; align-items: center; gap: 8px;
      box-shadow: 0 8px 32px rgba(0,0,0,0.3);
      transition: transform 0.3s ease; z-index: 10000;
    }
    .toast.show { transform: translateX(-50%) translateY(0); }
    .toast mat-icon { font-size: 20px; width: 20px; height: 20px; color: #00C853; }

    .spinner-sm { display: flex; gap: 4px; }
    .spinner-sm .dot { width: 6px; height: 6px; border-radius: 50%; background: white; animation: bounce 1.4s infinite ease-in-out both; }
    .spinner-sm .dot1 { animation-delay: -0.32s; }
    .spinner-sm .dot2 { animation-delay: -0.16s; }
    @keyframes bounce {
      0%, 80%, 100% { transform: scale(0); opacity: 0.5; }
      40% { transform: scale(1); opacity: 1; }
    }

    .fade-in { animation: fadeIn 0.3s ease; }
    @keyframes fadeIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }

    @media (max-width: 768px) {
      .header-inner { flex-direction: column; align-items: flex-start; gap: 12px; }
      .tabs-inner { overflow-x: auto; }
      .tab { white-space: nowrap; font-size: 0.78rem; }
      .user-card-details { flex-direction: column; gap: 8px; }
      .user-card-actions { flex-direction: column; }
      .action-btn { justify-content: center; }
    }
    @media (max-width: 480px) {
      .content-area { padding: 0 12px 40px; }
      .tabs-container { padding: 0 12px; }
      .header-inner { padding: 16px 12px 0; }
      .header-left h1 { font-size: 1.1rem; }
      .dialog-card { padding: 24px 20px; }
    }
  `]
})
export class AdminUsersComponent implements OnInit {
  users: any[] = [];
  filteredUsers: any[] = [];
  activeTab = 'all';
  isLoading = true;
  pendingCount = 0;

  // Confirm dialog
  showConfirmDialog = false;
  confirmDialogAction = '';
  confirmDialogUser: any = null;
  actionLoading = false;

  // Role dialog
  showRoleDialog = false;
  roleDialogUser: any = null;
  selectedRole = '';
  roles = [
    { value: 'Cliente', label: 'Cliente', desc: 'Acesso basico a conta', icon: 'person' },
    { value: 'Tecnico', label: 'Tecnico', desc: 'Acesso a ferramentas tecnicas', icon: 'build' },
    { value: 'Administrador', label: 'Administrador', desc: 'Acesso total ao sistema', icon: 'shield' }
  ];

  toastMsg = '';

  constructor(
    private userService: UserService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    if (!this.auth.isAdmin()) {
      this.router.navigate(['/dashboard']);
      return;
    }
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading = true;
    this.userService.getAll().subscribe({
      next: (res: any) => {
        this.users = Array.isArray(res) ? res : (res.users || res.data || []);
        this.pendingCount = this.users.filter((u: any) => this.isPending(u.status)).length;
        this.filterUsers();
        this.isLoading = false;
      },
      error: () => {
        this.users = [];
        this.filteredUsers = [];
        this.isLoading = false;
      }
    });
  }

  setTab(tab: string) {
    this.activeTab = tab;
    this.filterUsers();
  }

  filterUsers() {
    switch (this.activeTab) {
      case 'pending':
        this.filteredUsers = this.users.filter(u => this.isPending(u.status));
        break;
      case 'active':
        this.filteredUsers = this.users.filter(u => u.status === 'Active');
        break;
      case 'inactive':
        this.filteredUsers = this.users.filter(u => u.status === 'Inactive');
        break;
      default:
        this.filteredUsers = [...this.users];
    }
  }

  isPending(status: string): boolean {
    return status === 'PendingApproval' || status === 'PendingEmailConfirmation' || status === 'Pending';
  }

  getCountByStatus(status: string): number {
    return this.users.filter(u => u.status === status).length;
  }

  getStatusClass(status: string): string {
    if (this.isPending(status)) return 'pending';
    if (status === 'Active') return 'active';
    if (status === 'Inactive') return 'inactive';
    if (status === 'Rejected') return 'rejected';
    if (status === 'PendingEmailConfirmation') return 'email';
    return 'pending';
  }

  translateStatus(status: string): string {
    const map: Record<string, string> = {
      'Active': 'Ativo',
      'Inactive': 'Inativo',
      'PendingApproval': 'Pendente',
      'PendingEmailConfirmation': 'Aguardando Email',
      'Pending': 'Pendente',
      'Rejected': 'Rejeitado'
    };
    return map[status] || status;
  }

  getInitials(name: string): string {
    if (!name) return '?';
    return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
  }

  formatCpf(doc: string): string {
    if (!doc) return '---';
    const d = doc.replace(/\D/g, '');
    if (d.length === 11) return d.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    return doc;
  }

  formatDate(date: string): string {
    if (!date) return '---';
    try {
      return new Date(date).toLocaleDateString('pt-BR');
    } catch { return date; }
  }

  // Actions
  confirmAction(action: string, user: any) {
    this.confirmDialogAction = action;
    this.confirmDialogUser = user;
    this.showConfirmDialog = true;
  }

  getDialogIcon(): string {
    const icons: Record<string, string> = {
      'aprovar': 'check_circle',
      'rejeitar': 'cancel',
      'desativar': 'block',
      'ativar': 'check_circle'
    };
    return icons[this.confirmDialogAction] || 'help';
  }

  executeAction() {
    const user = this.confirmDialogUser;
    if (!user) return;
    this.actionLoading = true;
    const userId = user.id || user.userId;

    let action$;
    switch (this.confirmDialogAction) {
      case 'aprovar':
        action$ = this.userService.approve(userId);
        break;
      case 'rejeitar':
        action$ = this.userService.reject(userId);
        break;
      case 'desativar':
        action$ = this.userService.changeStatus(userId, false);
        break;
      case 'ativar':
        action$ = this.userService.changeStatus(userId, true);
        break;
      default:
        this.actionLoading = false;
        return;
    }

    action$.subscribe({
      next: () => {
        this.actionLoading = false;
        this.showConfirmDialog = false;
        this.showToast(`Usuario ${this.confirmDialogAction === 'aprovar' ? 'aprovado' :
          this.confirmDialogAction === 'rejeitar' ? 'rejeitado' :
          this.confirmDialogAction === 'desativar' ? 'desativado' : 'ativado'} com sucesso!`);
        this.loadUsers();
      },
      error: () => {
        this.actionLoading = false;
        this.showToast('Erro ao executar acao. Tente novamente.');
      }
    });
  }

  openRoleDialog(user: any) {
    this.roleDialogUser = user;
    this.selectedRole = user.role || 'Cliente';
    this.showRoleDialog = true;
  }

  saveRole() {
    if (!this.roleDialogUser || !this.selectedRole) return;
    this.actionLoading = true;
    const userId = this.roleDialogUser.id || this.roleDialogUser.userId;
    this.userService.changeRole(userId, this.selectedRole).subscribe({
      next: () => {
        this.actionLoading = false;
        this.showRoleDialog = false;
        this.showToast(`Role alterada para ${this.selectedRole} com sucesso!`);
        this.loadUsers();
      },
      error: () => {
        this.actionLoading = false;
        this.showToast('Erro ao alterar role. Tente novamente.');
      }
    });
  }

  closeDialogs() {
    this.showConfirmDialog = false;
    this.showRoleDialog = false;
    this.actionLoading = false;
  }

  showToast(msg: string) {
    this.toastMsg = msg;
    setTimeout(() => { this.toastMsg = ''; }, 3000);
  }

  goBack() { this.router.navigate(['/admin']); }
}
