namespace AttendanceRecord.Presentation.Utils

open R3
open AttendanceRecord.Shared

type FormContext<'TDto, 'TError> =
    { Form: ReactiveProperty<'TDto>
      IsFormDirty: ReadOnlyReactiveProperty<bool>
      Errors: ReactiveProperty<'TError list>
      ResetForm: 'TDto option -> unit
      OnReset: Observable<'TDto> }

module FormContext =
    let create<'TDto, 'TError when 'TDto: equality>
        (initialValue: 'TDto)
        (disposables: CompositeDisposable)
        : FormContext<'TDto, 'TError> =
        let form = R3.property initialValue |> R3.disposeWith disposables

        let defaultForm = R3.property form.CurrentValue |> R3.disposeWith disposables

        let errors = R3.property List.empty<'TError> |> R3.disposeWith disposables

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

        onReset |> R3.subscribe (fun v -> errors.Value <- List.empty) |> disposables.Add

        { Form = form
          IsFormDirty = isFormDirty
          Errors = errors
          ResetForm = resetForm
          OnReset = onReset }
