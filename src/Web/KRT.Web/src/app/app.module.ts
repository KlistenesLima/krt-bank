import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

// Components
import { CreateAccountComponent } from './modules/onboarding/pages/create-account/create-account.component';
import { DashboardPageComponent } from './modules/dashboard/pages/dashboard-page.component';
import { PixAreaComponent } from './modules/payments/components/pix-area.component';

@NgModule({
  declarations: [
    AppComponent,
    CreateAccountComponent,
    DashboardPageComponent,
    PixAreaComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule, // CRÍTICO PARA CHAMAR API
    FormsModule       // PARA OS FORMS
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
