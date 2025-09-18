using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly CallApiMovies _callApiMovies;

        private static readonly string[] Summaries = new[]
    {
            "Matrix", "Matrix 1 ", "Matrix 2 ", "Matrix 3 ", "Matrix 4 "
        };

        public  MovieController(CallApiMovies callApiMovies )
        {
            _callApiMovies = callApiMovies;
        }

        /// <summary>
        /// Obtiene una lista de películas desde la API externa de IMDb.
        /// Permite filtrar por título, tipo de contenido, página de resultados y versión de la API.
        /// </summary>
        /// <param name="titulo">Título o palabra clave de la película a buscar. Ejemplo: "Iron Man".</param>
        /// <param name="tipo">Tipo de contenido a buscar. Puede ser "movie", "series", etc. Si se deja vacío, devuelve todos los tipos.</param>
        /// <param name="pagina">Número de página de los resultados. Por defecto es 1.</param>
        /// <param name="version">Versión de la API a utilizar. Por defecto es 1.</param>
        /// <returns>Retorna un <see cref="IActionResult"/> con la lista de películas encontradas. Cada película incluye título, año, actores, URL de IMDb y poster.</returns>
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




    }
}
