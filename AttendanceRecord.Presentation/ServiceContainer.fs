namespace AttendanceRecord.Presentation

open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Services
open AttendanceRecord.Application.UseCases.WorkRecords
open AttendanceRecord.Infrastructure.Repositories
open AttendanceRecord.Infrastructure.Services

type ServiceContainer =
    { SingleInstanceGuard: SingleInstanceGuard
      NamedPipe: NamedPipe
      CurrentStatusStore: CurrentStatusStore
      AlarmService: AlarmService
      ToggleWorkUseCase: ToggleWork
      ToggleRestUseCase: ToggleRest
      SaveWorkRecordUseCase: SaveWorkRecord
      DeleteWorkRecordUseCase: DeleteWorkRecord
      GetMonthlyWorkRecordsUseCase: GetMonthlyWorkRecords
      GetWorkRecordDetailsUseCase: GetWorkRecordDetails }

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
        let appConfigStore = AppConfigStore appConfigRepository
        let getAppConfig () = appConfigStore.Current

        let currentStatusStore =
            new CurrentStatusStore(timerProvider, workRecordRepository, getAppConfig)

        let alarmService = new AlarmService(currentStatusStore, getAppConfig)

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

        { SingleInstanceGuard = singleInstanceGuard
          NamedPipe = namedPipe
          CurrentStatusStore = currentStatusStore
          AlarmService = alarmService
          ToggleWorkUseCase = toggleWorkUseCase
          ToggleRestUseCase = toggleRestUseCase
          SaveWorkRecordUseCase = saveWorkRecordUseCase
          DeleteWorkRecordUseCase = deleteWorkRecordUseCase
          GetMonthlyWorkRecordsUseCase = getMonthlyWorkRecordsUseCase
          GetWorkRecordDetailsUseCase = getWorkRecordDetailsUseCase }
