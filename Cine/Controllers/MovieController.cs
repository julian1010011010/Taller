using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Cine.Services;

namespace Cine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly CallApiMovies _callApiMovies;
        private readonly IMovieIdentifier _identifier;

        private static readonly string[] Summaries = new[]
    {
            "Matrix", "Matrix 1 ", "Matrix 2 ", "Matrix 3 ", "Matrix 4 "
        };

        public MovieController(CallApiMovies callApiMovies, IMovieIdentifier identifier)
        {
            _callApiMovies = callApiMovies;
            _identifier = identifier;
        }

        /// <summary>
        /// Obtiene una lista de películas desde la API externa de IMDb.
        /// Permite filtrar por título, tipo de contenido, página de resultados y versión de la API.
        /// </summary>
        /// <param name="titulo">Título o palabra clave de la película a buscar. Ejemplo: "Iron Man".</param>
        /// <param name="tipo">Tipo de contenido a buscar. Puede ser "movie", "series", etc. Si se deja vacío, devuelve todos los tipos.</param>
        /// <param name="pagina">Número de página de los resultados. Por defecto es 1.</param>
        /// <param name="version">Versión de la API a utilizar. Por defecto es 1.</param>
        /// <returns>Retorna un <see cref='IActionResult'/> con la lista de películas encontradas. Cada película incluye título, año, actores, URL de IMDb y poster.</returns>
        [HttpGet("ListMovies")]
        public async Task<IActionResult> ListMovies(
            [FromQuery] string titulo = "Matrix",
            [FromQuery] string tipo = "",
            [FromQuery] int pagina = 1,
            [FromQuery] int version = 1)
        {
            try
            {
                var peliculas = await _callApiMovies.ObtenerPeliculas(
                    q: titulo,
                    tt: tipo,
                    lsn: pagina,
                    v: version);

                return Ok(peliculas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener películas", detalle = ex.Message });
            }
        }



        // POST api/movie/adivinar
        [HttpPost("adivinar")]
        public async Task<IActionResult> Adivinar([FromBody] string descripcion, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return BadRequest("La descripción es requerida.");

            var movie = await _identifier.IdentificarAsync(descripcion, ct);
            if (movie == null) return NotFound();

            return Ok(movie);
        }

        // POST api/movie/candidatos
        [HttpPost("candidatos")]
        public async Task<IActionResult> Candidatos([FromBody] string descripcion, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return BadRequest("La descripción es requerida.");

            var items = await _identifier.IdentificarCandidatosAsync(descripcion, ct: ct);
            return Ok(items);
        }

        [HttpGet("media/{imdbId}")]
        public async Task<IActionResult> Media(string imdbId, CancellationToken ct)
        {
            try
            {
                var media = await _callApiMovies.ObtenerMediaAsync(imdbId, ct);

                // Habilita rangos para <video> (seek)
                Response.Headers["Accept-Ranges"] = "bytes";

                // Propaga metadatos útiles (opcionales)
                if (!string.IsNullOrEmpty(media.ETag))
                    Response.Headers["ETag"] = media.ETag!;
                if (media.LastModified is not null)
                    Response.Headers["Last-Modified"] = media.LastModified.Value.ToString("R");
                Response.Headers["Cache-Control"] = "public, max-age=3600";

                // Devuelve stream sin cargar a memoria + rangos
                return File(media.Stream, media.ContentType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener media", detalle = ex.Message });
            }
        }
    }
}
