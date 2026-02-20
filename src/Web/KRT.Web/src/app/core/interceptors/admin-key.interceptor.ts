import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable()
export class AdminKeyInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url.includes('/admin') && environment.adminApiKey) {
      req = req.clone({ setHeaders: { 'X-Admin-Key': environment.adminApiKey } });
    }
    return next.handle(req);
  }
}
