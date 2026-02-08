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
