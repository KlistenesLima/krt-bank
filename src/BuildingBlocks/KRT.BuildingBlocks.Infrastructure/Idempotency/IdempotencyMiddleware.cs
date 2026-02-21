using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace KRT.BuildingBlocks.Infrastructure.Idempotency
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;

        public IdempotencyMiddleware(RequestDelegate next, IDistributedCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Só aplicamos idempotência em métodos que alteram estado (POST/PUT/PATCH)
            if (context.Request.Method == "GET" || context.Request.Method == "DELETE")
            {
                await _next(context);
                return;
            }

            // 2. Verificamos se o Header existe
            if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key))
            {
                // Se não tem chave, segue fluxo normal (ou poderia rejeitar, dependendo da regra)
                await _next(context);
                return;
            }

            var cacheKey = $"Idempotency_{key}";
            
            // 3. Verifica se já processamos essa chave
            var cachedResponse = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResponse))
            {
                // JÁ PROCESSADO: Retorna o resultado salvo imediatamente (Short-circuit)
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200; 
                await context.Response.WriteAsync(cachedResponse);
                return;
            }

            // 4. Se é novo, precisamos interceptar a resposta para salvar depois
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Executa a Controller
            await _next(context);

            // 5. Salva o resultado no Cache se foi sucesso
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                await _cache.SetStringAsync(cacheKey, text, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // Chave vale por 24h
                });

                await responseBody.CopyToAsync(originalBodyStream);
            }
            else 
            {
                // Se deu erro, copiamos de volta sem salvar (para permitir retry)
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
