import { Routes } from '@angular/router';
import { PeliculasInfo } from './pages/peliculas-info/peliculas-info';
import { HojaDeVida } from './pages/hoja-de-vida/hoja-de-vida';
import { AdivinoPeliculaPage } from './pages/adivino-pelicula/adivino-pelicula';
import { MovieDetailPage } from './pages/movie-detail/movie-detail';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'peliculas' },
	{ path: 'peliculas', component: PeliculasInfo },
	{ path: 'pelicula/:imdbId', component: MovieDetailPage },
	{ path: 'adivino', component: AdivinoPeliculaPage },
	{ path: 'hoja-de-vida', component: HojaDeVida },
	{ path: '**', redirectTo: 'peliculas' },
];
