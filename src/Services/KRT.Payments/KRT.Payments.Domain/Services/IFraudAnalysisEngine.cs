using KRT.Payments.Domain.Entities;

namespace KRT.Payments.Domain.Services;

public record FraudAnalysisResult(
    int Score,
    FraudDecision Decision,
    string Details,
    List<FraudRuleHit> RuleHits
);

public record FraudRuleHit(string RuleName, int Points, string Description);

public enum FraudDecision
{
    Approved,
    Rejected,
    UnderReview
}

public interface IFraudAnalysisEngine
{
    Task<FraudAnalysisResult> AnalyzeAsync(PixTransaction transaction, CancellationToken ct = default);
}
