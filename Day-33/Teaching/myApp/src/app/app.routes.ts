import { Routes } from '@angular/router';
import { First } from './first/first';
import { Login } from './login/login';
import { Products } from './products/products';
import { Home } from './home/home';
import { Profile } from './profile/profile';
import { AuthGuard } from './guards/auth-guard-guard';

export const routes: Routes = [
    {path:'landing', component:First},
    {path:'login', component:Login},
    {path:'products', component:Products},
    {path:'home/:uname', component:Home, children:
        [
            {path:'first', component:First},
            {path:'products', component:Products}
        ]
    },
    {path: 'profile', component:Profile, canActivate:[AuthGuard]}
];
