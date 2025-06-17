import { Routes } from '@angular/router';
import { Home } from './components/home/home';
import { About } from './components/about/about';
import { ProductDetail } from './components/product-detail/product-detail';
import { authGuard } from './auth-guard';
import { Login } from './components/login/login';

export const routes: Routes = [
  { path: 'home', component: Home, title: 'Home | Product Search', canActivate: [authGuard] },
  {path: 'product/:id', component:ProductDetail, title: 'Product Details', canActivate: [authGuard]},
  { path: 'about', component: About, title: 'About Us' },
  {path:'login', component:Login},
  { path: '', redirectTo: '/home', pathMatch: 'full' }, // Default route
  { path: '**', redirectTo: '/home' } // Wildcard route for non existing,
];
