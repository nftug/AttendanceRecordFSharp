namespace AttendanceRecord.Presentation.Utils

open R3
open AttendanceRecord.Shared

type FormContext<'TDto> =
    { Form: ReactiveProperty<'TDto>
      IsFormDirty: ReadOnlyReactiveProperty<bool>
      Error: ReactiveProperty<string option>
      ResetForm: 'TDto option -> unit
      OnReset: Observable<'TDto> }

module FormContext =
    let create<'TDto when 'TDto: equality>
        (initialValue: 'TDto)
        (disposables: CompositeDisposable)
        : FormContext<'TDto> =
        let form = R3.property initialValue |> R3.disposeWith disposables

        let defaultForm = R3.property form.CurrentValue |> R3.disposeWith disposables

        let error = R3.property (None: string option) |> R3.disposeWith disposables

        form
        |> R3.distinctUntilChanged
        |> R3.subscribe (fun _ -> error.Value <- None)
        |> disposables.Add

        let isFormDirty =
            R3.combineLatest2 form defaultForm
            |> R3.map (fun (f, df) -> f <> df)
            |> R3.readonly None
            |> R3.disposeWith disposables

        let resetCommand =
            isFormDirty
            |> R3.toCommand<'TDto>
            |> R3.withSubscribe disposables (fun next ->
                form.Value <- next

                if next <> defaultForm.CurrentValue then
                    defaultForm.Value <- next)

        let resetForm (value: 'TDto option) =
            match value with
            | Some v -> resetCommand.Execute v
            | None -> resetCommand.Execute defaultForm.CurrentValue

        let onReset = R3.merge [ resetCommand; defaultForm |> R3.distinctUntilChanged ]

        { Form = form
          IsFormDirty = isFormDirty
          Error = error
          ResetForm = resetForm
          OnReset = onReset }
