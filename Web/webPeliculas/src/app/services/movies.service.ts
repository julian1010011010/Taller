import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map, catchError, throwError } from 'rxjs';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

// API raw item
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
  private readonly sanitizer = inject(DomSanitizer);
  private readonly baseUrl = '/api/Movie'; // proxy a https://localhost:7247

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

  // Nuevo: obtener trailer como SafeResourceUrl para usarlo en <video>
  getMedia(imdbId: string): Observable<SafeResourceUrl> {
    if (!imdbId) {
      console.error('getMedia: imdbId es requerido');
      return throwError(() => new Error('IMDb ID es requerido'));
    }

    const url = `${this.baseUrl}/media/${imdbId}`;
    console.log('getMedia: Solicitando video para imdbId:', imdbId);
    console.log('getMedia: URL del endpoint:', url);
    
    return this.http.get(url, { responseType: 'blob', observe: 'response' })
      .pipe(
        map(res => {
          console.log('getMedia: Respuesta recibida:', {
            status: res.status,
            contentType: res.headers.get('Content-Type'),
            bodySize: res.body?.size
          });

          if (!res.body) {
            throw new Error('No se recibió contenido del servidor');
          }

          const type = res.headers.get('Content-Type') ?? 'video/mp4';
          const blob = new Blob([res.body], { type });
          const objectUrl = URL.createObjectURL(blob);
          
          console.log('getMedia: URL del objeto creada:', objectUrl);
          
          // Almacenar la URL para limpieza posterior
          const safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
          (safeUrl as any)._objectUrl = objectUrl; // Agregamos la URL original para limpieza
          
          return safeUrl;
        }),
        catchError(error => {
          console.error('getMedia: Error al obtener el video:', error);
          return throwError(() => error);
        })
      );
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
