import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-pix-area',
  template: '<p>Redirecionando...</p>'
})
export class PixAreaComponent {
  constructor(private router: Router) { this.router.navigate(['/pix']); }
}
