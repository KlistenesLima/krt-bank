import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PixTransferRequest, PixTransferResponse, TransactionHistory, TransactionDetail } from '../models/payment.model';
import { ApiResponse } from '../models/account.model';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private baseUrl = `${environment.apiUrl}/pix`;

  constructor(private http: HttpClient) {}

  /** POST /api/v1/pix/transfer — Saga Orchestrator */
  sendPix(request: PixTransferRequest): Observable<PixTransferResponse> {
    return this.http.post<PixTransferResponse>(`${this.baseUrl}/transfer`, request);
  }

  /** GET /api/v1/pix/history/{accountId} — Histórico */
  getHistory(accountId: string, page = 1, pageSize = 20): Observable<{ data: TransactionHistory[] }> {
    return this.http.get<{ data: TransactionHistory[] }>(
      `${this.baseUrl}/history/${accountId}?page=${page}&pageSize=${pageSize}`
    );
  }

  /** GET /api/v1/pix/{id} — Detalhe de uma transação */
  getById(id: string): Observable<ApiResponse<TransactionDetail>> {
    return this.http.get<ApiResponse<TransactionDetail>>(`${this.baseUrl}/${id}`);
  }
}
