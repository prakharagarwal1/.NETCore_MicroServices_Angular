import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface CartItem {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  category: string;
  lineTotal: number;
}

export interface Cart {
  cartId: number;
  items: CartItem[];
  total: number;
  itemCount: number;
}

export interface AddToCartRequest {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  category?: string;
}

export interface UpdateCartItemRequest {
  quantity: number;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/cart';

  readonly cart = signal<Cart | null>(null);
  readonly itemCount = signal(0);

  getCart(): Observable<Cart> {
    return this.http.get<Cart>(this.apiUrl).pipe(
      tap(cart => {
        this.cart.set(cart);
        this.itemCount.set(cart.itemCount);
      })
    );
  }

  addToCart(request: AddToCartRequest): Observable<Cart> {
    return this.http.post<Cart>(this.apiUrl, request).pipe(
      tap(cart => {
        this.cart.set(cart);
        this.itemCount.set(cart.itemCount);
      })
    );
  }

  updateItem(productId: number, quantity: number): Observable<Cart> {
    return this.http.put<Cart>(`${this.apiUrl}/${productId}`, { quantity }).pipe(
      tap(cart => {
        this.cart.set(cart);
        this.itemCount.set(cart.itemCount);
      })
    );
  }

  removeItem(productId: number): Observable<Cart> {
    return this.http.delete<Cart>(`${this.apiUrl}/${productId}`).pipe(
      tap(cart => {
        this.cart.set(cart);
        this.itemCount.set(cart.itemCount);
      })
    );
  }

  clearCart(): Observable<Cart> {
    return this.http.delete<Cart>(this.apiUrl).pipe(
      tap(cart => {
        this.cart.set(cart);
        this.itemCount.set(0);
      })
    );
  }
}