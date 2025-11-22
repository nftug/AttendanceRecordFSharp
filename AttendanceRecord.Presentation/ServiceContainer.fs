namespace AttendanceRecord.Presentation

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
      CurrentStatusStore: CurrentStatusStore
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
        // Infrastructure Services
        let appDirService = AppDirectoryService.create ()
        let singleInstanceGuard = SingleInstanceGuardImpl.create appDirService
        let workRecordRepository = WorkRecordRepositoryImpl.create appDirService
        let appConfigRepository = AppConfigRepositoryImpl.create appDirService
        let namedPipe = NamedPipeImpl.create NamedPipe.pipeName

        // Application Services and Use Cases
        let timerProvider = TimerProvider.create ()
        let appConfigStore = new AppConfigStore(appConfigRepository)
        let getAppConfig () = appConfigStore.Current.CurrentValue

        let currentStatusStore =
            new CurrentStatusStore(timerProvider, workRecordRepository, appConfigStore.Current)

        let alarmService = new AlarmService(currentStatusStore, appConfigStore.Current)

        let toggleWorkUseCase = ToggleWork.create workRecordRepository currentStatusStore
        let toggleRestUseCase = ToggleRest.create workRecordRepository currentStatusStore

        let saveWorkRecordUseCase =
            SaveWorkRecord.create workRecordRepository currentStatusStore

        let deleteWorkRecordUseCase =
            DeleteWorkRecord.create workRecordRepository currentStatusStore

        let getMonthlyWorkRecordsUseCase =
            GetMonthlyWorkRecords.create workRecordRepository getAppConfig

        let getWorkRecordDetailsUseCase =
            GetWorkRecordDetails.create workRecordRepository getAppConfig

        let appConfig = appConfigStore.Current |> R3.map AppConfigDto.fromDomain
        let saveAppConfigUseCase = SaveAppConfig.create appConfigRepository appConfigStore

        { SingleInstanceGuard = singleInstanceGuard
          NamedPipe = namedPipe
          CurrentStatusStore = currentStatusStore
          AlarmService = alarmService
          AppConfig = appConfig
          ToggleWorkUseCase = toggleWorkUseCase
          ToggleRestUseCase = toggleRestUseCase
          SaveWorkRecordUseCase = saveWorkRecordUseCase
          DeleteWorkRecordUseCase = deleteWorkRecordUseCase
          GetMonthlyWorkRecordsUseCase = getMonthlyWorkRecordsUseCase
          GetWorkRecordDetailsUseCase = getWorkRecordDetailsUseCase
          SaveAppConfigUseCase = saveAppConfigUseCase }
