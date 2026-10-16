import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { Product, CreateProductRequest, UpdateProductRequest } from '../models/product';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/products';

  getProducts(category?: string, activeOnly = true): Observable<Product[]> {
    let params: Record<string, string> = {};
    if (category) params['category'] = category;
    if (!activeOnly) params['activeOnly'] = 'false';

    const qs = new URLSearchParams(params).toString();
    return this.http.get<Product[]>(`${this apiUrl}${qs ? '?' + qs : ''}`);
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this apiUrl}/${id}`);
  }

  createProduct(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this apiUrl, request);
  }

  updateProduct(id: number, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this apiUrl}/${id}`, request);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this apiUrl}/${id}`);
  }
}