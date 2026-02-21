import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<any> {
    return this.http.get(this.apiUrl);
  }

  getPending(): Observable<any> {
    return this.http.get(`${this.apiUrl}/pending`);
  }

  approve(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/approve`, {});
  }

  reject(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/reject`, {});
  }

  changeRole(id: string, newRole: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/role`, { newRole });
  }

  changeStatus(id: string, activate: boolean): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/status`, { activate });
  }
}
