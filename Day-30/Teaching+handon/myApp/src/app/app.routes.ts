import { Routes } from '@angular/router';
import { Products } from './products/products';
import { Login } from './login/login';
import { Dashboard } from './dashboard/dashboard';

export const routes: Routes = [
    {path: '', redirectTo: '/products', pathMatch: 'full'},
    {path: 'products', component: Products},
    {path: 'login', component: Login},
    {path: 'dashboard', component: Dashboard},
    {path: '**', redirectTo: '/products'}
];
