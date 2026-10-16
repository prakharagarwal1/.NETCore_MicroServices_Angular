import { Component, inject, signal, effect } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { Order, CreateOrderRequest } from '../../models/order';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss'
})
export class Checkout {
  private readonly fb = inject(FormBuilder);
  private readonly cartService = inject(CartService);
  private readonly orderService = inject(OrderService);
  private destroy$ = new Subject<void>();

  orderForm: FormGroup = this.fb.group({
    customerName: ['', [Validators.required, Validators.minLength(2)]],
    total: [0, [Validators.required, Validators.min(1)]]
  });
  loading = signal(false);
  error = signal('');
  cartItemCount = this.cartService.itemCount;
  cartTotal = this.cartService.cart()?.total ?? 0;

  constructor() {
    // Effect: sync form total with cart total
    effect(() => {
      const total = this.cartService.cart()?.total ?? 0;
      this.orderForm.patchValue({ total }, { emitEvent: false });
    });
  }

  mustShowError(controlName: string): boolean {
    const control = this.orderForm.controls[controlName];
    return control.touched && control.errors ? true : false;
  }

  onSubmit(): void {
    if (this.orderForm.invalid) return;

    this.loading.set(true);
    this.error.set('');

    const cn = this.orderForm.get('customerName')!.value;
    const total = this.orderForm.get('total')!.value;

    const request: CreateOrderRequest = {
      customerName: cn,
    };

    this.orderService.createOrder(request).subscribe({
      next: () => {
        this.cartService.clearCart().subscribe();
        this.loading.set(false);
        this.orderForm.reset({
          customerName: '',
          total: this.cartService.cart()?.total ?? 0
        });
      },
      error: () => {
        this.error.set('Failed to create order. Please try again.');
        this.loading.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}