import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GuessMovieService, GuessResponse } from '../../services/guess-movie.service';

@Component({
  selector: 'app-adivino-pelicula',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './adivino-pelicula.html',
  styles: `
    textarea { resize: vertical; }
  `
})
export class AdivinoPeliculaPage {
  private readonly svc = inject(GuessMovieService);

  description = '';
  candidates: string[] = [];

  loading = false;
  error: string | null = null;
  result: GuessResponse | null = null;

  onSubmit() {
    if (!this.description?.trim()) return;
    this.loading = true;
    this.error = null;
    this.result = null;
    this.svc.guessMovie(this.description.trim()).subscribe({
      next: (res) => {
        this.result = res;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.message || 'No se pudo adivinar la película';
        this.loading = false;
      }
    });

    // Cargar candidatos de forma oportunista (ignorar error)
    this.svc.getCandidates(this.description.trim()).subscribe({
      next: (list) => this.candidates = list || [],
      error: () => this.candidates = []
    });
  }

  reset() {
    this.description = '';
    this.result = null;
    this.error = null;
    this.candidates = [];
  }
}
