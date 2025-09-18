using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Cine.Services
{
    public class OpenAiMovieGuesser : IAiMovieGuesser
    {
        private readonly HttpClient _http;
        private readonly OpenAiSettings _cfg;
        private readonly bool _isAzure;

        private record ChatMessage([property: JsonPropertyName("role")] string Role,
                                   [property: JsonPropertyName("content")] string Content);

        private record ChatRequest(
            [property: JsonPropertyName("model")] string Model,
            [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
            [property: JsonPropertyName("temperature")] double Temperature,
            [property: JsonPropertyName("response_format")] object ResponseFormat);

        private record ChoiceMsg([property: JsonPropertyName("content")] string Content);
        private record Choice([property: JsonPropertyName("message")] ChoiceMsg Message);
        private record ChatResponse([property: JsonPropertyName("choices")] List<Choice> Choices);

        private class AiCandidates
        {
            [JsonPropertyName("candidates")]
            public List<TitleCandidate> Candidates { get; set; } = new();
        }

        public OpenAiMovieGuesser(HttpClient http, IOptions<OpenAiSettings> options)
        {
            _http = http;
            _cfg = options.Value ?? new OpenAiSettings();

            // Fallback a environment variables si no viene de appsettings
            _cfg.ApiKey ??= Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            _cfg.BaseUrl ??= Environment.GetEnvironmentVariable("OPENAI_API_BASE") ?? "https://api.openai.com";
            _cfg.Model ??= Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";
            _cfg.Provider ??= Environment.GetEnvironmentVariable("OPENAI_PROVIDER");
            _cfg.ApiVersion ??= Environment.GetEnvironmentVariable("OPENAI_API_VERSION");
            _cfg.Organization ??= Environment.GetEnvironmentVariable("OPENAI_ORG");
            _cfg.Project ??= Environment.GetEnvironmentVariable("OPENAI_PROJECT");

            if (string.IsNullOrWhiteSpace(_cfg.ApiKey))
                throw new InvalidOperationException("Falta OpenAI ApiKey. Configura OpenAi:ApiKey en appsettings o OPENAI_API_KEY.");

            _cfg.BaseUrl = _cfg.BaseUrl!.TrimEnd('/');

            _isAzure = (_cfg.Provider?.Equals("azure", StringComparison.OrdinalIgnoreCase) ?? false)
                       || _cfg.BaseUrl.Contains("openai.azure.com", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IReadOnlyList<TitleCandidate>> SugerirTitulosAsync(string descripcion, int maxResultados = 5, CancellationToken ct = default)
        {
            var systemPrompt =
                "Eres un asistente experto en cine. Dado un texto que describe escenas, trama o recuerdos de una película, " +
                "devuelve SOLO un JSON con la forma {\"candidates\":[{\"title\":\"...\",\"year\":YYYY?}, ...]} " +
                $"con hasta {maxResultados} candidatos ordenados por probabilidad. No incluyas texto adicional.";

            var req = new ChatRequest(
                Model: _cfg.Model!,
                Messages: new List<ChatMessage>
                {
                    new("system", systemPrompt),
                    new("user", $"Descripcion:\n\"\"\"\n{descripcion}\n\"\"\"")
                },
                Temperature: 0.2,
                ResponseFormat: new { type = "json_object" }
            );

            using var msg = new HttpRequestMessage();
            if (_isAzure)
            {
                if (string.IsNullOrWhiteSpace(_cfg.ApiVersion))
                    throw new InvalidOperationException("Para Azure OpenAI define OpenAi:ApiVersion o OPENAI_API_VERSION (p.ej. 2024-06-01).");
                msg.Method = HttpMethod.Post;
                msg.RequestUri = new Uri($"{_cfg.BaseUrl}/openai/deployments/{_cfg.Model}/chat/completions?api-version={_cfg.ApiVersion}");
                msg.Headers.Add("api-key", _cfg.ApiKey);
            }
            else
            {
                msg.Method = HttpMethod.Post;
                msg.RequestUri = new Uri($"{_cfg.BaseUrl}/v1/chat/completions");
                msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cfg.ApiKey);
                if (!string.IsNullOrWhiteSpace(_cfg.Organization))
                    msg.Headers.Add("OpenAI-Organization", _cfg.Organization);
                if (!string.IsNullOrWhiteSpace(_cfg.Project))
                    msg.Headers.Add("OpenAI-Project", _cfg.Project);
            }

            msg.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            msg.Content = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(msg, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error OpenAI: {(int)resp.StatusCode} {resp.StatusCode}. Cuerpo: {body}");
            }

            var parsed = JsonSerializer.Deserialize<ChatResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var content = parsed?.Choices?[0]?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content)) return Array.Empty<TitleCandidate>();

            var candidates = JsonSerializer.Deserialize<AiCandidates>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })?.Candidates ?? new List<TitleCandidate>();
            candidates.RemoveAll(c => string.IsNullOrWhiteSpace(c.Title));
            return candidates;
        }
    }
}
