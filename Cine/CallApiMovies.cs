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

    }
}
