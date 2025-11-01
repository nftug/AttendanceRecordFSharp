namespace AttendanceRecord.Presentation.Features.HomePage.Hooks

open R3
open Avalonia.FuncUI
open AttendanceRecord.Application.Dtos.Responses

type HomePageHooks =
    { Status: IReadable<CurrentStatusDto option> }

[<AutoOpen>]
module HomePageHooks =
    let useHomePageHooks (statusObservable: Observable<CurrentStatusDto>) (ctx: IComponentContext) : HomePageHooks =
        let state = ctx.useState<CurrentStatusDto option> None

        ctx.useEffect (
            (fun () -> statusObservable.Subscribe(fun s -> Some s |> state.Set)),
            [ EffectTrigger.AfterInit ]
        )

        { Status = state |> ctx.usePassedRead }
