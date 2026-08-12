import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard, roleGuard(['Admin'])],
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
  },
  {
    path: 'users',
    canActivate: [authGuard, roleGuard(['Admin'])],
    loadComponent: () => import('./features/users/users.component').then((m) => m.UsersComponent)
  },
  {
    path: 'tickets',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/tickets/ticket-list/ticket-list.component').then((m) => m.TicketListComponent)
  },
  {
    path: 'tickets/create',
    canActivate: [authGuard, roleGuard(['Customer'])],
    loadComponent: () =>
      import('./features/tickets/ticket-create/ticket-create.component').then(
        (m) => m.TicketCreateComponent
      )
  },
  {
    path: 'tickets/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/tickets/ticket-details/ticket-details.component').then(
        (m) => m.TicketDetailsComponent
      )
  },
  { path: '', pathMatch: 'full', redirectTo: 'tickets' },
  { path: '**', redirectTo: 'tickets' }
];
