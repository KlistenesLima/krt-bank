import { ThemeToggleComponent } from './shared/components/theme-toggle/theme-toggle.component';
import { ToastComponent } from './shared/components/toast/toast.component';
import { ConnectionStatusComponent } from './shared/components/connection-status/connection-status.component';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AuthInterceptor } from './core/interceptors/auth.interceptor';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSliderModule } from '@angular/material/slider';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatBadgeModule } from '@angular/material/badge';

// Components
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
import { MyDataComponent } from './modules/profile/pages/my-data/my-data.component';
import { SecurityComponent } from './modules/profile/pages/security/security.component';
import { NotificationsComponent } from './modules/profile/pages/notifications/notifications.component';
import { InvestmentsPageComponent } from './modules/investments/pages/investments-page.component';
import { RechargeComponent } from './modules/payments/pages/recharge/recharge.component';
import { InboxPageComponent } from './modules/notifications/pages/inbox/inbox-page.component';
import { ChatDialogComponent } from './shared/components/chat-dialog/chat-dialog.component';
import { PixAreaComponent } from './modules/payments/components/pix-area.component';

// Guard
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: CreateAccountComponent },

  // Rotas protegidas (AuthGuard verifica token JWT)
  { path: 'dashboard', component: DashboardPageComponent, canActivate: [AuthGuard] },
  { path: 'extract', component: StatementPageComponent, canActivate: [AuthGuard] },
  { path: 'pix', component: PixPageComponent, canActivate: [AuthGuard] },
  { path: 'pix/keys', component: PixKeysComponent, canActivate: [AuthGuard] },
  { path: 'boleto', component: BoletoComponent, canActivate: [AuthGuard] },
  { path: 'recharge', component: RechargeComponent, canActivate: [AuthGuard] },
  { path: 'profile', component: ProfilePageComponent, canActivate: [AuthGuard] },
  { path: 'profile/data', component: MyDataComponent, canActivate: [AuthGuard] },
  { path: 'profile/security', component: SecurityComponent, canActivate: [AuthGuard] },
  { path: 'profile/notifications', component: NotificationsComponent, canActivate: [AuthGuard] },
  { path: 'cards', component: CardsPageComponent, canActivate: [AuthGuard] },
  { path: 'receipt/:id', component: ReceiptComponent, canActivate: [AuthGuard] },
  { path: 'investments', component: InvestmentsPageComponent, canActivate: [AuthGuard] },
  { path: 'inbox', component: InboxPageComponent, canActivate: [AuthGuard] },
  { path: 'success', component: PaymentSuccessComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: 'login' }
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
    NotificationsComponent,
    InvestmentsPageComponent,
    RechargeComponent,
    InboxPageComponent,
    ChatDialogComponent,
    PixAreaComponent
  ],
  imports: [
    ThemeToggleComponent,
    ToastComponent,
    ConnectionStatusComponent,
    BrowserModule,
    FormsModule,
    HttpClientModule,
    BrowserAnimationsModule,
    RouterModule.forRoot(routes),
    // REMOVIDO: KeycloakAngularModule (conflitava com AuthInterceptor JWT)
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatDividerModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatSliderModule,
    MatChipsModule,
    MatProgressBarModule,
    MatBadgeModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
    // REMOVIDO: APP_INITIALIZER + KeycloakService (agora usamos JWT puro)
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}


