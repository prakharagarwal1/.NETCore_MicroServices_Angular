import { Component, inject, signal, effect } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import {
  interval,
  of,
  Subject,
  BehaviorSubject,
  combineLatest,
} from 'rxjs';
import {
  map,
  filter,
  debounceTime,
  distinctUntilChanged,
  switchMap,
  scan,
  tap,
  take,
} from 'rxjs/operators';

import { RxjsExamplesService } from '../../services/rxjs-examples.service';

@Component({
  selector: 'app-rxjs-demo',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './rxjs-demo.html',
  styleUrl: './rxjs-demo.scss',
})
export class RxjsDemo {
  private readonly rxjsService = inject(RxjsExamplesService);
  private readonly fb = inject(FormBuilder);

  // ================================================================
  // 1. SEARCH DEMO — debounceTime + distinctUntilChanged + switchMap
  // ================================================================
  searchForm = this.fb.nonNullable.group({
    term: [''],
  });

  searchResults = signal<{ id: number; name: string; category: string }[]>([]);
  searchLoading = signal(false);
  searchQuery = signal('');

  private readonly fakeProducts = [
    { id: 1, name: 'Wireless Mouse', category: 'Electronics' },
    { id: 2, name: 'Mechanical Keyboard', category: 'Electronics' },
    { id: 3, name: 'USB-C Hub', category: 'Electronics' },
    { id: 4, name: 'Cotton T-Shirt', category: 'Clothing' },
    { id: 5, name: 'Denim Jeans', category: 'Clothing' },
    { id: 6, name: 'Coffee Maker', category: 'Home' },
    { id: 7, name: 'Blender', category: 'Home' },
    { id: 8, name: 'Yoga Mat', category: 'Sports' },
    { id: 9, name: 'Dumbbells', category: 'Sports' },
    { id: 10, name: 'Running Shoes', category: 'Sports' },
  ];

  // ================================================================
  // 2. COUNTER DEMO — interval + scan + take
  // ================================================================
  counter = signal(0);
  counterRunning = signal(false);
  counterHistory = signal<number[]>([]);

  // ================================================================
  // 3. FORM STREAM DEMO — combineLatest + map
  // ================================================================
  calcForm = this.fb.nonNullable.group({
    a: [0, Validators.required],
    b: [0, Validators.required],
  });

  calculationResult = signal(0);
  calculationHistory = signal<string[]>([]);

  // ================================================================
  // 4. SUBJECT DEMO — Subject + BehaviorSubject
  // ================================================================
  clickCount = signal(0);
  clickLog = signal<string[]>([]);

  private clickSubject = new Subject<void>();
  private behaviorCount = new BehaviorSubject<number>(0);

  // ================================================================
  // 5. OPERATOR SHOWCASE
  // ================================================================
  operatorResults = signal<{ name: string; result: string }[]>([]);

  constructor() {
    // --- Search: debounceTime + distinctUntilChanged + switchMap ---
    this.searchForm.get('term')!.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged(),
        tap(() => this.searchLoading.set(true)),
        switchMap((term) => {
          this.searchQuery.set(term);
          if (!term.trim()) {
            this.searchResults.set([]);
            this.searchLoading.set(false);
            return of([]);
          }
          return of(this.fakeProducts).pipe(
            map((products) =>
              products.filter((p) =>
                p.name.toLowerCase().includes(term.toLowerCase())
              )
            )
          );
        })
      )
      .subscribe({
        next: (results) => {
          this.searchResults.set(results);
          this.searchLoading.set(false);
        },
        error: () => {
          this.searchLoading.set(false);
        },
      });

    // --- Calculator: combineLatest + map ---
    combineLatest([
      this.calcForm.get('a')!.valueChanges,
      this.calcForm.get('b')!.valueChanges,
    ])
      .pipe(
        map(([a, b]) => Number(a) + Number(b)),
        tap((result) => this.calculationResult.set(result))
      )
      .subscribe((result) => {
        this.calculationHistory.update((h) => [
          ...h,
          `${this.calcForm.get('a')?.value} + ${this.calcForm.get('b')?.value} = ${result}`,
        ]);
      });

    // --- Click Subject: scan + multicast ---
    this.clickSubject
      .pipe(scan((acc) => acc + 1, 0))
      .subscribe((count) => {
        this.clickCount.set(count);
        this.clickLog.update((l) => [
          ...l,
          `Click #${count} at ${new Date().toLocaleTimeString()}`,
        ]);
      });
  }

  // ================================================================
  // Counter actions
  // ================================================================
  startCounter(): void {
    this.counterRunning.set(true);
    this.counter.set(0);
    this.counterHistory.set([]);

    interval(500)
      .pipe(
        scan((acc) => acc + 1, 0),
        take(10)
      )
      .subscribe({
        next: (val) => {
          this.counter.set(val);
          this.counterHistory.update((h) => [...h, val]);
        },
        complete: () => this.counterRunning.set(false),
      });
  }

  resetCounter(): void {
    this.counter.set(0);
    this.counterRunning.set(false);
    this.counterHistory.set([]);
  }

  // ================================================================
  // Calculator actions
  // ================================================================
  clearCalcHistory(): void {
    this.calculationHistory.set([]);
  }

  // ================================================================
  // Subject actions
  // ================================================================
  recordClick(): void {
    this.clickSubject.next();
    this.behaviorCount.next(this.behaviorCount.value + 1);
  }

  // ================================================================
  // Operator showcase actions
  // ================================================================
  runOperatorDemo(name: string, result: string): void {
    this.operatorResults.update((r) => [...r, { name, result }]);
  }

  clearOperatorResults(): void {
    this.operatorResults.set([]);
  }

  runBuiltInDemos(): void {
    // map — emits single values
    this.rxjsService.demoMap().subscribe((v) =>
      this.runOperatorDemo('map', `map(x*10) on 1: ${v}`)
    );

    // filter — emits single values
    this.rxjsService.demoFilter().subscribe((v) =>
      this.runOperatorDemo('filter', `filter(even) on 2: ${v}`)
    );

    // scan — emits running totals
    this.rxjsService.demoScan().subscribe((v) =>
      this.runOperatorDemo('scan', `running total: ${v}`)
    );

    // take — emits single values
    this.rxjsService.demoTake().subscribe((v) =>
      this.runOperatorDemo('take(3)', `value: ${v}`)
    );

    // combineLatest
    this.rxjsService.demoCombineLatest().subscribe(([n, s]) =>
      this.runOperatorDemo('combineLatest', `${n} + ${s}`)
    );

    // forkJoin
    this.rxjsService.demoForkJoin().subscribe(([n, s]) =>
      this.runOperatorDemo('forkJoin', `${n}, ${s}`)
    );

    // catchError
    this.rxjsService.demoCatchError().subscribe((v) =>
      this.runOperatorDemo('catchError', v)
    );

    // toArray
    this.rxjsService.demoToArray().subscribe((v) =>
      this.runOperatorDemo('toArray', `collected: ${v.join(', ')}`)
    );

    // every
    this.rxjsService.demoEvery().subscribe((v) =>
      this.runOperatorDemo('every(even)', String(v))
    );
  }
}
