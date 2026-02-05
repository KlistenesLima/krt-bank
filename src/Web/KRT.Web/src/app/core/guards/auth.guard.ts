import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(): boolean | UrlTree {
    const token = localStorage.getItem('krt_account_id');
    if (token) {
      return true;
    }
    // Se não tiver login, manda pro login
    return this.router.createUrlTree(['/login']);
  }
}
