import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
public class AccountService {
  constructor(private http: HttpClient) {}

  createAccount(data: any) {
    return this.http.post(\\/accounts\, data);
  }
}
