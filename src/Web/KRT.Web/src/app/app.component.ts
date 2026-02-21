import { Component } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

@Component({
  selector: 'app-root',
  template: `
    <app-demo-banner></app-demo-banner>
    <router-outlet></router-outlet>
    <app-footer *ngIf="showFooter"></app-footer>
    <button class="chat-fab" *ngIf="showFab" (click)="chatOpen = !chatOpen">
      <mat-icon>{{ chatOpen ? 'close' : 'chat' }}</mat-icon>
    </button>
    <app-chat-dialog *ngIf="chatOpen" (closeChat)="chatOpen = false" class="chat-float"></app-chat-dialog>
  `,
  styles: [`
    .chat-fab {
      position: fixed; bottom: 24px; right: 24px; z-index: 1000;
      width: 56px; height: 56px; border-radius: 50%; border: none;
      background: linear-gradient(135deg, #0052D4, #0047BB); color: white;
      box-shadow: 0 4px 12px rgba(0,0,0,0.3); cursor: pointer;
      display: flex; align-items: center; justify-content: center;
    }
    .chat-fab:hover { transform: scale(1.1); }
    .chat-float { position: fixed; bottom: 90px; right: 24px; z-index: 999; }
  `]
})
export class AppComponent {
  chatOpen = false;
  showFab = false;
  showFooter = false;

  private footerRoutes = ['/about', '/resume', '/docs', '/login', '/register', '/forgot-password', '/portfolio'];
  private noFabRoutes = ['/login', '/register', '/forgot-password'];

  constructor(private router: Router) {
    this.router.events.subscribe(e => {
      if (e instanceof NavigationEnd) {
        const url = e.urlAfterRedirects;
        this.showFab = !this.noFabRoutes.includes(url);
        if (!this.showFab) this.chatOpen = false;
        this.showFooter = this.footerRoutes.includes(url);
      }
    });
  }
}
