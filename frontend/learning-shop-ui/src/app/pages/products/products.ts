import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';

import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { ProductCard } from '../../components/product-card/product-card';
@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule,ProductCard],
  templateUrl: './products.html',
  styleUrl: './products.scss'
})
export class Products implements OnInit {
  private readonly productService = inject(ProductService);

  products = signal<Product[]>([]);
  loading = signal(true);
  error = signal('');
cartCount = signal(0);

addToCart(product: Product): void {
  this.cartCount.update(count => count + 1);

  console.log('Product added to cart:', product);
}
  ngOnInit(): void {
    this.productService.getProducts().subscribe({
      next: products => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: error => {
        console.error('Failed to load products:', error);
        this.error.set('Unable to load products.');
        this.loading.set(false);
      }
    });
  }
}