export interface AccountResponse {
    accountId: string;
    customerName: string;
    customerDocument: string;
    balance: number;
    accountNumber: string;
    status: string;
}

export interface Transaction {
    id: string;
    amount: number;
    type: 'DEBIT' | 'CREDIT';
    description: string;
    createdAt: Date;
}

export interface CreateAccountRequest {
    customerName: string;
    customerDocument: string;
    customerEmail: string;
    branchCode: string; // <--- NOVO CAMPO OBRIGATÓRIO
}
