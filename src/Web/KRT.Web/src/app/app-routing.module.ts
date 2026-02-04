import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateAccountComponent } from './modules/onboarding/pages/create-account/create-account.component';
import { DashboardPageComponent } from './modules/dashboard/pages/dashboard-page.component';
import { PixAreaComponent } from './modules/payments/components/pix-area.component';

const routes: Routes = [
  { path: '', component: CreateAccountComponent }, // Home é criar conta
  { path: 'dashboard', component: DashboardPageComponent },
  { path: 'pix', component: PixAreaComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
