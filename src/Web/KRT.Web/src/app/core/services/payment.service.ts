import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface PixRequest { accountId: string; key: string; amount: number; }

@Injectable({ providedIn: 'root' })
public class PaymentService {
  constructor(private http: HttpClient) {}

  sendPix(data: PixRequest) {
    return this.http.post(\\/pix\, data);
  }
}
