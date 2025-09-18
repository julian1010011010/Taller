using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cine.Models;
using System.Collections.Generic;

namespace Cine.Services
{
    public interface IMovieIdentifier
    {
        Task<Descripcion?> IdentificarAsync(string descripcion, CancellationToken ct = default);
        Task<IReadOnlyList<Descripcion>> IdentificarCandidatosAsync(string descripcion, int maxValidaciones = 5, CancellationToken ct = default);
    }

    public class MovieIdentifier : IMovieIdentifier
    {
        private readonly IAiMovieGuesser _guesser;
        private readonly CallApiMovies _api;

        public MovieIdentifier(IAiMovieGuesser guesser, CallApiMovies api)
        {
            _guesser = guesser;
            _api = api;
        }

        public async Task<Descripcion?> IdentificarAsync(string descripcion, CancellationToken ct = default)
        {
            var validados = await IdentificarCandidatosAsync(descripcion, ct: ct);
            return validados.FirstOrDefault();
        }

        public async Task<IReadOnlyList<Descripcion>> IdentificarCandidatosAsync(string descripcion, int maxValidaciones = 5, CancellationToken ct = default)
        {
            var candidatos = await _guesser.SugerirTitulosAsync(descripcion, maxValidaciones, ct);
            var resultados = new List<Descripcion>();

            foreach (var c in candidatos.Take(maxValidaciones))
            {
                var lista = await _api.ObtenerPeliculas(q: c.Title);
                if (lista == null || lista.Count == 0) continue;

                var exact =
                    lista.FirstOrDefault(d =>
                        d.Title.Equals(c.Title, StringComparison.OrdinalIgnoreCase) &&
                        (!c.Year.HasValue || d.Year == c.Year.Value));

                if (exact != null)
                {
                    resultados.Add(exact);
                    continue;
                }

                var porAnio = c.Year.HasValue ? lista.FirstOrDefault(d => d.Year == c.Year.Value) : null;
                resultados.Add(porAnio ?? lista.First());
            }

            var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var unicos = resultados.Where(r => r?.ImdbId != null && vistos.Add(r.ImdbId)).ToList();
            return unicos;
        }
    }
}
