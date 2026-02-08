// account.model.ts - Modelos alinhados com o backend

export interface AccountResponse {
  id: string;
  customerName: string;
  document: string;
  email: string;
  balance: number;
  status: string;
  type: string;
}

export interface BalanceResponse {
  accountId: string;
  availableAmount: number;
}

export interface CreateAccountRequest {
  customerName: string;
  customerDocument: string;
  customerEmail: string;
  customerPhone: string;
  password: string;
  branchCode: string;
}

// DEPRECATED: ApiResponse wrapper removido na Parte 11
// O backend retorna objetos diretamente, sem wrapper { success, data }
export interface ApiResponse<T> {
  data: T;
}