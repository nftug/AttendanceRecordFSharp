namespace AttendanceRecord.Shared

open R3
open System

module R3 =
    let property<'T> (initialValue: 'T) : ReactiveProperty<'T> = new ReactiveProperty<'T>(initialValue)

    let readonly (initialValue: 'T option) (source: Observable<'T>) : ReadOnlyReactiveProperty<'T> =
        source
            .DistinctUntilChanged()
            .ToReadOnlyReactiveProperty(
                match initialValue with
                | Some v -> v
                | None -> Unchecked.defaultof<'T>
            )

    let command<'T> () : ReactiveCommand<'T> = new ReactiveCommand<'T>()

    let disposeWith<'T when 'T :> IDisposable> (disposables: CompositeDisposable) (disposable: 'T) : 'T =
        disposables.Add(R3.Disposable.Create(fun () -> disposable.Dispose()))
        disposable

    let filter<'T> (predicate: 'T -> bool) (source: Observable<'T>) : Observable<'T> =
        source.Where(fun v -> predicate v)

    let map<'TIn, 'TOut> (mapper: 'TIn -> 'TOut) (source: Observable<'TIn>) : Observable<'TOut> =
        source.Select(fun v -> mapper v)

    let subscribe<'T> (onNext: 'T -> unit) (source: Observable<'T>) : IDisposable = source.Subscribe(fun v -> onNext v)

    let combineLatest2<'T1, 'T2, 'TOut>
        (source2: Observable<'T2>)
        (combiner: 'T1 -> 'T2 -> 'TOut)
        (source1: Observable<'T1>)
        : Observable<'TOut> =
        source1.CombineLatest(source2, combiner)

    let combineLatest3<'T1, 'T2, 'T3, 'TOut>
        (source2: Observable<'T2>)
        (source3: Observable<'T3>)
        (combiner: 'T1 -> 'T2 -> 'T3 -> 'TOut)
        (source1: Observable<'T1>)
        : Observable<'TOut> =
        source1.CombineLatest(source2, source3, combiner)

    let combineLatest4<'T1, 'T2, 'T3, 'T4, 'TOut>
        (source2: Observable<'T2>)
        (source3: Observable<'T3>)
        (source4: Observable<'T4>)
        (combiner: 'T1 -> 'T2 -> 'T3 -> 'T4 -> 'TOut)
        (source1: Observable<'T1>)
        : Observable<'TOut> =
        source1.CombineLatest(source2, source3, source4, combiner)

    let merge<'T> (sources: seq<Observable<'T>>) : Observable<'T> = Observable.Merge sources

    let delay (dueTime: TimeSpan) (source: Observable<'T>) : Observable<'T> = source.Delay dueTime

    let prepend<'T> (value: 'T) (source: Observable<'T>) : Observable<'T> = source.Prepend value
