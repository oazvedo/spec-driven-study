import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'usuarios' },
  {
    path: 'usuarios',
    title: 'Usuários',
    loadComponent: () => import('../pages/usuarios-page').then((m) => m.UsuariosPage),
  },
];
