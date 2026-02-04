export interface PixRequest {
    accountId: string;
    receiverKey: string;
    amount: number;
}

export interface PaymentResponse {
    id: string;
    status: string;
    transactionId?: string;
}
