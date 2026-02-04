import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule, Routes } from '@angular/router';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule } from '@angular/material/snack-bar';

// Pages
import { AppComponent } from './app.component';
import { LoginComponent } from './modules/auth/pages/login/login.component';
import { CreateAccountComponent } from './modules/onboarding/pages/create-account/create-account.component';
import { DashboardPageComponent } from './modules/dashboard/pages/dashboard-page.component';
import { StatementPageComponent } from './modules/statement/pages/statement-page.component';
import { PixPageComponent } from './modules/payments/pages/pix/pix-page.component';
import { ProfilePageComponent } from './modules/profile/pages/profile-page.component';
import { CardsPageComponent } from './modules/cards/pages/cards-page.component';
import { ReceiptComponent } from './modules/statement/pages/receipt/receipt.component';
import { PaymentSuccessComponent } from './shared/pages/success/payment-success.component';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: CreateAccountComponent },
  { path: 'dashboard', component: DashboardPageComponent },
  { path: 'extract', component: StatementPageComponent },
  { path: 'pix', component: PixPageComponent },
  { path: 'profile', component: ProfilePageComponent },
  { path: 'cards', component: CardsPageComponent },         // Nova Rota
  { path: 'receipt/:id', component: ReceiptComponent },     // Nova Rota
  { path: 'success', component: PaymentSuccessComponent }   // Nova Rota
];

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    CreateAccountComponent,
    DashboardPageComponent,
    StatementPageComponent,
    PixPageComponent,
    ProfilePageComponent,
    CardsPageComponent,
    ReceiptComponent,
    PaymentSuccessComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    BrowserAnimationsModule,
    RouterModule.forRoot(routes),
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
