import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { KeycloakAuthGuard, KeycloakService } from 'keycloak-angular';

@Injectable({ providedIn: 'root' })
export class AuthGuard extends KeycloakAuthGuard {
  constructor(
    protected override readonly router: Router,
    protected readonly keycloak: KeycloakService
  ) {
    super(router, keycloak);
  }

  async isAccessAllowed(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Promise<boolean | UrlTree> {
    if (!this.authenticated) {
      // Redireciona para o Keycloak login
      await this.keycloak.login({
        redirectUri: window.location.origin + state.url
      });
      return false;
    }

    // Verificar roles se a rota exigir
    const requiredRoles = route.data['roles'];
    if (!requiredRoles || requiredRoles.length === 0) {
      return true;
    }

    return requiredRoles.every((role: string) => this.roles.includes(role));
  }
}
