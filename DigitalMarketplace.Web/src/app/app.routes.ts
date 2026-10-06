import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layouts/main-layout/main-layout')
        .then(m => m.MainLayout),

    children: [
  {
    path: '',
    loadComponent: () =>
      import('./pages/home/home')
        .then(m => m.Home)
  },

  {
    path: 'products',
    loadComponent: () =>
      import('./features/products/components/product-list/product-list')
        .then(m => m.ProductList)
  },
  {
    path: 'products/:slug',
    loadComponent: () =>
      import('./features/products/components/product-detail/product-detail')
        .then(m => m.ProductDetail)
  }
]
  }
];