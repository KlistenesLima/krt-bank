using System.Globalization;

namespace KRT.Payments.Api.Services;

public class BRCodeData
{
    public decimal Amount { get; set; }
    public string PixKey { get; set; } = "";
    public string MerchantName { get; set; } = "";
    public string MerchantCity { get; set; } = "";
    public string TxId { get; set; } = "";
    public bool IsValid { get; set; }
}

/// <summary>
/// Parser de BRCode (PIX Copia e Cola) no formato EMV QR Code.
/// Formato TLV: cada campo = 2 chars (tag) + 2 chars (tamanho) + N chars (valor).
/// Tags compostas (26, 62) contêm sub-TLVs no mesmo formato.
/// </summary>
public class BRCodeParser
{
    public BRCodeData Parse(string brcode)
    {
        var result = new BRCodeData();

        if (string.IsNullOrWhiteSpace(brcode) || brcode.Trim().Length < 20)
            return result;

        try
        {
            var clean = brcode.Trim();
            var tags = ParseTLV(clean);

            // Tag 00: Payload Format Indicator (deve ser "01")
            if (!tags.TryGetValue("00", out var pfi) || pfi != "01")
                return result;

            // Tag 26: Merchant Account Information (contém subtags)
            if (!tags.TryGetValue("26", out var tag26))
                return result;

            var sub26 = ParseTLV(tag26);

            // Subtag 00: GUI deve ser "BR.GOV.BCB.PIX"
            if (!sub26.TryGetValue("00", out var gui) || gui != "BR.GOV.BCB.PIX")
                return result;

            // Subtag 01: Chave PIX
            if (sub26.TryGetValue("01", out var pixKey))
                result.PixKey = pixKey;

            // Tag 54: Valor da transação
            if (tags.TryGetValue("54", out var amountStr) &&
                decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                result.Amount = amount;

            // Tag 59: Nome do recebedor
            if (tags.TryGetValue("59", out var merchantName))
                result.MerchantName = merchantName;

            // Tag 60: Cidade
            if (tags.TryGetValue("60", out var city))
                result.MerchantCity = city;

            // Tag 62: Dados adicionais (contém subtags)
            if (tags.TryGetValue("62", out var tag62))
            {
                var sub62 = ParseTLV(tag62);
                if (sub62.TryGetValue("05", out var txId))
                    result.TxId = txId;
            }

            result.IsValid = !string.IsNullOrEmpty(result.PixKey) && result.Amount > 0;
        }
        catch
        {
            // Parsing falhou — retorna resultado vazio (IsValid = false)
        }

        return result;
    }

    private static Dictionary<string, string> ParseTLV(string data)
    {
        var result = new Dictionary<string, string>();
        var i = 0;

        while (i + 4 <= data.Length)
        {
            var tag = data.Substring(i, 2);
            var lenStr = data.Substring(i + 2, 2);

            if (!int.TryParse(lenStr, out var len) || len < 0 || i + 4 + len > data.Length)
                break;

            result[tag] = data.Substring(i + 4, len);
            i += 4 + len;
        }

        return result;
    }
}
