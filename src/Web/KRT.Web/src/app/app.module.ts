import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule, Routes } from '@angular/router';

// Angular Material Imports
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSliderModule } from '@angular/material/slider'; // NOVO

// Components Imports
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
import { BoletoComponent } from './modules/payments/pages/boleto/boleto.component';
import { PixKeysComponent } from './modules/payments/pages/pix-keys/pix-keys.component';
import { BottomNavComponent } from './shared/components/bottom-nav/bottom-nav.component';
// NOVOS COMPONENTES
import { MyDataComponent } from './modules/profile/pages/my-data/my-data.component';
import { SecurityComponent } from './modules/profile/pages/security/security.component';
import { NotificationsComponent } from './modules/profile/pages/notifications/notifications.component';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: CreateAccountComponent },
  { path: 'dashboard', component: DashboardPageComponent },
  { path: 'extract', component: StatementPageComponent },
  { path: 'pix', component: PixPageComponent },
  { path: 'pix/keys', component: PixKeysComponent },
  { path: 'boleto', component: BoletoComponent },
  { path: 'profile', component: ProfilePageComponent },
  // NOVAS ROTAS
  { path: 'profile/data', component: MyDataComponent },
  { path: 'profile/security', component: SecurityComponent },
  { path: 'profile/notifications', component: NotificationsComponent },
  
  { path: 'cards', component: CardsPageComponent },
  { path: 'receipt/:id', component: ReceiptComponent },
  { path: 'success', component: PaymentSuccessComponent }
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
    PaymentSuccessComponent,
    BoletoComponent,
    PixKeysComponent,
    BottomNavComponent,
    MyDataComponent,
    SecurityComponent,
    NotificationsComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    BrowserAnimationsModule,
    RouterModule.forRoot(routes),
    // Material Modules
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatDividerModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatSliderModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }

