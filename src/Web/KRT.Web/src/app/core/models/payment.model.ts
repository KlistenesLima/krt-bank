// === Pix Transfer (ProcessPixCommand no backend) ===
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
  errors?: string[];
}

export interface TransactionHistory {
  id: string;
  sourceAccountId: string;
  destinationAccountId: string;
  amount: number;
  pixKey: string;
  status: string;
  description?: string;
  createdAt: string;
  completedAt?: string;
}

export interface TransactionDetail {
  id: string;
  sourceAccountId: string;
  destinationAccountId: string;
  amount: number;
  currency: string;
  pixKey: string;
  status: string;
  description?: string;
  failureReason?: string;
  sourceDebited: boolean;
  destinationCredited: boolean;
  createdAt: string;
  completedAt?: string;
}
