namespace AttendanceRecord.Infrastructure.Services

open System
open System.Text.Json
open System.IO.Pipes
open System.Threading.Tasks
open R3
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Persistence.Dtos
open AttendanceRecord.Persistence.Constants

module NamedPipeImpl =
   let create (pipeName: string) : NamedPipe =
      let receiverObservable =
         Observable.Create<NamedPipeMessage>(fun observer ->
            let cts = new Threading.CancellationTokenSource()

            let rec listenLoop () =
               task {
                  use server =
                     new NamedPipeServerStream(
                        pipeName,
                        PipeDirection.In,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous
                     )

                  do! server.WaitForConnectionAsync cts.Token

                  let! message =
                     JsonSerializer.DeserializeAsync<NamedPipeMessage>(
                        server,
                        InfraJsonContext.Default.NamedPipeMessage,
                        cts.Token
                     )

                  match message with
                  | null -> ()
                  | msg -> observer.OnNext msg

                  if not cts.Token.IsCancellationRequested then
                     do! listenLoop ()
               }

            listenLoop () |> ignore

            Disposable.Create(fun () -> cts.Cancel()))

      let sendMessage (content: string) : Task<Result<unit, string>> =
         task {
            let message =
               NamedPipeMessage(
                  Sender = Environment.ProcessId.ToString(),
                  Content = content,
                  Timestamp = DateTime.UtcNow
               )

            try
               use client =
                  new NamedPipeClientStream(
                     ".",
                     pipeName,
                     PipeDirection.Out,
                     PipeOptions.Asynchronous
                  )

               do! client.ConnectAsync 3000

               do!
                  JsonSerializer.SerializeAsync(
                     client,
                     message,
                     InfraJsonContext.Default.NamedPipeMessage
                  )

               do! client.FlushAsync()

               return Ok()
            with ex ->
               return Error ex.Message
         }

      { Receiver = receiverObservable
        SendMessage = sendMessage }
