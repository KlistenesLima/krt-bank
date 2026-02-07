using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Domain.Services;
using Microsoft.Extensions.Logging;

namespace KRT.Payments.Application.Services;

/// <summary>
/// Engine de análise de fraude baseada em regras com scoring.
/// Score < 40 = Aprovado
/// Score 40-70 = Revisão Manual
/// Score > 70 = Rejeitado
/// </summary>
public class FraudAnalysisEngine : IFraudAnalysisEngine
{
    private readonly IPixTransactionRepository _repository;
    private readonly ILogger<FraudAnalysisEngine> _logger;

    // Thresholds configuráveis
    private const int ApprovalThreshold = 40;
    private const int ReviewThreshold = 70;

    public FraudAnalysisEngine(
        IPixTransactionRepository repository,
        ILogger<FraudAnalysisEngine> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<FraudAnalysisResult> AnalyzeAsync(PixTransaction tx, CancellationToken ct = default)
    {
        var hits = new List<FraudRuleHit>();
        var totalScore = 0;

        _logger.LogInformation(
            "🔍 [Fraud] Analisando Pix {TxId}: R${Amount} de {Source} para {Dest}",
            tx.Id, tx.Amount, tx.SourceAccountId, tx.DestinationAccountId);

        // === REGRA 1: Valor alto (> R$5.000 = +30, > R$10.000 = +50) ===
        if (tx.Amount > 10_000)
        {
            var hit = new FraudRuleHit("HIGH_VALUE_CRITICAL", 50, $"Valor muito alto: R${tx.Amount:N2}");
            hits.Add(hit);
            totalScore += hit.Points;
        }
        else if (tx.Amount > 5_000)
        {
            var hit = new FraudRuleHit("HIGH_VALUE", 30, $"Valor alto: R${tx.Amount:N2}");
            hits.Add(hit);
            totalScore += hit.Points;
        }

        // === REGRA 2: Horário suspeito (00:00 - 06:00) = +20 ===
        var hour = DateTime.UtcNow.AddHours(-3).Hour; // UTC-3 (Brasil)
        if (hour >= 0 && hour < 6)
        {
            var hit = new FraudRuleHit("SUSPICIOUS_HOUR", 20, $"Transação em horário suspeito: {hour}h");
            hits.Add(hit);
            totalScore += hit.Points;
        }

        // === REGRA 3: Auto-transferência (origem = destino) = +80 ===
        if (tx.SourceAccountId == tx.DestinationAccountId)
        {
            var hit = new FraudRuleHit("SELF_TRANSFER", 80, "Transferência para a própria conta");
            hits.Add(hit);
            totalScore += hit.Points;
        }

        // === REGRA 4: Frequência alta (>3 Pix na última hora) = +40 ===
        var recentTxs = await _repository.GetByAccountIdAsync(tx.SourceAccountId, page: 1, pageSize: 10);
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentCount = recentTxs.Count(t => t.CreatedAt >= oneHourAgo && t.Id != tx.Id);
        if (recentCount >= 3)
        {
            var hit = new FraudRuleHit("HIGH_FREQUENCY", 40, $"{recentCount + 1} transações na última hora");
            hits.Add(hit);
            totalScore += hit.Points;
        }
        else if (recentCount >= 2)
        {
            var hit = new FraudRuleHit("MODERATE_FREQUENCY", 15, $"{recentCount + 1} transações na última hora");
            hits.Add(hit);
            totalScore += hit.Points;
        }

        // === REGRA 5: Mesmo destino repetido (>2x na última hora) = +35 ===
        var sameDestCount = recentTxs.Count(t =>
            t.DestinationAccountId == tx.DestinationAccountId &&
            t.CreatedAt >= oneHourAgo && t.Id != tx.Id);
        if (sameDestCount >= 2)
        {
            var hit = new FraudRuleHit("REPEATED_DESTINATION", 35,
                $"Mesmo destino {sameDestCount + 1}x na última hora");
            hits.Add(hit);
            totalScore += hit.Points;
        }

        // === REGRA 6: Valor redondo exato (ex: 1000, 5000) = +10 ===
        if (tx.Amount >= 1000 && tx.Amount % 1000 == 0)
        {
            var hit = new FraudRuleHit("ROUND_AMOUNT", 10, $"Valor redondo: R${tx.Amount:N2}");
            hits.Add(hit);
            totalScore += hit.Points;
        }

        // === REGRA 7: Valor muito pequeno e repetido (possível teste de cartão) = +25 ===
        if (tx.Amount <= 1 && recentCount >= 1)
        {
            var hit = new FraudRuleHit("MICRO_TRANSACTION_PATTERN", 25,
                "Micro-transação repetida (possível teste)");
            hits.Add(hit);
            totalScore += hit.Points;
        }

        // Determina decisão
        FraudDecision decision;
        if (totalScore > ReviewThreshold)
            decision = FraudDecision.Rejected;
        else if (totalScore >= ApprovalThreshold)
            decision = FraudDecision.UnderReview;
        else
            decision = FraudDecision.Approved;

        var details = hits.Count > 0
            ? string.Join(" | ", hits.Select(h => $"{h.RuleName}(+{h.Points})"))
            : "Nenhuma regra acionada";

        _logger.LogInformation(
            "🔍 [Fraud] Pix {TxId}: Score={Score}, Decisão={Decision}, Regras=[{Details}]",
            tx.Id, totalScore, decision, details);

        return new FraudAnalysisResult(totalScore, decision, details, hits);
    }
}
