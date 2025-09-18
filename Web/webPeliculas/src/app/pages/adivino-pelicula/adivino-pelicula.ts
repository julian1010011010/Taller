import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GuessMovieService, GuessResponse } from '../../services/guess-movie.service';
import { MovieItem } from '../../services/movies.service';
import { Subscription, catchError, finalize, of, tap, timeout } from 'rxjs';

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
  private readonly cdr = inject(ChangeDetectorRef);

  description = '';
  candidates: MovieItem[] = [];

  loading = false;
  error: string | null = null;
  result: GuessResponse | null = null;
  usedAi = false;
  private submitSub?: Subscription;

  onSubmit() {
    if (!this.description?.trim()) return;
    this.loading = true;
    this.error = null;
    this.result = null;
    const desc = this.description.trim();
    let usedAi = false;
    this.submitSub?.unsubscribe();
    this.submitSub = this.svc
      .guessMovie(desc)
      .pipe(
        timeout({ each: 8000 }),
        // En caso de error, probar fallback IA
        catchError(() => {
          return this.svc.guessMovieAi(desc).pipe(
            timeout({ each: 12000 }),
            tap(() => (usedAi = true))
          );
        }),
        // Si también falla el fallback, mostrar error y continuar con null
        catchError((err) => {
          this.error = err?.message || 'No se pudo adivinar la película';
          return of(null);
        }),
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe((res) => {
        this.result = res;
        this.usedAi = usedAi;
        this.cdr.markForCheck();
      });

    // Cargar candidatos de forma oportunista (ignorar error)
    this.svc.getCandidates(this.description.trim()).subscribe({
      next: (list) => {
        this.candidates = list || [];
        this.cdr.markForCheck();
      },
      error: () => {
        this.candidates = [];
        this.cdr.markForCheck();
      }
    });
  }

  reset() {
    this.description = '';
    this.result = null;
    this.error = null;
  this.candidates = [];
    this.usedAi = true;
  }
}
