namespace Avalonia.FuncUI

open System.Runtime.CompilerServices

[<Extension>]
type __UseDerivedExtensions =
    [<Extension>]
    static member useDerived1(ctx: IComponentContext, source: IReadable<'A>, mapFunc: 'A -> 'B) : IReadable<'B> =
        ctx.usePassedRead (source.Map mapFunc)

    [<Extension>]
    static member useDerived2
        (
            ctx: IComponentContext,
            sources: (IReadable<'A> * IReadable<'B>),
            mapFunc: 'A * 'B -> 'C
        ) : IReadable<'C> =
        let aRead = ctx.usePassedRead (fst sources)
        let bRead = ctx.usePassedRead (snd sources)
        let mapped = ctx.useState (mapFunc (aRead.Current, bRead.Current))

        ctx.useEffect (
            fun () ->
                let next = mapFunc (aRead.Current, bRead.Current)

                if next <> mapped.Current then
                    mapped.Set next
            , [ EffectTrigger.AfterChange aRead; EffectTrigger.AfterChange bRead ]
        )

        mapped

    [<Extension>]
    static member useDerived3
        (
            ctx: IComponentContext,
            sources: (IReadable<'A> * IReadable<'B> * IReadable<'C>),
            mapFunc: 'A * 'B * 'C -> 'D
        ) : IReadable<'D> =
        let a, b, c = sources
        let abRead = ctx.useDerived2 ((a, b), (fun (a, b) -> a, b))
        let abcRead = ctx.useDerived2 ((abRead, c), (fun (ab, c) -> ab, c))

        abcRead.Map(fun (ab, c) -> mapFunc (fst ab, snd ab, c)) |> ctx.usePassedRead

    [<Extension>]
    static member useDerived4
        (
            ctx: IComponentContext,
            sources: (IReadable<'A> * IReadable<'B> * IReadable<'C> * IReadable<'D>),
            mapFunc: 'A * 'B * 'C * 'D -> 'E
        ) : IReadable<'E> =
        let a, b, c, d = sources
        let abcRead = ctx.useDerived3 ((a, b, c), (fun (a, b, c) -> a, b, c))
        let abcdRead = ctx.useDerived2 ((abcRead, d), (fun (abc, d) -> abc, d))

        abcdRead.Map(fun (abc, d) ->
            let a, b, c = abc
            mapFunc (a, b, c, d))
        |> ctx.usePassedRead
