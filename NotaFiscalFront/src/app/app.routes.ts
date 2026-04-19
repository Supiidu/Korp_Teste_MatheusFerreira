import { Routes } from '@angular/router';
import { Estoque } from './pages/estoque/estoque';
import { Notas } from './pages/notas/notas';

export const routes: Routes = [
  { path: 'estoque', component: Estoque },
  { path: 'notas', component: Notas },
];
