namespace Cine.Services
{
    public class OpenAiSettings
    {
        public string? ApiKey { get; set; }
        public string? BaseUrl { get; set; }
        public string? Model { get; set; }
        public string? Provider { get; set; } // "openai" | "azure"
        public string? ApiVersion { get; set; } // requerido para Azure
        public string? Organization { get; set; }
        public string? Project { get; set; }
    }
}
