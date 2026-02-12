namespace KRT.BuildingBlocks.MessageBus.Storage;

public class BackblazeB2Settings
{
    /// <summary>
    /// Endpoint S3-compatible do Backblaze B2.
    /// Ex: "https://s3.us-east-005.backblazeb2.com"
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Application Key ID (equivalente ao AWS Access Key).
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Application Key (equivalente ao AWS Secret Key).
    /// </summary>
    public string ApplicationKey { get; set; } = string.Empty;

    /// <summary>
    /// Nome do bucket para comprovantes.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// Prefixo base para os arquivos no bucket.
    /// Ex: "receipts" -> receipts/pix/2026/02/12/{txId}.pdf
    /// </summary>
    public string BasePrefix { get; set; } = "receipts";
}
