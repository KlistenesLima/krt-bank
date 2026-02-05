import { Component } from '@angular/core';
import { Location } from '@angular/common';

@Component({
  selector: 'app-investments',
  template: `
    <div class="app-layout">
      <header class="header-simple"><button mat-icon-button (click)="back()"><mat-icon>arrow_back</mat-icon></button><h1>Investimentos</h1><div style="width:40px"></div></header>
      <main class="container">
        <mat-card>
            <h2>Meu Cofre</h2>
            <h1>R$ 12.500,00</h1>
            <p>Rendendo 100% do CDI</p>
        </mat-card>
      </main>
    </div>
  `,
  styles: [`.header-simple { display: flex; align-items: center; justify-content: space-between; padding: 15px; background: #fff; }`]
})
export class InvestmentsPageComponent {
  constructor(private location: Location) {}
  back() { this.location.back(); }
}
