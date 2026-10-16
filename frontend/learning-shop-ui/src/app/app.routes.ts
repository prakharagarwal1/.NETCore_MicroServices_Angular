import { Routes } from '@angular/router';

import { Products } from './pages/products/products';
import { Orders } from './pages/orders/orders';
import { Checkout } from './pages/checkout/checkout';
import { RxjsDemo } from './pages/rxjs-demo/rxjs-demo';
import { ProductCrud } from './pages/product-crud/product-crud';

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
    path: 'checkout',
    component: Checkout
  },
  {
    path: 'rxjs-demo',
    component: RxjsDemo
  },
  {
    path: 'product-crud',
    component: ProductCrud
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