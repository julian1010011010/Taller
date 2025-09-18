using Cine.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cine
{ 
    public class CallApiMovies
    {
        private   HttpClient _httpClient;

        // Inyectamos HttpClient
        public CallApiMovies(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<List<Descripcion>> ObtenerPeliculas(string q = "", string tt = "", int lsn = 1, int v = 1)
        {
            string url = $"https://imdb.iamidiotareyoutoo.com/search?q={q}&tt={tt}&lsn={lsn}&v={v}";

            var respuesta = await _httpClient.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
                throw new Exception($"Error al llamar a la API: {respuesta.StatusCode}");

            var contenido = await respuesta.Content.ReadAsStringAsync();

            var resultado = JsonSerializer.Deserialize<RespuestaApiMoviesRoot>(contenido,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return resultado?.Description;
        }

        // Nuevo: obtener media (trailer, si existe) por IMDB ID
        public async Task<MediaStreamResult> ObtenerMediaAsync(string imdbId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(imdbId))
                throw new ArgumentException("imdbId requerido", nameof(imdbId));

            var url = $"https://imdb.iamidiotareyoutoo.com/media/{imdbId}";

            // Importante: headers primero, no buferizar todo el cuerpo
            var resp = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Upstream error: {resp.StatusCode}");

            var stream = await resp.Content.ReadAsStreamAsync(ct);
            var contentType = resp.Content.Headers.ContentType?.ToString() ?? "video/mp4";

            return new MediaStreamResult
            {
                Stream = stream,                 // ASP.NET Core lo cerrará al terminar de enviar
                ContentType = contentType,
                ETag = resp.Headers.ETag?.Tag,
                LastModified = resp.Content.Headers.LastModified
            };
        }
    }
}
