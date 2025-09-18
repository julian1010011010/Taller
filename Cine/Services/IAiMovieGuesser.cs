using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cine.Services
{
    public class TitleCandidate
    {
        public string Title { get; set; } = string.Empty;
        public int? Year { get; set; }
    }

    public interface IAiMovieGuesser
    {
        Task<IReadOnlyList<TitleCandidate>> SugerirTitulosAsync(string descripcion, int maxResultados = 5, CancellationToken ct = default);
    }
}
