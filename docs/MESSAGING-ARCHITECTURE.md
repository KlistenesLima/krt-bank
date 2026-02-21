# KRT Bank - Arquitetura de Mensageria: Kafka + RabbitMQ

## Por que dois brokers?

| Aspecto               | Kafka (Event Bus)                | RabbitMQ (Message Bus)              |
|-----------------------|----------------------------------|-------------------------------------|
| **Papel**             | Event Streaming                  | Task Queue                          |
| **Semantica**         | "O que aconteceu" (fato)         | "O que precisa ser feito" (tarefa)  |
| **Retencao**          | Imutavel, configuravel (7 anos)  | Deletado apos consumo (ack)         |
| **Replay**            | Sim (reset de offset)            | Nao                                 |
| **Consumer Groups**   | Multiplos (cada um recebe tudo)  | Competicao (1 consumer por msg)     |
| **Prioridade**        | Nao                              | Sim (0-9)                           |
| **Dead Letter Queue** | Requer implementacao custom      | Nativo                              |
| **Fair Dispatch**     | Nao (particoes)                  | Sim (prefetchCount)                 |

## Topicos Kafka (6)
- krt.pix.transfer-initiated
- krt.fraud.analysis-approved / rejected / review
- krt.pix.transfer-completed / failed
- krt.accounts.created / debited / credited

## Filas RabbitMQ (5+1)
- krt.notifications.email (TTL 5min, priority 0-9)
- krt.notifications.sms (TTL 5min, priority 0-9)
- krt.notifications.push
- krt.receipts.generate (TTL 10min, priority 0-9)
- krt.receipts.upload (TTL 10min, priority 0-9)
- krt.dead-letters (DLQ)

## Numeros
- 5 Kafka consumers (3 negocio + 2 auditoria)
- 2 RabbitMQ workers (NotificationWorker + ReceiptWorker)
- Outbox Pattern garantindo zero perda de eventos
- Saga Pattern com compensacao automatica
