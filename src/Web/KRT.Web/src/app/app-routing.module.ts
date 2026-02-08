import { ProfileComponent } from './pages/profile/profile.component';
import { BoletosComponent } from './pages/boletos/boletos.component';
import { ContactsComponent } from './pages/contacts/contacts.component';
import { NotificationCenterComponent } from './pages/notifications/notification-center.component';
import { ScheduledPixComponent } from './pages/scheduled-pix/scheduled-pix.component';
import { StatementComponent } from './pages/statement/statement.component';
import { DashboardChartsComponent } from './pages/dashboard-charts/dashboard-charts.component';
import { VirtualCardComponent } from './pages/virtual-card/virtual-card.component';
import { PixQrcodeComponent } from './pages/pix-qrcode/pix-qrcode.component';
// DEPRECATED - Routes definidas em app.module.ts
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

@NgModule({
  imports: [RouterModule.forRoot([])],
  exports: [RouterModule]
})
export class AppRoutingModule {}
