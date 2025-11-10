namespace AttendanceRecord.Presentation.Utils

open System.Threading.Tasks
open System.Threading
open R3
open AttendanceRecord.Shared

type UseMutationResult<'arg, 'ret> =
    { MutateTask: 'arg -> CancellationToken -> Task<Result<'ret, string>>
      Mutate: 'arg -> unit
      IsPending: ReadOnlyReactiveProperty<bool>
      Error: ReadOnlyReactiveProperty<string option> }

[<AutoOpen>]
module UseMutation =
    let useMutation
        (disposables: CompositeDisposable)
        (mutateFunc: 'arg -> CancellationToken -> Task<Result<'ret, string>>)
        =
        let isPending = R3.property false |> R3.disposeWith disposables
        let error = R3.property None |> R3.disposeWith disposables

        let mutate (arg: 'arg) (ct: CancellationToken) : Task<Result<'ret, string>> =
            task {
                isPending.Value <- true
                error.Value <- None

                let! result = mutateFunc arg ct

                isPending.Value <- false

                match result with
                | Ok v -> return Ok v
                | Error e ->
                    printfn $"Mutation error: {e}"
                    error.Value <- Some e
                    return Error e
            }

        { MutateTask = mutate
          Mutate = mutate >> ignore
          IsPending = isPending
          Error = error }
