import { Routes } from '@angular/router';
import { PeliculasInfo } from './pages/peliculas-info/peliculas-info';
import { HojaDeVida } from './pages/hoja-de-vida/hoja-de-vida';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'peliculas' },
	{ path: 'peliculas', component: PeliculasInfo },
	{ path: 'hoja-de-vida', component: HojaDeVida },
	{ path: '**', redirectTo: 'peliculas' },
];
