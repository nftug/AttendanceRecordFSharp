namespace Avalonia.FuncUI

open System.Runtime.CompilerServices
open R3

[<Extension>]
type __UseObservableExtensions =
    [<Extension>]
    static member useObservableState(ctx: IComponentContext, observable: Observable<'t>, ?initial: 't) =
        let state = ctx.useState initial

        ctx.useEffect ((fun () -> observable.Subscribe(fun v -> state.Set(Some v))), [ EffectTrigger.AfterInit ])

        state |> ctx.usePassedRead

    [<Extension>]
    static member useObservableEffect(ctx: IComponentContext, observable: Observable<'t>, effect: 't -> unit) =
        ctx.useEffect ((fun () -> observable.Subscribe effect), [ EffectTrigger.AfterInit ])
