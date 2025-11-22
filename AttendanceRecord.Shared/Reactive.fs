namespace AttendanceRecord.Shared

open R3
open System
open ObservableCollections

module R3 =
    let property<'T> (initialValue: 'T) : ReactiveProperty<'T> =
        new ReactiveProperty<'T>(initialValue)

    let readonly (initialValue: 'T option) (source: Observable<'T>) : ReadOnlyReactiveProperty<'T> =
        source
            .DistinctUntilChanged()
            .ToReadOnlyReactiveProperty(
                match initialValue with
                | Some v -> v
                | None -> Unchecked.defaultof<'T>
            )

    let command<'T> () : ReactiveCommand<'T> = new ReactiveCommand<'T>()

    let disposeWith<'T when 'T :> IDisposable>
        (disposables: CompositeDisposable)
        (disposable: 'T)
        : 'T =
        disposables.Add(R3.Disposable.Create(fun () -> disposable.Dispose()))
        disposable

    let filter<'T> (predicate: 'T -> bool) (source: Observable<'T>) : Observable<'T> =
        source.Where predicate

    let map<'TIn, 'TOut> (mapper: 'TIn -> 'TOut) (source: Observable<'TIn>) : Observable<'TOut> =
        source.Select mapper

    let subscribe<'T> (onNext: 'T -> unit) (source: Observable<'T>) : IDisposable =
        source.Subscribe onNext

    let combineLatest2<'T1, 'T2>
        (source1: Observable<'T1>)
        (source2: Observable<'T2>)
        : Observable<'T1 * 'T2> =
        source1.CombineLatest(source2, fun v1 v2 -> v1, v2)

    let combineLatest3<'T1, 'T2, 'T3>
        (source1: Observable<'T1>)
        (source2: Observable<'T2>)
        (source3: Observable<'T3>)
        : Observable<'T1 * 'T2 * 'T3> =
        source1.CombineLatest(source2, source3, fun v1 v2 v3 -> v1, v2, v3)

    let combineLatest4<'T1, 'T2, 'T3, 'T4>
        (source1: Observable<'T1>)
        (source2: Observable<'T2>)
        (source3: Observable<'T3>)
        (source4: Observable<'T4>)
        : Observable<'T1 * 'T2 * 'T3 * 'T4> =
        source1.CombineLatest(source2, source3, source4, fun v1 v2 v3 v4 -> v1, v2, v3, v4)

    let list<'obs when 'obs :> IDisposable>
        (initialItems: seq<'obs>)
        (disposables: CompositeDisposable)
        : ObservableList<'obs> =
        let collection = ObservableList<'obs> initialItems

        collection.ObserveAdd()
        |> subscribe (fun e -> e.Value |> disposables.Add)
        |> disposables.Add

        collection.ObserveRemove() |> subscribe _.Value.Dispose() |> disposables.Add

        collection.ObserveClear()
        |> subscribe (fun _ -> collection |> Seq.iter _.Dispose())
        |> disposables.Add

        collection

    let everyValueChanged (selector: 'T -> 'U) (source: 'T) : Observable<'U> =
        Observable.EveryValueChanged(source, selector)

    let selectMany (selector: 'T -> Observable<'U>) (source: Observable<'T>) : Observable<'U> =
        source.SelectMany selector

    let mapFromListChanged
        (selector: 'T[] -> 'U)
        (defaultValue: 'U)
        (collection: ObservableList<'obs :> Observable<'T>>)
        : Observable<'U> =
        collection
        |> everyValueChanged _.Count
        |> selectMany (fun _ ->
            if collection.Count = 0 then
                Observable.Return defaultValue
            else
                collection
                |> Seq.cast<Observable<'T>>
                |> Observable.CombineLatest
                |> map selector)

    let merge<'T> (sources: seq<Observable<'T>>) : Observable<'T> = Observable.Merge sources

    let delay (dueTime: TimeSpan) (source: Observable<'T>) : Observable<'T> = source.Delay dueTime

    let prepend<'T> (value: 'T) (source: Observable<'T>) : Observable<'T> = source.Prepend value

    let skip<'T> (count: int) (source: Observable<'T>) : Observable<'T> = source.Skip count

    let distinctUntilChanged<'T> (source: Observable<'T>) : Observable<'T> =
        source.DistinctUntilChanged()

    let distinctUntilChangedBy<'T, 'K>
        (selector: 'T -> 'K)
        (source: Observable<'T>)
        : Observable<'T> =
        source.DistinctUntilChangedBy selector
