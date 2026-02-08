using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/chatbot")]
public class ChatbotController : ControllerBase
{
    private static readonly Dictionary<string, (string response, string category, string[] suggestions)> _intents = new(StringComparer.OrdinalIgnoreCase)
    {
        ["saldo"] = ("Seu saldo atual e de R$ 12.450,80. Ultimo deposito: R$ 3.200,00 recebido ontem via Pix.", "conta", new[] { "Ver extrato", "Fazer Pix", "Ver graficos" }),
        ["extrato"] = ("Voce pode acessar seu extrato completo na aba Extrato. La voce pode filtrar por periodo, tipo e exportar em CSV ou PDF.", "conta", new[] { "Abrir extrato", "Exportar PDF", "Ultimos 30 dias" }),
        ["pix"] = ("Para fazer um Pix, va ate a aba Pix Transfer. Voce pode enviar por chave CPF, email, telefone ou QR Code. Limite diurno: R$ 10.000.", "pix", new[] { "Fazer Pix", "Ver limites", "QR Code" }),
        ["cartao"] = ("Voce tem 2 cartoes virtuais ativos. Para criar um novo, va em Cartoes Virtuais. CVV rotativo disponivel para compras online.", "cartao", new[] { "Ver cartoes", "Criar cartao", "Bloquear cartao" }),
        ["seguro"] = ("Temos 4 planos: Pix Protect (R$4,90/mes), Celular (R$19,90), Vida Basico (R$29,90) e Cartao (R$7,90). Todos com cobertura 24h.", "seguro", new[] { "Ver planos", "Contratar", "Meus seguros" }),
        ["emprestimo"] = ("Oferecemos emprestimo pessoal a partir de 29,9% a.a., consignado 18,5% a.a. e mais. Use o simulador para calcular parcelas.", "emprestimo", new[] { "Simular emprestimo", "Ver taxas", "Tabela Price vs SAC" }),
        ["meta"] = ("Voce tem 4 metas ativas totalizando R$ 30.650 guardados. A meta mais proxima e 'iPhone novo' com 76% concluida.", "meta", new[] { "Ver metas", "Criar meta", "Depositar" }),
        ["ajuda"] = ("Posso ajudar com: saldo, extrato, pix, cartoes, seguros, emprestimos, metas, boletos, notificacoes e configuracoes. O que precisa?", "ajuda", new[] { "Ver saldo", "Fazer Pix", "Abrir extrato" }),
        ["boleto"] = ("Para pagar um boleto, va em Boletos e cole o codigo de barras. Para gerar, escolha 'Gerar Boleto'. Pagamento instantaneo.", "pagamento", new[] { "Pagar boleto", "Gerar boleto", "Meus boletos" }),
        ["notificacao"] = ("Voce tem 4 notificacoes nao lidas. A mais recente: 'Pix recebido de R$ 1.500,00 de Maria Silva'.", "notificacao", new[] { "Ver notificacoes", "Marcar lidas", "Configurar alertas" }),
    };

    /// <summary>
    /// Processa mensagem do chatbot com NLP basico.
    /// </summary>
    [HttpPost("message")]
    [AllowAnonymous]
    public IActionResult SendMessage([FromBody] ChatMessageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Message))
            return BadRequest(new { error = "Mensagem vazia" });

        var msg = req.Message.ToLower().Trim();

        // Buscar intent
        foreach (var (key, value) in _intents)
        {
            if (msg.Contains(key))
            {
                return Ok(new
                {
                    response = value.response,
                    category = value.category,
                    suggestions = value.suggestions,
                    confidence = 0.92,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // Saudacoes
        if (msg.Contains("ola") || msg.Contains("oi") || msg.Contains("bom dia") || msg.Contains("boa tarde"))
        {
            return Ok(new
            {
                response = "Ola! Sou o assistente virtual do KRT Bank. Como posso ajudar voce hoje?",
                category = "saudacao",
                suggestions = new[] { "Ver saldo", "Fazer Pix", "Ajuda" },
                confidence = 0.95,
                timestamp = DateTime.UtcNow
            });
        }

        // Default
        return Ok(new
        {
            response = "Desculpe, nao entendi. Posso ajudar com: saldo, extrato, pix, cartoes, seguros, emprestimos, metas e boletos. Tente ser mais especifico!",
            category = "fallback",
            suggestions = new[] { "Ajuda", "Ver saldo", "Fazer Pix" },
            confidence = 0.3,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Historico de mensagens da sessao.
    /// </summary>
    [HttpGet("suggestions")]
    [AllowAnonymous]
    public IActionResult GetSuggestions()
    {
        return Ok(new[]
        {
            "Qual meu saldo?", "Quero fazer um Pix", "Mostrar extrato",
            "Como criar cartao virtual?", "Simular emprestimo", "Minhas metas",
            "Pagar boleto", "Contratar seguro", "Configurar notificacoes"
        });
    }
}

public record ChatMessageRequest(string Message);