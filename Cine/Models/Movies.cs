using System.Net.Http;
using System.Text.Json.Serialization;

namespace Cine.Models
{ 
    public class RespuestaApiMoviesRoot
    {
        public bool Ok { get; set; }
        public List<Descripcion> Description { get; set; }
        public int Error_Code { get; set; }
    }

    public class Descripcion
    {
        [JsonPropertyName("#TITLE")]
        public string Title { get; set; }

        [JsonPropertyName("#YEAR")]
        public int Year { get; set; }

        [JsonPropertyName("#IMDB_ID")]
        public string ImdbId { get; set; }

        [JsonPropertyName("#ACTORS")]
        public string Actors { get; set; }

        [JsonPropertyName("#IMDB_URL")]
        public string ImdbUrl { get; set; }

        [JsonPropertyName("#IMG_POSTER")]
        public string ImgPoster { get; set; }
    }

}
