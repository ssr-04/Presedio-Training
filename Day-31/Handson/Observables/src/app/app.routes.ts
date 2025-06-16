import { Routes } from '@angular/router';
import { Home } from './components/home/home';
import { About } from './components/about/about';

export const routes: Routes = [
     { path: 'home', component: Home, title: 'Home | Product Search' },
  { path: 'about', component: About, title: 'About Us' },
  { path: '', redirectTo: '/home', pathMatch: 'full' }, // Default route
  { path: '**', redirectTo: '/home' } // Wildcard route for non existing
];
