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
        // Formato EMV simplificado para Pix
        var payload = $"00020126" +
            $"0014BR.GOV.BCB.PIX" +
            $"01{pixKey.Length:D2}{pixKey}" +
            $"52040000" +
            $"5303986" +  // BRL
            $"54{amount:F2}".Replace(",", ".").PadLeft(2 + amount.ToString("F2").Length, '0') +
            $"5802BR" +
            $"59{merchantName.Length:D2}{merchantName}" +
            $"60{city.Length:D2}{city}" +
            $"62{(4 + txId.Length):D2}05{txId.Length:D2}{txId}" +
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