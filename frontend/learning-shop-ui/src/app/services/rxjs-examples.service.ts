import { Injectable, inject } from '@angular/core';
import {
  Observable,
  of,
  from,
  interval,
  timer,
  fromEvent,
  BehaviorSubject,
  ReplaySubject,
  Subject,
  combineLatest,
  forkJoin,
  zip,
  merge,
  concat,
  race,
  EMPTY,
  throwError,
  iif,
  defer,
} from 'rxjs';
import {
  map,
  filter,
  tap,
  scan,
  pluck,
  distinctUntilChanged,
  debounceTime,
  switchMap,
  mergeMap,
  concatMap,
  exhaustMap,
  catchError,
  retry,
  take,
  skip,
  first,
  last,
  takeUntil,
  shareReplay,
  finalize,
  delay,
  timeout,
  pairwise,
  buffer,
  window,
  partition,
  groupBy,
  combineAll,
  exhaust,
  switchAll,
  concatAll,
  toArray,
  defaultIfEmpty,
  every,
  find,
  findIndex,
  count,
  max,
  min,
} from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class RxjsExamplesService {
  // ================================================================
  // 1. CREATION OPERATORS
  // ================================================================

  /** `of` - emits provided values synchronously */
  demoOf(): Observable<string> {
    return of('Apple', 'Banana', 'Cherry');
  }

  /** `from` - converts an array, promise, or iterable to an observable */
  demoFrom(): Observable<number> {
    return from([10, 20, 30, 40, 50]);
  }

  /** `interval` - emits sequential numbers every given interval */
  demoInterval(): Observable<number> {
    return interval(1000); // every 1 second
  }

  /** `timer` - emits after a given delay, optionally at intervals */
  demoTimer(): Observable<number> {
    return timer(2000, 1000); // start after 2s, then every 1s
  }

  /** `fromEvent` - creates an observable from DOM events */
  demoFromEvent(target: EventTarget): Observable<Event> {
    return fromEvent(target, 'click');
  }

  // ================================================================
  // 2. SUBJECTS (multicasting)
  // ================================================================

  /** `BehaviorSubject` - requires initial value, emits latest to new subscribers */
  behaviorSubject = new BehaviorSubject<number>(0);

  /** `ReplaySubject` - replays N previous emissions to new subscribers */
  replaySubject = new ReplaySubject<string>(3);

  /** `Subject` - plain multicast, no memory of past values */
  subject = new Subject<void>();

  // ================================================================
  // 3. TRANSFORMATION OPERATORS
  // ================================================================

  /** `map` - projects each value to a new form */
  demoMap(): Observable<number> {
    return of(1, 2, 3).pipe(map((x) => x * 10));
  }

  /** `pluck` - picks a nested property from each emitted object */
  demoPluck(): Observable<string> {
    const users = of(
      { name: 'Alice', address: { city: 'NY' } },
      { name: 'Bob', address: { city: 'LA' } }
    );
    return users.pipe(pluck('address', 'city'));
  }

  /** `scan` - accumulates values over time (like reduce but streaming) */
  demoScan(): Observable<number> {
    return of(1, 2, 3, 4, 5).pipe(scan((acc, curr) => acc + curr, 0));
  }

  // ================================================================
  // 4. FILTERING OPERATORS
  // ================================================================

  /** `filter` - emits only values that pass a predicate */
  demoFilter(): Observable<number> {
    return of(1, 2, 3, 4, 5, 6).pipe(filter((x) => x % 2 === 0));
  }

  /** `distinctUntilChanged` - suppresses consecutive duplicate values */
  demoDistinctUntilChanged(): Observable<string> {
    return of('a', 'a', 'b', 'b', 'a', 'c').pipe(distinctUntilChanged());
  }

  /** `debounceTime` - delays emissions, dropping pending ones if new arrive */
  demoDebounceTime(): Observable<string> {
    return of('hello', 'world').pipe(debounceTime(300));
  }

  /** `take` - emits first N values then completes */
  demoTake(): Observable<number> {
    return of(1, 2, 3, 4, 5).pipe(take(3));
  }

  /** `skip` - ignores first N values */
  demoSkip(): Observable<number> {
    return of(1, 2, 3, 4, 5).pipe(skip(2));
  }

  /** `first` - emits only the first value */
  demoFirst(): Observable<number> {
    return of(10, 20, 30).pipe(first());
  }

  /** `last` - emits only the last value */
  demoLast(): Observable<number> {
    return of(10, 20, 30).pipe(last());
  }

  /** `takeUntil` - emits until another observable emits */
  demoTakeUntil(stop$: Observable<void>): Observable<number> {
    return interval(1000).pipe(takeUntil(stop$));
  }

  // ================================================================
  // 5. COMBINATION OPERATORS
  // ================================================================

  /** `combineLatest` - emits latest from each source whenever any emits */
  demoCombineLatest(): Observable<[number, string]> {
    return combineLatest([of(1, 2, 3), of('x', 'y')]);
  }

  /** `forkJoin` - waits for all sources to complete, emits last value of each */
  demoForkJoin(): Observable<[number, string]> {
    return forkJoin([of(100), of('done')]);
  }

  /** `zip` - pairs values by index from each source */
  demoZip(): Observable<[number, string]> {
    return zip(of(1, 2), of('a', 'b'));
  }

  /** `merge` - interleaves emissions from multiple sources */
  demoMerge(): Observable<number> {
    return merge(of(1, 2), of(3, 4));
  }

  /** `concat` - subscribes to sources one after another */
  demoConcat(): Observable<number> {
    return concat(of(1, 2), of(3, 4));
  }

  /** `race` - emits from the first source to emit, others are unsubscribed */
  demoRace(): Observable<string> {
    return race(of('slow').pipe(delay(100)), of('fast'));
  }

  // ================================================================
  // 6. HIGHER-ORDER (FLATTENING) OPERATORS
  // ================================================================

  /** `switchMap` - cancels previous inner subscription on new outer emission */
  demoSwitchMap(): Observable<number> {
    return of(1, 2, 3).pipe(switchMap((x) => of(x * 10).pipe(delay(100))));
  }

  /** `mergeMap` (flatMap) - subscribes to all inner observables concurrently */
  demoMergeMap(): Observable<number> {
    return of(1, 2, 3).pipe(mergeMap((x) => of(x * 10).pipe(delay(100))));
  }

  /** `concatMap` - subscribes to inner observables sequentially */
  demoConcatMap(): Observable<number> {
    return of(1, 2, 3).pipe(concatMap((x) => of(x * 10).pipe(delay(100))));
  }

  /** `exhaustMap` - ignores new outer emissions while inner is active */
  demoExhaustMap(): Observable<number> {
    return interval(500).pipe(exhaustMap(() => of(99).pipe(delay(1000))));
  }

  // ================================================================
  // 7. ERROR HANDLING
  // ================================================================

  /** `catchError` - catches errors and returns a new observable */
  demoCatchError(): Observable<string> {
    return throwError(() => new Error('boom')).pipe(
      catchError((err) => of('Recovered from: ' + err.message))
    );
  }

  /** `retry` - retries the source observable N times on error */
  demoRetry(): Observable<string> {
    return throwError(() => new Error('fail')).pipe(retry(3));
  }

  // ================================================================
  // 8. UTILITY OPERATORS
  // ================================================================

  /** `tap` - performs side effects without changing the stream */
  demoTap(): Observable<number> {
    return of(1, 2, 3).pipe(tap((x) => console.log('tap side effect:', x)));
  }

  /** `shareReplay` - shares the source and replays to late subscribers */
  demoShareReplay(): Observable<number> {
    return of(42).pipe(shareReplay(1));
  }

  /** `finalize` - runs when observable completes or errors */
  demoFinalize(): Observable<number> {
    return of(1, 2, 3).pipe(finalize(() => console.log('finalized')));
  }

  /** `delay` - shifts emissions forward in time */
  demoDelay(): Observable<number> {
    return of(1, 2, 3).pipe(delay(500));
  }

  /** `timeout` - errors if source takes too long to emit */
  demoTimeout(): Observable<number> {
    return interval(2000).pipe(timeout(1000));
  }

  /** `pairwise` - emits pairs of consecutive values */
  demoPairwise(): Observable<[number, number]> {
    return of(1, 2, 3, 4).pipe(pairwise());
  }

  /** `buffer` - collects values into arrays when boundary observable emits */
  demoBuffer(boundary$: Observable<void>): Observable<number[]> {
    return interval(100).pipe(buffer(boundary$));
  }

  /** `window` - emits observables that window values over time */
  demoWindow(windowBoundaries: Observable<void>): Observable<Observable<number>> {
    return interval(100).pipe(window(windowBoundaries)) as Observable<Observable<number>>;
  }

  /** `partition` - splits source into two observables based on predicate */
  demoPartition(): [Observable<number>, Observable<number>] {
    const result = (partition as any)(of(1, 2, 3, 4, 5, 6), (x: number) => x % 2 === 0);
    return result as [Observable<number>, Observable<number>];
  }

  /** `groupBy` - groups values by key into grouped observables */
  demoGroupBy(): Observable<Observable<{ key: string; value: number }>> {
    return of(
      { key: 'A', value: 1 },
      { key: 'B', value: 2 },
      { key: 'A', value: 3 }
    ).pipe(groupBy((x: { key: string; value: number }) => x.key));
  }

  // ================================================================
  // 9. AGGREGATION / MATHEMATICAL
  // ================================================================

  /** `count` - counts emitted values */
  demoCount(): Observable<number> {
    return of(1, 2, 3, 4).pipe(count());
  }

  /** `max` - emits the maximum value */
  demoMax(): Observable<number | undefined> {
    return of(3, 1, 4, 1, 5).pipe(max());
  }

  /** `min` - emits the minimum value */
  demoMin(): Observable<number | undefined> {
    return of(3, 1, 4, 1, 5).pipe(min());
  }

  /** `sum` via scan + last */
  demoSum(): Observable<number | undefined> {
    return of(1, 2, 3, 4, 5).pipe(scan((a, b) => a + b, 0), last());
  }

  /** `every` - true if all values satisfy predicate */
  demoEvery(): Observable<boolean> {
    return of(2, 4, 6, 8).pipe(every((x) => x % 2 === 0));
  }

  /** `find` - emits first value matching predicate */
  demoFind(): Observable<number | undefined> {
    return of(10, 20, 30, 40).pipe(find((x: number) => x > 25));
  }

  /** `findIndex` - emits index of first matching value */
  demoFindIndex(): Observable<number> {
    return of(10, 20, 30, 40).pipe(findIndex((x: number) => x > 25));
  }

  /** `defaultIfEmpty` - emits default value if source is empty */
  demoDefaultIfEmpty(): Observable<string> {
    return of().pipe(defaultIfEmpty('fallback'));
  }

  /** `toArray` - collects all emissions into an array on completion */
  demoToArray(): Observable<number[]> {
    return of(1, 2, 3, 4).pipe(toArray());
  }

  // ================================================================
  // 10. CONDITIONAL / DEFERRED
  // ================================================================

  /** `iif` - chooses observable dynamically based on condition */
  demoIif(condition: boolean): Observable<string> {
    return iif(
      () => condition,
      of('yes branch'),
      of('no branch')
    );
  }

  /** `defer` - creates observable lazily on subscription */
  demoDefer(): Observable<Date> {
    return defer(() => of(new Date()));
  }

  // ================================================================
  // 11. FLATTENING (ALL)
  // ================================================================

  /** `combineAll` - combines inner observables like combineLatest */
  demoCombineAll(): Observable<number[]> {
    return of(of(1), of(2)).pipe(combineAll()) as Observable<number[]>;
  }

  /** `exhaust` - exhausts inner observables */
  demoExhaust(): Observable<number> {
    return of(of(1), of(2)).pipe(exhaust()) as Observable<number>;
  }

  /** `switchAll` - switches to latest inner observable */
  demoSwitchAll(): Observable<number> {
    return of(of(1), of(2)).pipe(switchAll()) as Observable<number>;
  }

  /** `concatAll` - concatenates inner observables */
  demoConcatAll(): Observable<number> {
    return of(of(1), of(2)).pipe(concatAll()) as Observable<number>;
  }
}
