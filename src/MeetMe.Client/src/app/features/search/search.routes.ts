import { Routes } from '@angular/router';

export const searchRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./search-results/search-results.component').then(m => m.SearchResultsComponent)
  }
];
