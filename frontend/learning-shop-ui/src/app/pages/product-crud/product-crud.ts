import { Component, inject, signal, computed, effect } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { of } from 'rxjs';
import {
  debounceTime,
  distinctUntilChanged,
  switchMap,
  tap,
  catchError,
  finalize,
} from 'rxjs/operators';

import { ProductService } from '../../services/product.service';
import {
  Product,
  CreateProductRequest,
  UpdateProductRequest,
} from '../../models/product';

type FormMode = 'create' | 'edit' | 'none';

@Component({
  selector: 'app-product-crud',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-crud.html',
  styleUrl: './product-crud.scss',
})
export class ProductCrud {
  private readonly productService = inject(ProductService);
  private readonly fb = inject(FormBuilder);

  // ================================================================
  // State signals
  // ================================================================
  products = signal<Product[]>([]);
  loading = signal(false);
  saving = signal(false);
  deletingId = signal<number | null>(null);
  error = signal('');
  success = signal('');

  // Filter / search state
  searchQuery = signal('');
  selectedCategory = signal('');
  activeOnly = signal(true);

  // Form mode: 'create' | 'edit' | 'none'
  formMode = signal<FormMode>('none');
  editingId = signal<number | null>(null);

  // ================================================================
  // Derived
  // ================================================================
  categories = computed(() => {
    const cats = new Set(this.products().map((p) => p.category).filter(Boolean));
    return [...cats].sort();
  });

  filteredProducts = computed(() => {
    let list = this.products();
    const q = this.searchQuery().toLowerCase().trim();
    const cat = this.selectedCategory();

    if (q) {
      list = list.filter(
        (p) =>
          p.name.toLowerCase().includes(q) ||
          p.description.toLowerCase().includes(q) ||
          p.category.toLowerCase().includes(q)
      );
    }
    if (cat) {
      list = list.filter((p) => p.category === cat);
    }
    if (this.activeOnly()) {
      list = list.filter((p) => p.isActive);
    }
    return list;
  });

  isEditMode = computed(() => this.formMode() === 'edit');
  isCreateMode = computed(() => this.formMode() === 'create');

  // ================================================================
  // Reactive Form
  // ================================================================
  productForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
    price: [0, [Validators.required, Validators.min(0.01)]],
    stock: [0, [Validators.required, Validators.min(0)]],
    category: ['', [Validators.required, Validators.minLength(2)]],
    isActive: [true],
  });

  // ================================================================
  // Lifecycle
  // ================================================================
  constructor() {
    // Reset form when mode changes to create
    effect(() => {
      if (this.formMode() === 'create') {
        this.productForm.reset({
          name: '',
          description: '',
          price: 0,
          stock: 0,
          category: '',
          isActive: true,
        });
        this.editingId.set(null);
        this.error.set('');
        this.success.set('');
      }
    });

    // Load products on init using RxJS stream
    this.loadProducts();
  }

  // ================================================================
  // CRUD Operations
  // ================================================================

  /** READ - Load products */
  loadProducts(): void {
    this.loading.set(true);
    this.error.set('');

    this.productService
      .getProducts(this.selectedCategory() || undefined, this.activeOnly())
      .pipe(
        tap(() => this.loading.set(false)),
        catchError((err) => {
          this.error.set('Failed to load products.');
          this.loading.set(false);
          return of([]);
        })
      )
      .subscribe({
        next: (products) => this.products.set(products),
      });
  }

  /** READ - Load single product for editing */
  loadProductForEdit(id: number): void {
    this.loading.set(true);
    this.error.set('');

    this.productService
      .getProduct(id)
      .pipe(
        tap(() => this.loading.set(false)),
        catchError((err) => {
          this.error.set('Failed to load product for editing.');
          this.loading.set(false);
          return of(null as unknown as Product);
        })
      )
      .subscribe({
        next: (product) => {
          if (!product) return;
          this.editingId.set(id);
          this.formMode.set('edit');
          this.productForm.patchValue({
            name: product.name,
            description: product.description ?? '',
            price: product.price,
            stock: product.stock,
            category: product.category ?? '',
            isActive: product.isActive,
          });
          this.scrollToForm();
        },
      });
  }

  /** CREATE - Add a new product */
  createProduct(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.success.set('');

    const request: CreateProductRequest = this.productForm.getRawValue();

    this.productService
      .createProduct(request)
      .pipe(
        tap(() => this.saving.set(false)),
        finalize(() => this.saving.set(false)),
        catchError((err) => {
          this.error.set('Failed to create product.');
          this.saving.set(false);
          return of(null as unknown as Product);
        })
      )
      .subscribe({
        next: (product) => {
          if (!product) return;
          this.products.update((list) => [...list, product]);
          this.success.set(`Product "${product.name}" created successfully.`);
          this.cancelForm();
          this.loadProducts(); // refresh list
        },
      });
  }

  /** UPDATE - Modify an existing product */
  updateProduct(): void {
    if (this.productForm.invalid || !this.editingId()) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.success.set('');

    const id = this.editingId()!;
    const request: UpdateProductRequest = this.productForm.getRawValue();

    this.productService
      .updateProduct(id, request)
      .pipe(
        tap(() => this.saving.set(false)),
        finalize(() => this.saving.set(false)),
        catchError((err) => {
          this.error.set('Failed to update product.');
          this.saving.set(false);
          return of(null as unknown as Product);
        })
      )
      .subscribe({
        next: (product) => {
          if (!product) return;
          this.products.update((list) =>
            list.map((p) => (p.id === id ? product : p))
          );
          this.success.set(`Product "${product.name}" updated successfully.`);
          this.cancelForm();
          this.loadProducts();
        },
      });
  }

  /** DELETE - Remove a product */
  deleteProduct(id: number, name: string): void {
    if (!confirm(`Are you sure you want to delete "${name}"?`)) return;

    this.deletingId.set(id);
    this.error.set('');

    this.productService
      .deleteProduct(id)
      .pipe(
        tap(() => this.deletingId.set(null)),
        finalize(() => this.deletingId.set(null)),
        catchError((err) => {
          this.error.set('Failed to delete product.');
          this.deletingId.set(null);
          return of(undefined);
        })
      )
      .subscribe({
        next: () => {
          this.products.update((list) => list.filter((p) => p.id !== id));
          this.success.set(`Product "${name}" deleted successfully.`);
        },
      });
  }

  // ================================================================
  // Form UI Helpers
  // ================================================================

  openCreateForm(): void {
    this.formMode.set('create');
    this.error.set('');
    this.success.set('');
    this.scrollToForm();
  }

  cancelForm(): void {
    this.formMode.set('none');
    this.editingId.set(null);
    this.productForm.reset({
      name: '',
      description: '',
      price: 0,
      stock: 0,
      category: '',
      isActive: true,
    });
    this.error.set('');
    this.success.set('');
  }

  saveProduct(): void {
    if (this.isEditMode()) {
      this.updateProduct();
    } else {
      this.createProduct();
    }
  }

  private scrollToForm(): void {
    setTimeout(() => {
      const el = document.querySelector('.product-form-section');
      el?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 0);
  }

  // ================================================================
  // Filter helpers
  // ================================================================
  setSearch(term: string): void {
    this.searchQuery.set(term);
  }

  setCategory(cat: string): void {
    this.selectedCategory.set(this.selectedCategory() === cat ? '' : cat);
  }

  toggleActiveOnly(): void {
    this.activeOnly.set(!this.activeOnly());
    this.loadProducts();
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.selectedCategory.set('');
    this.activeOnly.set(true);
    this.loadProducts();
  }

  // ================================================================
  // Form error helpers (for template)
  // ================================================================
  getFieldError(controlName: string): string {
    const control = this.productForm.get(controlName);
    if (!control || !control.touched || !control.errors) return '';

    const errors = control.errors;
    if (errors['required']) return `${this.labelFor(controlName)} is required.`;
    if (errors['minlength'])
      return `Minimum ${errors['minlength'].requiredLength} characters.`;
    if (errors['maxlength'])
      return `Maximum ${errors['maxlength'].requiredLength} characters.`;
    if (errors['min']) return `Must be at least ${errors['min'].min}.`;
    return '';
  }

  private labelFor(controlName: string): string {
    const labels: Record<string, string> = {
      name: 'Name',
      description: 'Description',
      price: 'Price',
      stock: 'Stock',
      category: 'Category',
    };
    return labels[controlName] || controlName;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(value);
  }
}
