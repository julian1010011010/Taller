import { Route } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AboutComponent } from './about/about.component';
import { MoviesListComponent } from './movies-list/movies-list.component';

export const routes: Route[] = [
  { path: '', component: HomeComponent },
  { path: 'about', component: AboutComponent },
  { path: 'movies', component: MoviesListComponent },
  { path: '**', redirectTo: '' }
];
