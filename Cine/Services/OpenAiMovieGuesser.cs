using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Cine.Services
{
    public class OpenAiMovieGuesser : IAiMovieGuesser
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly string _model;
        private readonly bool _isAzure;
        private readonly string? _apiVersion;
        private readonly string? _org;
        private readonly string? _project;

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

        public OpenAiMovieGuesser(HttpClient http)
        {
            _http = http;
            // Leer la clave desde la variable de entorno estándar
            _apiKey = (Environment.GetEnvironmentVariable("OPENAI_API_KEY")?.Trim())
                      ?? throw new InvalidOperationException("Falta la variable de entorno OPENAI_API_KEY.");
            _endpoint = (Environment.GetEnvironmentVariable("OPENAI_API_BASE") ?? "https://api.openai.com").TrimEnd('/');
            _model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";
            _apiVersion = Environment.GetEnvironmentVariable("OPENAI_API_VERSION");
            _org = Environment.GetEnvironmentVariable("OPENAI_ORG");
            _project = Environment.GetEnvironmentVariable("OPENAI_PROJECT");

            // Heurística: si el endpoint parece de Azure, cambiar modo
            var provider = Environment.GetEnvironmentVariable("OPENAI_PROVIDER");
            _isAzure = (provider?.Equals("azure", StringComparison.OrdinalIgnoreCase) ?? false)
                       || _endpoint.Contains("openai.azure.com", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IReadOnlyList<TitleCandidate>> SugerirTitulosAsync(string descripcion, int maxResultados = 5, CancellationToken ct = default)
        {
            var systemPrompt =
                "Eres un asistente experto en cine. Dado un texto que describe escenas, trama o recuerdos de una película, " +
                "devuelve SOLO un JSON con la forma {\"candidates\":[{\"title\":\"...\",\"year\":YYYY?}, ...]} " +
                $"con hasta {maxResultados} candidatos ordenados por probabilidad. No incluyas texto adicional.";

            var req = new ChatRequest(
                Model: _model,
                Messages: new List<ChatMessage>
                {
                    new("system", systemPrompt),
                    new("user", $"Descripcion:\n\"\"\"\n{descripcion}\n\"\"\"")
                },
                Temperature: 0.2,
                ResponseFormat: new { type = "json_object" }
            );

            // Construir URL y headers según proveedor
            string url;
            using var msg = new HttpRequestMessage();
            if (_isAzure)
            {
                if (string.IsNullOrWhiteSpace(_apiVersion))
                    throw new InvalidOperationException("Para Azure OpenAI debes definir OPENAI_API_VERSION, p.ej. 2024-06-01");
                url = $"{_endpoint}/openai/deployments/{_model}/chat/completions?api-version={_apiVersion}";
                msg.Method = HttpMethod.Post;
                msg.RequestUri = new Uri(url);
                msg.Headers.Add("api-key", _apiKey);
            }
            else
            {
                url = $"{_endpoint}/v1/chat/completions";
                msg.Method = HttpMethod.Post;
                msg.RequestUri = new Uri(url);
                msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                if (!string.IsNullOrWhiteSpace(_org))
                    msg.Headers.Add("OpenAI-Organization", _org);
                if (!string.IsNullOrWhiteSpace(_project))
                    msg.Headers.Add("OpenAI-Project", _project);
            }

            msg.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            msg.Content = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(msg, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error llamando al proveedor de IA: {(int)resp.StatusCode} {resp.StatusCode}. Cuerpo: {body}");
            }

            var parsed = JsonSerializer.Deserialize<ChatResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var content = parsed?.Choices?[0]?.Message?.Content;

            if (string.IsNullOrWhiteSpace(content))
                return Array.Empty<TitleCandidate>();

            var candidates = JsonSerializer.Deserialize<AiCandidates>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })?.Candidates ?? new List<TitleCandidate>();

            candidates.RemoveAll(c => string.IsNullOrWhiteSpace(c.Title));
            return candidates;
        }
    }
}
