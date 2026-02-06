// === API Envelope (padrão do backend) ===
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  errors: string[];
  correlationId?: string;
}

// === Account ===
export interface AccountResponse {
  id: string;
  customerName: string;
  document: string;
  email: string;
  balance: number;
  status: string;
  type: string;
  currency: string;
}

export interface BalanceResponse {
  accountId: string;
  availableAmount: number;
  currency: string;
  updatedAt: string;
}

export interface CreateAccountRequest {
  customerName: string;
  customerDocument: string;
  customerEmail: string;
  branchCode: string;
}
