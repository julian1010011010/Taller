import { Component, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MoviesService, MovieItem } from '../../services/movies.service';

@Component({
  selector: 'app-peliculas-info',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './peliculas-info.html',
  styles: `
    .movie-poster { height: 280px; object-fit: cover; }
  `
})
export class PeliculasInfo {
  private readonly moviesSvc = inject(MoviesService);
  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  query = signal('Matrix');
  tipo = signal(''); // movie, series, etc.
  page = signal(1);
  loading = signal(false);
  error = signal<string | null>(null);
  movies = signal<MovieItem[]>([]);

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.search();
    }
  }

  search() {
    this.loading.set(true);
    this.error.set(null);
    this.moviesSvc.listMovies(this.query(), this.tipo(), this.page(), 1)
      .subscribe({
        next: items => {
          this.movies.set(items);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err?.message ?? 'Error al cargar películas');
          this.loading.set(false);
        }
      });
  }

  nextPage() { this.page.set(this.page() + 1); this.search(); }
  prevPage() { if (this.page() > 1) { this.page.set(this.page() - 1); this.search(); } }
  resetPageAndSearch() { this.page.set(1); this.search(); }
}
