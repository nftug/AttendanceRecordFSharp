namespace AttendanceRecord.Presentation.Compositions

open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Services
open AttendanceRecord.Application.UseCases.WorkRecords
open AttendanceRecord.Infrastructure.Repositories
open AttendanceRecord.Infrastructure.Services

type ServiceContainer =
    { WorkRecordRepository: WorkRecordRepository
      TimerProvider: TimerProvider
      CurrentStatusStore: CurrentStatusStore
      ToggleWorkUseCase: ToggleWork
      ToggleRestUseCase: ToggleRest }

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

        { WorkRecordRepository = workRecordRepository
          TimerProvider = timerProvider
          CurrentStatusStore = currentStatusStore
          ToggleWorkUseCase = toggleWorkUseCase
          ToggleRestUseCase = toggleRestUseCase }
