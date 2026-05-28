import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { HomeComponent } from './home/home.component';
import { authGuard } from './auth/auth.guard';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { UsersComponent } from './features/users/users.component';
import { UserFormComponent } from './features/users/user-form.component';
import { RolesComponent } from './features/roles/roles.component';
import { RoleFormComponent } from './features/roles/role-form.component';
import { SettingsComponent } from './features/settings/settings.component';
import { UserSettingsComponent } from './features/user-settings/user-settings.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: HomeComponent,
    canActivate: [authGuard],
    children: [
      { path: 'home', component: DashboardComponent },
      { path: 'users', component: UsersComponent },
      { path: 'users/new', component: UserFormComponent },
      { path: 'roles', component: RolesComponent },
      { path: 'roles/new', component: RoleFormComponent },
      { path: 'admin/settings', component: SettingsComponent },
      { path: 'settings/user', component: UserSettingsComponent },
      { path: '', pathMatch: 'full', redirectTo: 'home' }
    ]
  },
  { path: '**', redirectTo: '' }
];
