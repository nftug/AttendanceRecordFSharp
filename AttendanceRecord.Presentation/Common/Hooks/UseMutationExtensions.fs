namespace Avalonia.FuncUI

open System.Threading.Tasks
open System.Runtime.CompilerServices

type UseMutationResult<'arg, 'ret> =
    { MutateTask: 'arg -> Task<'ret>
      Mutate: 'arg -> unit
      IsPending: IReadable<bool> }

[<Extension>]
type __UseMutationExtensions =
    [<Extension>]
    static member useMutation(ctx: IComponentContext, mutateFunc: ('arg -> Task<'ret>)) =
        let isPending = ctx.useState false

        let mutateTask =
            fun (param: 'arg) ->
                task {
                    isPending.Set true
                    let! result = mutateFunc param
                    isPending.Set false
                    return result
                }

        { MutateTask = mutateTask
          Mutate = mutateTask >> ignore
          IsPending = isPending |> ctx.usePassedRead }
