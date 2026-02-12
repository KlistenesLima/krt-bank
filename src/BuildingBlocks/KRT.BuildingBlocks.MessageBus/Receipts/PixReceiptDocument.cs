using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KRT.BuildingBlocks.MessageBus.Receipts;

/// <summary>
/// Documento PDF profissional de comprovante PIX usando QuestPDF.
/// 
/// Design inspirado em comprovantes de bancos digitais brasileiros (Nubank, BTG, Inter).
/// Usa layout clean com hierarquia visual clara e todas as informações obrigatórias
/// conforme regulamentação BACEN para comprovantes de transferência PIX.
/// 
/// QuestPDF usa licença Community (gratuita) para receita < $1M/ano.
/// </summary>
public class PixReceiptDocument : IDocument
{
    private readonly GenerateReceiptMessage _data;

    // Cores do KRT Bank
    private static readonly string PrimaryColor = "#1A1A2E";    // Azul escuro
    private static readonly string AccentColor = "#16213E";      // Azul médio
    private static readonly string SuccessColor = "#00B894";     // Verde sucesso
    private static readonly string TextLight = "#636E72";        // Cinza texto
    private static readonly string BorderColor = "#DFE6E9";      // Cinza borda

    public PixReceiptDocument(GenerateReceiptMessage data)
    {
        _data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.MarginHorizontal(50);
            page.MarginVertical(40);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(PrimaryColor));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // Logo / Nome do banco
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("KRT BANK")
                        .FontSize(24)
                        .Bold()
                        .FontColor(PrimaryColor);
                    col.Item().Text("Banco Digital")
                        .FontSize(10)
                        .FontColor(TextLight);
                });

                row.ConstantItem(120).AlignRight().Column(col =>
                {
                    col.Item().Text("COMPROVANTE")
                        .FontSize(12)
                        .Bold()
                        .FontColor(AccentColor);
                    col.Item().Text("Transferência PIX")
                        .FontSize(9)
                        .FontColor(TextLight);
                });
            });

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(BorderColor);

            // Badge de status
            column.Item().PaddingVertical(5).AlignCenter().Row(row =>
            {
                row.AutoItem()
                    .Background(SuccessColor)
                    .Padding(6)
                    .PaddingHorizontal(20)
                    .Text("✓ TRANSFERÊNCIA REALIZADA COM SUCESSO")
                    .FontSize(10)
                    .Bold()
                    .FontColor("#FFFFFF");
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(15).Column(column =>
        {
            // ═══════════════════════════════════════════
            // VALOR DA TRANSAÇÃO (destaque principal)
            // ═══════════════════════════════════════════
            column.Item().PaddingVertical(10).AlignCenter().Column(val =>
            {
                val.Item().AlignCenter().Text("Valor transferido")
                    .FontSize(10).FontColor(TextLight);
                val.Item().AlignCenter().Text($"R$ {_data.Amount:N2}")
                    .FontSize(32).Bold().FontColor(PrimaryColor);
                val.Item().AlignCenter().Text($"{_data.CompletedAt:dd 'de' MMMM 'de' yyyy 'às' HH:mm:ss}")
                    .FontSize(9).FontColor(TextLight);
            });

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(BorderColor);

            // ═══════════════════════════════════════════
            // DADOS DA TRANSAÇÃO
            // ═══════════════════════════════════════════
            column.Item().PaddingVertical(8).Text("DADOS DA TRANSAÇÃO")
                .FontSize(11).Bold().FontColor(AccentColor);

            column.Item().Element(c => ComposeInfoRow(c, "ID da Transação", _data.TransactionId.ToString()));
            column.Item().Element(c => ComposeInfoRow(c, "Tipo", _data.TransactionType));
            column.Item().Element(c => ComposeInfoRow(c, "Moeda", _data.Currency));
            column.Item().Element(c => ComposeInfoRow(c, "Data/Hora", _data.CompletedAt.ToString("dd/MM/yyyy HH:mm:ss")));

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(BorderColor);

            // ═══════════════════════════════════════════
            // ORIGEM
            // ═══════════════════════════════════════════
            column.Item().PaddingVertical(8).Text("ORIGEM (Pagador)")
                .FontSize(11).Bold().FontColor(AccentColor);

            column.Item().Element(c => ComposeInfoRow(c, "Conta", _data.SourceAccountId.ToString()));

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(BorderColor);

            // ═══════════════════════════════════════════
            // DESTINO
            // ═══════════════════════════════════════════
            column.Item().PaddingVertical(8).Text("DESTINO (Recebedor)")
                .FontSize(11).Bold().FontColor(AccentColor);

            column.Item().Element(c => ComposeInfoRow(c, "Conta", _data.DestinationAccountId.ToString()));
            column.Item().Element(c => ComposeInfoRow(c, "Chave PIX", _data.PixKey ?? "N/A"));

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(BorderColor);

            // ═══════════════════════════════════════════
            // INFORMAÇÕES ADICIONAIS
            // ═══════════════════════════════════════════
            column.Item().PaddingVertical(8).Text("INFORMAÇÕES ADICIONAIS")
                .FontSize(11).Bold().FontColor(AccentColor);

            column.Item().Element(c => ComposeInfoRow(c, "ID Comprovante", _data.ReceiptId.ToString()));
            column.Item().Element(c => ComposeInfoRow(c, "Correlation ID", _data.CorrelationId.ToString()));

            // ═══════════════════════════════════════════
            // AVISO LEGAL
            // ═══════════════════════════════════════════
            column.Item().PaddingVertical(15)
                .Background("#F8F9FA")
                .Padding(12)
                .Column(notice =>
                {
                    notice.Item().Text("Informações importantes:")
                        .FontSize(8).Bold().FontColor(TextLight);
                    notice.Item().PaddingTop(4).Text(
                        "Este comprovante é válido como documento de prova da transação realizada. " +
                        "A transação PIX foi processada e liquidada pelo Sistema de Pagamentos Instantâneos (SPI) " +
                        "do Banco Central do Brasil. Para contestações, entre em contato com a Central de Atendimento " +
                        "KRT Bank: 0800-123-4567 ou pelo chat do aplicativo.")
                        .FontSize(7).FontColor(TextLight).LineHeight(1.4f);
                });
        });
    }

    private void ComposeInfoRow(IContainer container, string label, string value)
    {
        container.PaddingVertical(3).Row(row =>
        {
            row.RelativeItem(1).Text(label)
                .FontSize(9).FontColor(TextLight);
            row.RelativeItem(2).AlignRight().Text(value)
                .FontSize(9).Bold().FontColor(PrimaryColor);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingVertical(8).Row(row =>
            {
                row.RelativeItem().Text("KRT Bank © 2026 - CNPJ: 00.000.000/0001-00")
                    .FontSize(7).FontColor(TextLight);
                row.RelativeItem().AlignRight().Text($"Gerado em {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC")
                    .FontSize(7).FontColor(TextLight);
            });
            column.Item().AlignCenter().Text("www.krtbank.com.br")
                .FontSize(7).FontColor(AccentColor);
        });
    }
}
