import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormGroup, FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { CartService } from '../../services/cart.service';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product';
import { ProductCard } from '../../components/product-card/product-card';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ProductCard],
  templateUrl: './products.html',
  styleUrl: './products.scss'
})
export class Products {
  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);

  products = signal<Product[]>([]);
  categories = computed(() => {
    const cats = new Set(this.products().map(p => p.category).filter(Boolean));
    return [...cats].sort();
  });
  loading = signal(true);
  error = signal('');
  searchQuery = signal('');
  selectedCategory = signal('');

  readonly cartCount = this.cartService.itemCount;

  searchForm = new FormGroup({
    query: new FormControl('')
  });

  get filteredProducts(): Product[] {
    let list = this.products();
    const q = this.searchQuery().toLowerCase().trim();
    const cat = this.selectedCategory();

    if (q) {
      list = list.filter(p =>
        p.name.toLowerCase().includes(q) ||
        p.description.toLowerCase().includes(q) ||
        p.category.toLowerCase().includes(q)
      );
    }
    if (cat) {
      list = list.filter(p => p.category === cat);
    }
    return list;
  }

  addToCart(product: Product): void {
    this.cartService.addToCart({
      productId: product.id,
      productName: product.name,
      unitPrice: product.price,
      quantity: 1,
      category: product.category
    }).subscribe({
      error: () => this.error.set('Failed to add item to cart.')
    });
  }

  selectCategory(cat: string): void {
    this.selectedCategory.set(this.selectedCategory() === cat ? '' : cat);
  }

  onSearch(): void {
    this.searchQuery.set(this.searchForm.value.query ?? '');
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.searchForm.patchValue({ query: '' });
  }

  ngOnInit(): void {
    this.productService.getProducts().subscribe({
      next: products => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load products.');
        this.loading.set(false);
      }
    });
  }
}