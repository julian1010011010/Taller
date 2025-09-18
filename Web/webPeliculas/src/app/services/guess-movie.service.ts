import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiMovieItem, MovieItem, mapApiMovie } from './movies.service';

export interface GuessResponse {
  title: string;
  year?: string | number;
  poster?: string;
  plot?: string;
  type?: string; // movie/series
  confidence?: number; // 0..1
  suggestions?: Array<string>;
}

@Injectable({ providedIn: 'root' })
export class GuessMovieService {
  private readonly http = inject(HttpClient);

  // Backend ASP.NET: MovieController -> [HttpPost("adivinar")]
  // Se espera un string plano en el cuerpo con la descripción
  guessMovie(description: string): Observable<GuessResponse> {
    // Enviar como JSON string literal ("texto") para que [FromBody] string lo vincule
    return this.http.post<GuessResponse>('/api/Movie/adivinar', JSON.stringify(description), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  // Obtener candidatos con info completa ([HttpPost("candidatos")])
  // El backend devuelve una lista como la del buscador con claves #TITLE, etc.
  getCandidates(description: string): Observable<MovieItem[]> {
    return this.http
      .post<ApiMovieItem[]>('/api/Movie/candidatos', JSON.stringify(description), {
        headers: { 'Content-Type': 'application/json' }
      })
      .pipe(map(items => items?.map(mapApiMovie) ?? []));
  }
}
