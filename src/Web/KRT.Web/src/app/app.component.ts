import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: \
    <nav style='background: #004d40; padding: 1rem; color: white; display: flex; gap: 20px;'>
      <h1 style='margin:0'>KRT Bank Enterprise</h1>
      <a routerLink='/dashboard' style='color: white; text-decoration: none; align-self: center'>Dashboard</a>
      <a routerLink='/pix' style='color: white; text-decoration: none; align-self: center'>Área Pix</a>
    </nav>
    <div style='padding: 20px'>
      <router-outlet></router-outlet>
    </div>
  \
})
export class AppComponent {}
