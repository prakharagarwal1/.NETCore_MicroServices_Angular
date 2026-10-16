import { CurrencyPipe } from '@angular/common';
import { Component, input, output } from '@angular/core';

import { Product } from '../../models/product';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CurrencyPipe],
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss'
})
export class ProductCard {

  // Data received from the parent
  product = input.required<Product>();

  // Event sent from the child to the parent
  productAdded = output<Product>();
  add = output<Product>();

  addToCart(): void {
    this.productAdded.emit(this.product());
  }
}