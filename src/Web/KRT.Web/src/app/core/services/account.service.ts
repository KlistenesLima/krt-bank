import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccountResponse, CreateAccountRequest } from '../models/account.model';

@Injectable({ providedIn: 'root' })
export class AccountService {
    private apiUrl = 'http://localhost:5000/api/v1/accounts';

    constructor(private http: HttpClient) {}

    create(request: CreateAccountRequest): Observable<AccountResponse> {
        return this.http.post<AccountResponse>(this.apiUrl, request);
    }

    getById(id: string): Observable<AccountResponse> {
        return this.http.get<AccountResponse>(`${this.apiUrl}/${id}`);
    }

    getBalance(id: string): Observable<any> {
        return this.http.get(`${this.apiUrl}/${id}/balance`);
    }

    getStatement(id: string): Observable<any> {
        return this.http.get(`${this.apiUrl}/${id}/statement`);
    }
}
