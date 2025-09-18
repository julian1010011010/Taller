import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MovieItem, MoviesService } from '../../services/movies.service'; 
import { SafeResourceUrl } from '@angular/platform-browser';
// Avoid DI for platform checks to prevent provider resolution issues

@Component({
  selector: 'app-movie-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './movie-detail.html',
  styles: `:host { display: block; }`
})
export class MovieDetailPage {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly moviesService = inject(MoviesService);
 

  // Movie state passed via navigation or loaded by param in the future
  private readonly stateMovie = this.router.getCurrentNavigation()?.extras?.state?.['movie'] as (MovieItem & { plot?: string; images?: string[] }) | undefined;

  movie = signal<(MovieItem & { plot?: string; images?: string[] }) | undefined>(this.stateMovie);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  
  // Video state
  videoUrl = signal<SafeResourceUrl | undefined>(undefined);
  videoLoading = signal<boolean>(false);
  videoError = signal<string | null>(null);
  // Gallery + description
  images = signal<string[]>([]);
  description = signal<string>('');

  // YouTube trailer search embed via IFrame API
  private ytPlayer?: any;
  ytReady = signal(false);
  ytSearchUrl = signal<string>('');
  private currentVideoObjectUrl?: string;

  ngOnInit() {
    const m = this.movie();
    
    // If no movie from state, try to get param and search for the movie
    if (!m) {
      const imdbId = this.route.snapshot.paramMap.get('imdbId');
      const title = this.route.snapshot.paramMap.get('title');
      
      if (title) {
        this.searchMovieByTitle(title);
      } else if (imdbId) {
        // Fallback: create minimal movie object with imdbId
        const imdbUrl = `https://www.imdb.com/title/${imdbId}/`;
        this.movie.set({ title: imdbId, year: NaN as any, imdbId, actors: '', imdbUrl, poster: '' });
        this.loadVideoMedia(imdbId);
      }
    } else {
      // If we have a movie but it might be missing details, try to enrich it
      this.enrichMovieData(m);
      // Load video if we have imdbId
      if (m.imdbId) {
        this.loadVideoMedia(m.imdbId);
      }
    }

    this.setupMovieDisplay();
  }

  private loadVideoMedia(imdbId: string) {
    if (!imdbId) {
      console.warn('loadVideoMedia: No se proporcionó imdbId');
      this.videoError.set('ID de película no disponible');
      return;
    }

    this.videoLoading.set(true);
    this.videoError.set(null);
    console.log('loadVideoMedia: Cargando video para imdbId:', imdbId);

    this.moviesService.getMedia(imdbId).subscribe({
      next: (url) => {
        console.log('loadVideoMedia: Video cargado exitosamente');
        // Clean up previous URL if exists
        if (this.currentVideoObjectUrl) {
          URL.revokeObjectURL(this.currentVideoObjectUrl);
        }
        // Store current URL for cleanup
        this.currentVideoObjectUrl = (url as any)._objectUrl;
        this.videoUrl.set(url);
        this.videoLoading.set(false);
      },
      error: (error) => {
        console.error('loadVideoMedia: Error al cargar video:', error);
        this.videoError.set('No se pudo cargar el video');
        this.videoLoading.set(false);
      }
    });
  }

  private searchMovieByTitle(title: string) {
    this.loading.set(true);
    this.error.set(null);
    
    this.moviesService.listMovies(title).subscribe({
      next: (movies) => {
        this.loading.set(false);
        if (movies.length > 0) {
          // Take the first movie that matches
          this.movie.set(movies[0]);
          this.setupMovieDisplay();
          // Load video after setting movie
          if (movies[0].imdbId) {
            this.loadVideoMedia(movies[0].imdbId);
          }
        } else {
          this.error.set('No se encontró información de la película');
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.error.set('Error al cargar la información de la película');
        console.error('Error loading movie:', error);
      }
    });
  }

  private enrichMovieData(currentMovie: MovieItem & { plot?: string; images?: string[] }) {
    // If the current movie lacks detailed information, try to fetch more
    if (!currentMovie.actors || !currentMovie.poster) {
      this.moviesService.listMovies(currentMovie.title).subscribe({
        next: (movies) => {
          const enrichedMovie = movies.find(m => 
            m.imdbId === currentMovie.imdbId || 
            (m.title === currentMovie.title && m.year === currentMovie.year)
          );
          
          if (enrichedMovie) {
            // Merge the enriched data with existing data
            this.movie.set({
              ...currentMovie,
              ...enrichedMovie,
              plot: currentMovie.plot, // Keep existing plot if available
              images: currentMovie.images // Keep existing images if available
            });
            this.setupMovieDisplay();
            // Load video after enriching data
            if (enrichedMovie.imdbId) {
              this.loadVideoMedia(enrichedMovie.imdbId);
            }
          }
        },
        error: (error) => {
          console.error('Error enriching movie data:', error);
          // Continue with existing data
        }
      });
    }
  }

  private setupMovieDisplay() {
  // Setup gallery images (poster fallback)
  const mm = this.movie();
  const imgs = mm?.images?.length ? mm.images : (mm?.poster ? [mm.poster] : []);
  this.images.set(imgs);

    // Description: plot or fallback synthesized
    const desc = (mm as any)?.plot
      || (mm ? `Película de ${mm.year || 'año desconocido'} protagonizada por ${mm.actors || 'elenco desconocido'}.` : 'Sin descripción disponible.');
    this.description.set(desc);

    // Setup YouTube player if we have movie data
    if (mm?.title) {
      this.prepareSearchPlayer(mm.title, mm.year);
    }
  }

  private prepareSearchPlayer(title?: string, year?: number) {
    if (!title) return;
    const q = `${title} ${year || ''} trailer`.trim();
    this.ytSearchUrl.set(`https://www.youtube.com/results?search_query=${encodeURIComponent(q)}`);
    if (this.isBrowser()) {
      this.loadYouTubeApi().then(() => {
        // @ts-ignore
        const YT = (window as any).YT;
        this.ytPlayer = new YT.Player('yt-player', {
          width: '100%',
          height: '100%',
          playerVars: { listType: 'search', list: q, modestbranding: 1, rel: 0, playsinline: 1 },
          events: {
            onReady: (e: any) => { this.ytReady.set(true); try { e.target.playVideo(); } catch {} },
            onError: () => { try { this.ytPlayer?.nextVideo?.(); } catch {} }
          }
        });
      }).catch(() => {/* ignore */});
    }
  }

  ngOnDestroy() {
    try { this.ytPlayer?.destroy?.(); } catch {}
    // Clean up video object URL to prevent memory leaks
    if (this.currentVideoObjectUrl) {
      URL.revokeObjectURL(this.currentVideoObjectUrl);
    }
  }

  private loadYouTubeApi(): Promise<void> {
    return new Promise((resolve, reject) => {
      // Already loaded
      // @ts-ignore
      if ((window as any).YT?.Player) return resolve();
      const w = window as any;
      const prev = w.onYouTubeIframeAPIReady;
      w.onYouTubeIframeAPIReady = () => { prev?.(); resolve(); };
      const script = document.createElement('script');
      script.src = 'https://www.youtube.com/iframe_api';
      script.async = true;
      script.onerror = () => reject();
      document.head.appendChild(script);
    });
  }

  private isBrowser(): boolean {
    return typeof window !== 'undefined' && typeof document !== 'undefined';
  }
}
