import { Routes } from '@angular/router';
import { UsersList } from './components/users/users-list/users-list';
import { UserAdd } from './components/users/user-add/user-add';
import { UserDetail } from './components/users/user-detail/user-detail';
import { Dashboard } from './components/dashboard/dashboard';

export const routes: Routes = [
    { path: 'dashboard', component: Dashboard, title: 'User Dashboard' },
    {path: 'users', component: UsersList, title: 'Users'},
    {path: 'users/add', component: UserAdd, title: 'Add New User'},
    {path: 'user/:id', component: UserDetail, title: 'User Deatils'},
    {path: '', redirectTo: '/users', pathMatch: 'full'},
    {path: '**', redirectTo: '/users'}
];
