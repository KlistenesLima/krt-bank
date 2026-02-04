import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentResponse, PixRequest } from '../models/payment.model';

@Injectable({ providedIn: 'root' })
export class PaymentService {
    private apiUrl = 'http://localhost:5001/api/v1/payments';

    constructor(private http: HttpClient) {}

    sendPix(request: PixRequest): Observable<PaymentResponse> {
        return this.http.post<PaymentResponse>(`${this.apiUrl}/pix`, request);
    }
}
