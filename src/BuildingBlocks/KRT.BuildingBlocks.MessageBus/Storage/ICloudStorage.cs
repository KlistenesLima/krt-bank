namespace KRT.BuildingBlocks.MessageBus.Storage;

/// <summary>
/// Abstração para storage em nuvem.
/// Permite trocar Backblaze B2 por AWS S3, Azure Blob, MinIO, etc.
/// sem alterar nenhum consumer ou worker.
/// </summary>
public interface ICloudStorage
{
    /// <summary>
    /// Faz upload de um arquivo para o cloud storage.
    /// </summary>
    /// <param name="fileName">Caminho completo no bucket (ex: receipts/pix/2026/02/12/uuid.pdf)</param>
    /// <param name="content">Conteúdo do arquivo em bytes</param>
    /// <param name="contentType">MIME type (ex: application/pdf)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>URL pública ou identificador do arquivo</returns>
    Task<CloudStorageResult> UploadAsync(string fileName, byte[] content, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Gera URL pré-assinada para download temporário (útil para bucket Private).
    /// </summary>
    Task<string> GetPresignedUrlAsync(string fileName, TimeSpan expiry, CancellationToken ct = default);

    /// <summary>
    /// Verifica se o arquivo existe no bucket.
    /// </summary>
    Task<bool> ExistsAsync(string fileName, CancellationToken ct = default);
}

public record CloudStorageResult
{
    public bool Success { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string? ETag { get; init; }
    public string? Error { get; init; }

    public static CloudStorageResult Ok(string fileName, string url, long size, string? etag) =>
        new() { Success = true, FileName = fileName, Url = url, SizeBytes = size, ETag = etag };

    public static CloudStorageResult Fail(string fileName, string error) =>
        new() { Success = false, FileName = fileName, Error = error };
}
