using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/statement")]
public class StatementController : ControllerBase
{
    private static readonly string[] Types = { "PIX_SENT", "PIX_RECEIVED", "TED_SENT", "TED_RECEIVED", "BOLETO", "CARD_PURCHASE", "REFUND" };
    private static readonly string[] Categories = { "Alimentacao", "Transporte", "Moradia", "Lazer", "Saude", "Educacao", "Servicos", "Outros" };
    private static readonly string[] Names = { "Maria Silva", "Joao Santos", "Supermercado ABC", "Farmacia Popular", "Posto Shell", "Netflix", "Uber", "iFood", "Aluguel", "Energia Eletrica" };

    /// <summary>
    /// Lista extrato com filtros, paginacao e ordenacao.
    /// </summary>
    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public IActionResult GetStatement(
        Guid accountId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? type,
        [FromQuery] string? category,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "date",
        [FromQuery] string sortOrder = "desc")
    {
        var transactions = GenerateMockTransactions(accountId, 200);

        // Aplicar filtros
        var filtered = transactions.AsEnumerable();

        if (startDate.HasValue)
            filtered = filtered.Where(t => t.Date >= startDate.Value);
        if (endDate.HasValue)
            filtered = filtered.Where(t => t.Date <= endDate.Value.AddDays(1));
        if (!string.IsNullOrEmpty(type))
            filtered = filtered.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(category))
            filtered = filtered.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (minAmount.HasValue)
            filtered = filtered.Where(t => Math.Abs(t.Amount) >= minAmount.Value);
        if (maxAmount.HasValue)
            filtered = filtered.Where(t => Math.Abs(t.Amount) <= maxAmount.Value);
        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(t => t.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                                        || t.CounterpartyName.Contains(search, StringComparison.OrdinalIgnoreCase));

        // Ordenar
        filtered = sortBy.ToLower() switch
        {
            "amount" => sortOrder == "asc" ? filtered.OrderBy(t => t.Amount) : filtered.OrderByDescending(t => t.Amount),
            "type" => sortOrder == "asc" ? filtered.OrderBy(t => t.Type) : filtered.OrderByDescending(t => t.Type),
            _ => sortOrder == "asc" ? filtered.OrderBy(t => t.Date) : filtered.OrderByDescending(t => t.Date)
        };

        var total = filtered.Count();
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var totalIncome = filtered.Where(t => t.Amount > 0).Sum(t => t.Amount);
        var totalExpenses = filtered.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));

        return Ok(new
        {
            accountId,
            page,
            pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling((double)total / pageSize),
            summary = new { totalIncome, totalExpenses, net = totalIncome - totalExpenses },
            items = items.Select(t => new
            {
                t.Id,
                t.Date,
                t.Type,
                t.Category,
                t.Amount,
                t.Description,
                t.CounterpartyName,
                isCredit = t.Amount > 0
            })
        });
    }

    /// <summary>
    /// Exporta extrato em CSV.
    /// </summary>
    [HttpGet("{accountId}/export/csv")]
    [AllowAnonymous]
    public IActionResult ExportCsv(
        Guid accountId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? type,
        [FromQuery] string? category)
    {
        var transactions = GenerateMockTransactions(accountId, 200).AsEnumerable();
        if (startDate.HasValue) transactions = transactions.Where(t => t.Date >= startDate.Value);
        if (endDate.HasValue) transactions = transactions.Where(t => t.Date <= endDate.Value.AddDays(1));
        if (!string.IsNullOrEmpty(type)) transactions = transactions.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(category)) transactions = transactions.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        var sb = new StringBuilder();
        sb.AppendLine("Data;Tipo;Categoria;Descricao;Favorecido;Valor");
        foreach (var t in transactions.OrderByDescending(t => t.Date))
        {
            sb.AppendLine($"{t.Date:dd/MM/yyyy HH:mm};{t.Type};{t.Category};{t.Description};{t.CounterpartyName};{t.Amount.ToString("F2", CultureInfo.GetCultureInfo("pt-BR"))}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"extrato-{accountId:N}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Exporta extrato em PDF.
    /// </summary>
    [HttpGet("{accountId}/export/pdf")]
    [AllowAnonymous]
    public IActionResult ExportPdf(
        Guid accountId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? type,
        [FromQuery] string? category)
    {
        var transactions = GenerateMockTransactions(accountId, 200).AsEnumerable();
        if (startDate.HasValue) transactions = transactions.Where(t => t.Date >= startDate.Value);
        if (endDate.HasValue) transactions = transactions.Where(t => t.Date <= endDate.Value.AddDays(1));
        if (!string.IsNullOrEmpty(type)) transactions = transactions.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(category)) transactions = transactions.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        var list = transactions.OrderByDescending(t => t.Date).ToList();
        var totalIn = list.Where(t => t.Amount > 0).Sum(t => t.Amount);
        var totalOut = list.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("KRT Bank").Bold().FontSize(18).FontColor("#1a237e");
                        row.RelativeItem().AlignRight().Text($"Extrato Bancario").FontSize(14);
                    });
                    col.Item().PaddingTop(5).Text($"Conta: {accountId}").FontSize(8).FontColor("#666");
                    col.Item().Text($"Periodo: {(startDate?.ToString("dd/MM/yyyy") ?? "inicio")} a {(endDate?.ToString("dd/MM/yyyy") ?? DateTime.UtcNow.ToString("dd/MM/yyyy"))}").FontSize(8).FontColor("#666");
                    col.Item().PaddingBottom(10).LineHorizontal(1).LineColor("#1a237e");
                });

                page.Content().Column(col =>
                {
                    // Resumo
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Background("#e8f5e9").Padding(8).Column(c =>
                        {
                            c.Item().Text("Entradas").FontSize(8).FontColor("#2e7d32");
                            c.Item().Text($"R$ {totalIn:N2}").Bold().FontColor("#2e7d32");
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Background("#ffebee").Padding(8).Column(c =>
                        {
                            c.Item().Text("Saidas").FontSize(8).FontColor("#c62828");
                            c.Item().Text($"R$ {totalOut:N2}").Bold().FontColor("#c62828");
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Background("#e3f2fd").Padding(8).Column(c =>
                        {
                            c.Item().Text("Saldo Periodo").FontSize(8).FontColor("#1565c0");
                            c.Item().Text($"R$ {(totalIn - totalOut):N2}").Bold().FontColor("#1565c0");
                        });
                    });

                    // Tabela
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(70);  // Data
                            cols.ConstantColumn(75);  // Tipo
                            cols.RelativeColumn();    // Descricao
                            cols.ConstantColumn(80);  // Valor
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#1a237e").Padding(4).Text("Data").FontColor("#fff").Bold();
                            header.Cell().Background("#1a237e").Padding(4).Text("Tipo").FontColor("#fff").Bold();
                            header.Cell().Background("#1a237e").Padding(4).Text("Descricao").FontColor("#fff").Bold();
                            header.Cell().Background("#1a237e").Padding(4).AlignRight().Text("Valor").FontColor("#fff").Bold();
                        });

                        var idx = 0;
                        foreach (var t in list.Take(100))
                        {
                            var bg = idx++ % 2 == 0 ? "#fff" : "#f5f5f5";
                            var color = t.Amount >= 0 ? "#2e7d32" : "#c62828";
                            table.Cell().Background(bg).Padding(3).Text(t.Date.ToString("dd/MM HH:mm"));
                            table.Cell().Background(bg).Padding(3).Text(t.Type).FontSize(8);
                            table.Cell().Background(bg).Padding(3).Text($"{t.CounterpartyName} - {t.Description}").FontSize(8);
                            table.Cell().Background(bg).Padding(3).AlignRight().Text($"R$ {t.Amount:N2}").FontColor(color);
                        }
                    });

                    if (list.Count > 100)
                        col.Item().PaddingTop(5).Text($"Mostrando 100 de {list.Count} transacoes.").FontSize(8).Italic();
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("KRT Bank - Gerado em ");
                    x.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")).Bold();
                    x.Span(" | Pagina ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"extrato-{accountId:N}-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    // Mock data generator (deterministic per account)
    private List<TransactionItem> GenerateMockTransactions(Guid accountId, int count)
    {
        var rng = new Random(accountId.GetHashCode());
        return Enumerable.Range(0, count).Select(i =>
        {
            var type = Types[rng.Next(Types.Length)];
            var isCredit = type.Contains("RECEIVED") || type == "REFUND";
            var amount = Math.Round((decimal)(rng.Next(10, 5000) + rng.NextDouble()), 2);

            return new TransactionItem
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow.AddDays(-rng.Next(0, 90)).AddHours(-rng.Next(0, 24)).AddMinutes(-rng.Next(0, 60)),
                Type = type,
                Category = Categories[rng.Next(Categories.Length)],
                Amount = isCredit ? amount : -amount,
                Description = type switch
                {
                    "PIX_SENT" => "Pix enviado",
                    "PIX_RECEIVED" => "Pix recebido",
                    "TED_SENT" => "TED enviada",
                    "TED_RECEIVED" => "TED recebida",
                    "BOLETO" => "Pagamento boleto",
                    "CARD_PURCHASE" => "Compra cartao",
                    "REFUND" => "Estorno",
                    _ => "Transacao"
                },
                CounterpartyName = Names[rng.Next(Names.Length)]
            };
        }).OrderByDescending(t => t.Date).ToList();
    }

    private class TransactionItem
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
        public string Description { get; set; } = "";
        public string CounterpartyName { get; set; } = "";
    }
}