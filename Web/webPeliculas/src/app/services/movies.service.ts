import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

// API raw item with hash-prefixed keys
export interface ApiMovieItem {
  '#TITLE': string;
  '#YEAR': number;
  '#IMDB_ID': string;
  '#ACTORS': string;
  '#IMDB_URL': string;
  '#IMG_POSTER': string;
}

export interface MovieItem {
  title: string;
  year: number;
  imdbId: string;
  actors: string;
  imdbUrl: string;
  poster: string;
}

@Injectable({ providedIn: 'root' })
export class MoviesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/Movie'; // will be proxied to https://localhost:7247

  listMovies(titulo = 'Matrix', tipo = '', pagina = 1, version = 1): Observable<MovieItem[]> {
    const params = new HttpParams()
      .set('titulo', titulo)
      .set('tipo', tipo)
      .set('pagina', pagina)
      .set('version', version);

    return this.http
      .get<ApiMovieItem[]>(`${this.baseUrl}/ListMovies`, { params })
      .pipe(map(items => items?.map(mapApiMovie) ?? []));
  }
}

export function mapApiMovie(api: ApiMovieItem): MovieItem {
  return {
    title: api['#TITLE'],
    year: api['#YEAR'],
    imdbId: api['#IMDB_ID'],
    actors: api['#ACTORS'],
    imdbUrl: api['#IMDB_URL'],
    poster: api['#IMG_POSTER'],
  };
}
