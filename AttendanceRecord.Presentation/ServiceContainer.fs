namespace AttendanceRecord.Presentation

open R3
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Services
open AttendanceRecord.Application.UseCases.WorkRecords
open AttendanceRecord.Application.UseCases.AppConfig
open AttendanceRecord.Infrastructure.Repositories
open AttendanceRecord.Infrastructure.Services
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Shared

type ServiceContainer =
    { SingleInstanceGuard: SingleInstanceGuard
      NamedPipe: NamedPipe
      WorkStatusStore: WorkStatusStore
      AlarmService: AlarmService
      AppConfig: R3.Observable<AppConfigDto>
      ToggleWorkUseCase: ToggleWork
      ToggleRestUseCase: ToggleRest
      SaveWorkRecordUseCase: SaveWorkRecord
      DeleteWorkRecordUseCase: DeleteWorkRecord
      GetMonthlyWorkRecordsUseCase: GetMonthlyWorkRecords
      GetWorkRecordDetailsUseCase: GetWorkRecordDetails
      SaveAppConfigUseCase: SaveAppConfig }

module ServiceContainer =
    let create () : ServiceContainer =
        // Singleton composite disposable for application lifetime
        let disposables = new CompositeDisposable()

        // Infrastructure Services
        let appDirService = AppDirectoryService.create ()
        let singleInstanceGuard = SingleInstanceGuardImpl.create appDirService
        let workRecordRepository = WorkRecordRepositoryImpl.create appDirService
        let appConfigRepository = AppConfigRepositoryImpl.create appDirService
        let namedPipe = NamedPipeImpl.create NamedPipe.pipeName

        // Application Services and Use Cases
        let timerProvider = TimerProvider.create ()
        let appConfigStore = AppConfigStore.create appConfigRepository disposables
        let getAppConfig () = appConfigStore.Current.CurrentValue

        let workStatusStore =
            WorkStatusStore.create
                { TimerProvider = timerProvider
                  WorkRecordRepository = workRecordRepository
                  AppConfig = appConfigStore.Current
                  Disposables = disposables }

        let alarmService =
            AlarmService.create
                { StatusStore = workStatusStore
                  AppConfig = appConfigStore.Current
                  Disposables = disposables }

        let toggleWorkUseCase = ToggleWork.create workRecordRepository workStatusStore
        let toggleRestUseCase = ToggleRest.create workRecordRepository workStatusStore

        let saveWorkRecordUseCase =
            SaveWorkRecord.create workRecordRepository workStatusStore

        let deleteWorkRecordUseCase =
            DeleteWorkRecord.create workRecordRepository workStatusStore

        let getMonthlyWorkRecordsUseCase =
            GetMonthlyWorkRecords.create workRecordRepository getAppConfig

        let getWorkRecordDetailsUseCase =
            GetWorkRecordDetails.create workRecordRepository getAppConfig

        let appConfig = appConfigStore.Current |> R3.map AppConfigDto.fromDomain
        let saveAppConfigUseCase = SaveAppConfig.create appConfigRepository appConfigStore

        { SingleInstanceGuard = singleInstanceGuard
          NamedPipe = namedPipe
          WorkStatusStore = workStatusStore
          AlarmService = alarmService
          AppConfig = appConfig
          ToggleWorkUseCase = toggleWorkUseCase
          ToggleRestUseCase = toggleRestUseCase
          SaveWorkRecordUseCase = saveWorkRecordUseCase
          DeleteWorkRecordUseCase = deleteWorkRecordUseCase
          GetMonthlyWorkRecordsUseCase = getMonthlyWorkRecordsUseCase
          GetWorkRecordDetailsUseCase = getWorkRecordDetailsUseCase
          SaveAppConfigUseCase = saveAppConfigUseCase }
