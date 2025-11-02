namespace AttendanceRecord.Presentation

open AttendanceRecord.Application.Services
open AttendanceRecord.Application.UseCases.WorkRecords
open AttendanceRecord.Infrastructure.Repositories
open AttendanceRecord.Infrastructure.Services

type ServiceContainer =
    { CurrentStatusStore: CurrentStatusStore
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
        let workRecordRepository = WorkRecordRepositoryImpl.create appDirService

        // Application Services and Use Cases
        let timerProvider = TimerProvider.create ()
        let standardWorkTime = System.TimeSpan.FromHours 8.0

        let currentStatusStore =
            new CurrentStatusStore(timerProvider, workRecordRepository, standardWorkTime)

        let toggleWorkUseCase = ToggleWork.create workRecordRepository currentStatusStore
        let toggleRestUseCase = ToggleRest.create workRecordRepository currentStatusStore

        let saveWorkRecordUseCase =
            SaveWorkRecord.create workRecordRepository currentStatusStore

        let deleteWorkRecordUseCase =
            DeleteWorkRecord.create workRecordRepository currentStatusStore

        let getMonthlyWorkRecordsUseCase =
            GetMonthlyWorkRecords.create workRecordRepository standardWorkTime

        let getWorkRecordDetailsUseCase =
            GetWorkRecordDetails.create workRecordRepository standardWorkTime

        { CurrentStatusStore = currentStatusStore
          ToggleWorkUseCase = toggleWorkUseCase
          ToggleRestUseCase = toggleRestUseCase
          SaveWorkRecordUseCase = saveWorkRecordUseCase
          DeleteWorkRecordUseCase = deleteWorkRecordUseCase
          GetMonthlyWorkRecordsUseCase = getMonthlyWorkRecordsUseCase
          GetWorkRecordDetailsUseCase = getWorkRecordDetailsUseCase }
