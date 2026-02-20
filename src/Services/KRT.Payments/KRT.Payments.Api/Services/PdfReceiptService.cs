using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KRT.Payments.Api.Services;

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
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Calibri));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("KRT Bank").Bold().FontSize(22).FontColor("#1a237e");
                    col.Item().AlignCenter().Text("Comprovante de Transferencia Pix").FontSize(13).FontColor("#666666");
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#e0e0e0");
                });

                // Content
                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Spacing(6);

                    var statusBg = data.Status == "Concluido" ? "#e8f5e9" : "#fff3e0";
                    var statusFg = data.Status == "Concluido" ? "#2e7d32" : "#e65100";
                    var statusText = data.Status == "Concluido" ? "TRANSACAO CONCLUIDA" : data.Status.ToUpper();

                    col.Item().AlignCenter().Padding(8).Background(statusBg)
                        .Text(statusText).Bold().FontSize(12).FontColor(statusFg);

                    col.Item().PaddingTop(10);

                    AddRow(col, "ID da Transacao", TruncateId(data.TransactionId));
                    AddRow(col, "Data/Hora", data.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"));

                    col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor("#eeeeee");
                    col.Item().PaddingTop(6);

                    AddRow(col, "De (Origem)", data.SourceName);
                    if (!string.IsNullOrEmpty(data.SourceDocument))
                        AddRow(col, "Documento Origem", MaskCpf(data.SourceDocument));

                    col.Item().PaddingTop(4);

                    AddRow(col, "Para (Destino)", data.DestinationName);
                    if (!string.IsNullOrEmpty(data.DestinationDocument))
                        AddRow(col, "Chave Pix", data.DestinationDocument);

                    col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor("#eeeeee");
                    col.Item().PaddingTop(6);

                    col.Item().AlignCenter().PaddingVertical(10)
                        .Text($"R$ {data.Amount:N2}").Bold().FontSize(28).FontColor("#1a237e");

                    col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor("#eeeeee");

                    if (data.QrCodeBytes != null && data.QrCodeBytes.Length > 0)
                    {
                        col.Item().PaddingTop(10).AlignCenter().Width(120).Image(data.QrCodeBytes);
                        col.Item().AlignCenter().Text("Escaneie para verificar").FontSize(8).FontColor("#999999");
                    }
                });

                // Footer
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor("#e0e0e0");
                    col.Item().PaddingTop(6).AlignCenter()
                        .Text($"Documento gerado em {DateTime.Now:dd/MM/yyyy HH:mm} - KRT Bank S.A.")
                        .FontSize(8).FontColor("#999999");
                    col.Item().AlignCenter()
                        .Text("Este comprovante nao tem valor fiscal")
                        .FontSize(7).FontColor("#bbbbbb");
                });
            });
        }).GeneratePdf();
    }

    private static void AddRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem(1).Text(label).FontColor("#888888").FontSize(10);
            row.RelativeItem(2).Text(value).Bold().FontSize(11);
        });
    }

    private static string TruncateId(string id)
    {
        if (string.IsNullOrEmpty(id)) return "";
        return id.Length > 20 ? id[..8] + "..." + id[^8..] : id;
    }

    private static string MaskCpf(string cpf)
    {
        if (string.IsNullOrEmpty(cpf) || cpf.Length < 11) return cpf ?? "";
        var clean = cpf.Replace(".", "").Replace("-", "");
        return $"***.{clean[3..6]}.{clean[6..9]}-**";
    }
}

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
