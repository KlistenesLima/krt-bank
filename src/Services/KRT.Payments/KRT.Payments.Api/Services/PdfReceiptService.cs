using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KRT.Payments.Api.Services;

/// <summary>
/// Gera comprovantes Pix em PDF usando QuestPDF.
/// </summary>
public class PdfReceiptService
{
    public byte[] GeneratePixReceipt(PixReceiptData data)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Helvetica"));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("KRT Bank").Bold().FontSize(22).FontColor("#1a237e");
                    col.Item().AlignCenter().Text("Comprovante de Transferencia Pix").FontSize(13).FontColor("#666");
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#e0e0e0");
                });

                // Content
                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Spacing(6);

                    // Status badge
                    col.Item().AlignCenter().Padding(8).Background(data.Status == "Completed" ? "#e8f5e9" : "#fff3e0")
                        .Text(data.Status == "Completed" ? "TRANSACAO CONCLUIDA" : $"STATUS: {data.Status.ToUpper()}")
                        .Bold().FontSize(12)
                        .FontColor(data.Status == "Completed" ? "#2e7d32" : "#e65100");

                    col.Item().PaddingTop(10);

                    // Info rows
                    AddRow(col, "ID da Transacao", data.TransactionId);
                    AddRow(col, "Data/Hora", data.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"));
                    
                    col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor("#eee");
                    col.Item().PaddingTop(6);

                    AddRow(col, "De (Origem)", data.SourceName);
                    AddRow(col, "CPF Origem", MaskCpf(data.SourceDocument));
                    
                    col.Item().PaddingTop(4);

                    AddRow(col, "Para (Destino)", data.DestinationName);
                    AddRow(col, "CPF Destino", MaskCpf(data.DestinationDocument));

                    col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor("#eee");
                    col.Item().PaddingTop(6);

                    // Valor em destaque
                    col.Item().AlignCenter().PaddingVertical(10)
                        .Text($"R$ {data.Amount:N2}").Bold().FontSize(28).FontColor("#1a237e");

                    col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor("#eee");

                    // QR Code se disponivel
                    if (data.QrCodeBytes != null && data.QrCodeBytes.Length > 0)
                    {
                        col.Item().PaddingTop(10).AlignCenter().Width(120).Image(data.QrCodeBytes);
                        col.Item().AlignCenter().Text("Escaneie para verificar").FontSize(8).FontColor("#999");
                    }
                });

                // Footer
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor("#e0e0e0");
                    col.Item().PaddingTop(6).AlignCenter()
                        .Text($"Documento gerado em {DateTime.Now:dd/MM/yyyy HH:mm} — KRT Bank S.A.")
                        .FontSize(8).FontColor("#999");
                    col.Item().AlignCenter()
                        .Text("Este comprovante nao tem valor fiscal")
                        .FontSize(7).FontColor("#bbb");
                });
            });
        }).GeneratePdf();
    }

    private static void AddRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem(1).Text(label).FontColor("#888").FontSize(10);
            row.RelativeItem(2).Text(value).Bold().FontSize(11);
        });
    }

    private static string MaskCpf(string cpf)
    {
        if (string.IsNullOrEmpty(cpf) || cpf.Length < 11) return cpf ?? "";
        var clean = cpf.Replace(".", "").Replace("-", "");
        return $"***.{clean[3..6]}.{clean[6..9]}-**";
    }
}

/// <summary>
/// Dados para gerar comprovante Pix.
/// </summary>
public class PixReceiptData
{
    public string TransactionId { get; set; } = "";
    public string SourceName { get; set; } = "";
    public string SourceDocument { get; set; } = "";
    public string DestinationName { get; set; } = "";
    public string DestinationDocument { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Completed";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public byte[]? QrCodeBytes { get; set; }
}