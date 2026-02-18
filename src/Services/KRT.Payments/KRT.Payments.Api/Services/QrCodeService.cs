using QRCoder;

namespace KRT.Payments.Api.Services;

/// <summary>
/// Gera QR Codes Pix no formato EMV (simplificado).
/// </summary>
public class QrCodeService
{
    /// <summary>
    /// Gera payload Pix EMV simplificado.
    /// </summary>
    public string GeneratePixPayload(string pixKey, string merchantName, string city, decimal amount, string txId)
    {
        var amountStr = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        // Tag 26: Merchant Account Information (subtags)
        var tag26Content = $"0014BR.GOV.BCB.PIX01{pixKey.Length:D2}{pixKey}";

        // Tag 62: Additional Data (subtags)
        var tag62Content = $"05{txId.Length:D2}{txId}";

        // Formato EMV com TLV correto (tag 2 chars + length 2 chars + value)
        var payload = "000201" +
            $"26{tag26Content.Length:D2}{tag26Content}" +
            "52040000" +
            "5303986" +
            $"54{amountStr.Length:D2}{amountStr}" +
            "5802BR" +
            $"59{merchantName.Length:D2}{merchantName}" +
            $"60{city.Length:D2}{city}" +
            $"62{tag62Content.Length:D2}{tag62Content}" +
            "6304";

        // CRC16 simplificado (para demo)
        payload += "0000";
        return payload;
    }

    /// <summary>
    /// Gera imagem QR Code como base64 PNG.
    /// </summary>
    public string GenerateQrCodeBase64(string content)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(data);
        var bytes = qrCode.GetGraphic(10);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Gera imagem QR Code como bytes PNG.
    /// </summary>
    public byte[] GenerateQrCodeBytes(string content)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(data);
        return qrCode.GetGraphic(10);
    }
}