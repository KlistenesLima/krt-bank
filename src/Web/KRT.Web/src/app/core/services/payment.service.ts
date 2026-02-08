import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PixTransferRequest {
  sourceAccountId: string;
  destinationAccountId: string;
  pixKey: string;
  amount: number;
  description?: string;
  idempotencyKey: string;
}

export interface PixTransferResponse {
  success: boolean;
  transactionId: string;
  status: string;
  message: string;
}

export interface PixTransaction {
  transactionId: string;
  sourceAccountId: string;
  destinationAccountId: string;
  amount: number;
  status: string;
  fraudScore: number | null;
  description: string | null;
  createdAt: string;
  completedAt: string | null;
}

export interface PixTransactionDetail {
  transactionId: string;
  sourceAccountId: string;
  destinationAccountId: string;
  amount: number;
  currency: string;
  pixKey: string;
  status: string;
  description: string | null;
  failureReason: string | null;
  createdAt: string;
  completedAt: string | null;
  fraud: {
    score: number | null;
    details: string | null;
    analyzedAt: string | null;
  };
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private baseUrl = `${environment.apiUrl}/pix`;

  constructor(private http: HttpClient) {}

  /** POST /api/v1/pix — Inicia Pix (202 Accepted, anti-fraude async) */
  sendPix(request: PixTransferRequest): Observable<PixTransferResponse> {
    return this.http.post<PixTransferResponse>(this.baseUrl, request);
  }

  /** GET /api/v1/pix/{id} — Status + fraud score */
  getById(id: string): Observable<PixTransactionDetail> {
    return this.http.get<PixTransactionDetail>(`${this.baseUrl}/${id}`);
  }

  /** GET /api/v1/pix/account/{accountId} — Extrato Pix (paginado) */
  getHistory(accountId: string, page = 1, pageSize = 20): Observable<PixTransaction[]> {
    return this.http.get<PixTransaction[]>(
      `${this.baseUrl}/account/${accountId}?page=${page}&pageSize=${pageSize}`
    );
  }
}