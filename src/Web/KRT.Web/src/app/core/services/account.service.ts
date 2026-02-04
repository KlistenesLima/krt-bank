import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private apiUrl = 'http://localhost:5000/api/v1/accounts';

  constructor(private http: HttpClient) {}

  create(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }

  getById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  getBalance(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/balance`);
  }

  getStatement(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/statement`);
  }

  // O MÉTODO QUE FALTAVA
  performPix(accountId: string, pixKey: string, amount: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${accountId}/pix`, { pixKey, amount });
  }
}
