import { ToastComponent } from './shared/components/toast/toast.component';
import { ConnectionStatusComponent } from './shared/components/connection-status/connection-status.component';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: '<router-outlet></router-outlet>'
})
export class AppComponent {
  title = 'KRT Bank';
}
