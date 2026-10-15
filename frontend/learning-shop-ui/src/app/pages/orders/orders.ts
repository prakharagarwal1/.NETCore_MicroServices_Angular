import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { OrderService } from '../../services/order.service';
import { Order } from '../../models/order';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './orders.html',
  styleUrl: './orders.scss'
})
export class Orders implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly fb = inject(FormBuilder);

  orders = signal<Order[]>([]);

  loading = signal(true);
  submitting = signal(false);

  error = signal('');
  submitError = signal('');
  submitSuccess = signal('');

  form = this.fb.nonNullable.group({
    customerName: [
      '',
      [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100)
      ]
    ],
    total: [
      0,
      [
        Validators.required,
        Validators.min(1),
        Validators.max(1000000000)
      ]
    ]
  });

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.orderService.getOrders().subscribe({
      next: orders => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: err => {
        console.error('Failed to load orders:', err);
        this.error.set('Unable to load orders.');
        this.loading.set(false);
      }
    });
  }

  createOrder(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set('');
    this.submitSuccess.set('');

    const request = this.form.getRawValue();

    this.orderService.createOrder(request).subscribe({
      next: order => {
        this.orders.update(current => [
          ...current,
          order
        ]);

        this.submitSuccess.set(
          `Order #${order.id} created successfully.`
        );

        this.form.reset({
          customerName: '',
          total: 0
        });

        this.submitting.set(false);
      },

      error: err => {
        console.error('Failed to create order:', err);

        this.submitError.set(
          'Unable to create order. Please try again.'
        );

        this.submitting.set(false);
      }
    });
  }
}