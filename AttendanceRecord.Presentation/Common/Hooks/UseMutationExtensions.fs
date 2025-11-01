namespace Avalonia.FuncUI

open System.Runtime.CompilerServices
open FsToolkit.ErrorHandling

type UseMutationResult<'arg, 'ret> =
    { MutateTask: 'arg -> TaskResult<'ret, string>
      Mutate: 'arg -> unit
      IsPending: IReadable<bool>
      Error: IReadable<string option> }

[<Extension>]
type __UseMutationExtensions =
    [<Extension>]
    static member useMutation(ctx: IComponentContext, mutateFunc: ('arg -> TaskResult<'ret, string>)) =
        let isPending = ctx.useState false
        let error = ctx.useState<string option> None

        let mutateTask =
            fun (param: 'arg) ->
                task {
                    isPending.Set true
                    error.Set None

                    let! result = mutateFunc param

                    isPending.Set false

                    match result with
                    | Ok v -> return Ok v
                    | Error e ->
                        printfn $"Mutation error: {e}"
                        error.Set(Some e)
                        return Error e
                }

        { MutateTask = mutateTask
          Mutate = mutateTask >> ignore
          IsPending = isPending |> ctx.usePassedRead
          Error = error |> ctx.usePassedRead }
