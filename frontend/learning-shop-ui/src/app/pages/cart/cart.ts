import { COMMON } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { Cart, CartItem } from '../../models/cart';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [COMMON, RouterLink],
  templateUrl: './cart.html',
  styleUrl: './cart.scss'
})
export class CartPage {
  private readonly cartService = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly fb = inject(FormBuilder);

  readonly cart = this.cartService.cart;
  readonly submitting = signal(false);
  readonly submitError = signal('');
  readonly submitSuccess = signal('');

  readonly total = computed(() => this.cart()?.total ?? 0);

  checkoutForm = this.fb.nonNullable.group({
    customerName: ['', [Validators.required, Validators.minLength(2)]],
    shippingAddress: ['', [Validators.maxLength(200)]]
  });

  updateQuantity(item: CartItem, quantity: number): void {
    if (quantity < 1) return;
    this.cartService.updateItem(item.productId, quantity).subscribe({
      error: () => this.submitError.set('Failed to update quantity.')
    });
  }

  removeItem(item: CartItem): void {
    this.cartService.removeItem(item.productId).subscribe({
      error: () => this.submitError.set('Failed to remove item.')
    });
  }

  checkout(): void {
    const cart = this.cart();
    if (!cart || cart.items.length === 0) return;

    if (!this.checkoutForm.valid) {
      this.checkoutForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set('');
    this.submitSuccess.set('');

    const formValue = this.checkoutForm.getRawValue();

    this.orderService.createOrder({
      customerName: formValue.customerName,
      items: cart.items.map(i => ({
        productId: i.productId,
        productName: i.productName,
        unitPrice: i.unitPrice,
        quantity: i.quantity
      }))
    }).subscribe({
      next: order => {
        this.submitSuccess.set(`Order #${order.id} created successfully!`);
        this.cartService.clearCart().subscribe();
        this.checkoutForm.reset();
        this.submitting.set(false);
      },
      error: () => {
        this.submitError.set('Failed to place order. Please try again.');
        this.submitting.set(false);
      }
    });
  }
}