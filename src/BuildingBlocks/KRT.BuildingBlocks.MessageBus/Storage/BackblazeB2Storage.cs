using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KRT.BuildingBlocks.MessageBus.Storage;

/// <summary>
/// Implementação de cloud storage usando Backblaze B2 via protocolo S3-compatible.
/// 
/// POR QUE S3-COMPATIBLE (e não API nativa do B2):
/// - Mesmo SDK usado por AWS S3, MinIO, DigitalOcean Spaces
/// - Se precisar migrar para AWS S3, basta trocar endpoint e credenciais
/// - AWSSDK.S3 é battle-tested com milhões de usuários
/// - Backblaze recomenda S3-compatible para novos projetos
/// 
/// CUSTO BACKBLAZE B2 vs AWS S3:
/// - Storage: $0.006/GB (B2) vs $0.023/GB (S3) = 4x mais barato
/// - Download: 3x free por dia (B2) vs pago desde o 1º byte (S3)
/// - Para portfólio: essencialmente grátis (10GB free tier)
/// </summary>
public class BackblazeB2Storage : ICloudStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly BackblazeB2Settings _settings;
    private readonly ILogger<BackblazeB2Storage> _logger;

    public BackblazeB2Storage(
        IAmazonS3 s3Client,
        IOptions<BackblazeB2Settings> settings,
        ILogger<BackblazeB2Storage> logger)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<CloudStorageResult> UploadAsync(
        string fileName, byte[] content, string contentType, CancellationToken ct = default)
    {
        var fullPath = string.IsNullOrEmpty(_settings.BasePrefix)
            ? fileName
            : $"{_settings.BasePrefix}/{fileName}";

        try
        {
            using var stream = new MemoryStream(content);

            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fullPath,
                InputStream = stream,
                ContentType = contentType,
                AutoCloseStream = true,
                // Metadata para rastreabilidade
                Metadata =
                {
                    ["x-amz-meta-uploaded-by"] = "krt-bank-receipt-worker",
                    ["x-amz-meta-uploaded-at"] = DateTime.UtcNow.ToString("O")
                }
            };

            var response = await _s3Client.PutObjectAsync(request, ct);

            var url = $"{_settings.Endpoint}/{_settings.BucketName}/{fullPath}";

            _logger.LogInformation(
                "B2 Upload OK: {FileName} ({Size} bytes, ETag={ETag})",
                fullPath, content.Length, response.ETag);

            return CloudStorageResult.Ok(fullPath, url, content.Length, response.ETag);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex,
                "B2 Upload FAILED: {FileName} ({Size} bytes). StatusCode={StatusCode}, ErrorCode={ErrorCode}",
                fullPath, content.Length, ex.StatusCode, ex.ErrorCode);

            return CloudStorageResult.Fail(fullPath, $"{ex.ErrorCode}: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "B2 Upload FAILED: {FileName} ({Size} bytes)", fullPath, content.Length);
            return CloudStorageResult.Fail(fullPath, ex.Message);
        }
    }

    public async Task<string> GetPresignedUrlAsync(
        string fileName, TimeSpan expiry, CancellationToken ct = default)
    {
        var fullPath = string.IsNullOrEmpty(_settings.BasePrefix)
            ? fileName
            : $"{_settings.BasePrefix}/{fileName}";

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _settings.BucketName,
                Key = fullPath,
                Expires = DateTime.UtcNow.Add(expiry),
                Verb = HttpVerb.GET
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);

            _logger.LogInformation(
                "B2 Presigned URL generated: {FileName} (expires in {Expiry})",
                fullPath, expiry);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "B2 Presigned URL FAILED: {FileName}", fullPath);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string fileName, CancellationToken ct = default)
    {
        var fullPath = string.IsNullOrEmpty(_settings.BasePrefix)
            ? fileName
            : $"{_settings.BasePrefix}/{fileName}";

        try
        {
            await _s3Client.GetObjectMetadataAsync(_settings.BucketName, fullPath, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
