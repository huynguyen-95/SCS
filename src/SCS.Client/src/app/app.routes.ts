import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./layouts/layout.component').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full'
      },
      {
        path: 'home',
        loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent)
      },
      {
        path: 'premise/:id',
        loadComponent: () => import('./pages/premise/premise.component').then(m => m.PremiseComponent)
      },
      {
        path: 'admin/alarm-system-simulation',
        loadComponent: () => import('./pages/alarm-system-simulation/alarm-system-simulation.component').then(m => m.AlarmSystemSimulationComponent)
      },
      {
        path: 'admin/capture-incident-simulation',
        loadComponent: () => import('./pages/capture-incident-simulation/capture-incident-simulation.component').then(m => m.CaptureIncidentSimulationComponent)
      },
      {
        path: 'admin/users',
        loadComponent: () => import('./pages/users/users.component').then(m => m.UsersComponent)
      }
    ]
  },
];
