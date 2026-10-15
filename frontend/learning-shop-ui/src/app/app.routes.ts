import { Routes } from '@angular/router';

import { Products } from './pages/products/products';
import { Orders } from './pages/orders/orders';

export const routes: Routes = [
  {
    path: 'products',
    component: Products
  },
  {
    path: 'orders',
    component: Orders
  },
  {
    path: '',
    redirectTo: 'products',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'products'
  }
];