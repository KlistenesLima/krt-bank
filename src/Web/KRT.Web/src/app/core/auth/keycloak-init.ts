import { KeycloakService } from 'keycloak-angular';
import { environment } from '../../../environments/environment';

/**
 * APP_INITIALIZER: conecta ao Keycloak antes do app carregar.
 * Se não conseguir conectar (Keycloak fora), loga no console e permite acesso anônimo.
 */
export function initializeKeycloak(keycloak: KeycloakService): () => Promise<boolean> {
  return () =>
    keycloak.init({
      config: {
        url: environment.keycloak.url,
        realm: environment.keycloak.realm,
        clientId: environment.keycloak.clientId
      },
      initOptions: {
        onLoad: 'check-sso',              // Não força login, só verifica se já logou
        silentCheckSsoRedirectUri:
          window.location.origin + '/assets/silent-check-sso.html',
        checkLoginIframe: false            // Evita erro de CORS em dev
      },
      enableBearerInterceptor: true,       // Anexa JWT automaticamente
      bearerPrefix: 'Bearer',
      bearerExcludedUrls: [
        '/assets',
        '/api/v1/accounts'                 // POST criar conta é público
      ]
    }).catch((err) => {
      console.warn('Keycloak não disponível. Modo anônimo.', err);
      return false;
    });
}
