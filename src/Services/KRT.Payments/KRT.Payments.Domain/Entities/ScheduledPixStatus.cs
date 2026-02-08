namespace KRT.Payments.Domain.Entities;

public enum ScheduledPixStatus
{
    Pending,     // Aguardando execucao
    Executed,    // Executado com sucesso
    Failed,      // Falhou na execucao
    Cancelled,   // Cancelado pelo usuario
    Paused       // Pausado (recorrente)
}