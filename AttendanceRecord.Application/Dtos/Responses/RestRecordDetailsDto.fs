namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Dtos.Enums

type RestRecordDetailsDto =
   { Id: Guid
     Date: DateTime
     Duration: TimeDurationDto
     Variant: RestVariantEnum }

module RestRecordDetailsDto =
   let empty: RestRecordDetailsDto =
      { Id = Guid.Empty
        Date = DateTime.MinValue
        Duration = TimeDurationDto.empty
        Variant = RestVariantEnum.RegularRest }

   let fromDomain (now: DateTime) (restRecord: RestRecord) : RestRecordDetailsDto =
      { Id = restRecord.Id
        Date = restRecord |> RestRecord.getDate
        Duration = restRecord.Duration |> TimeDurationDto.fromDomain now
        Variant = restRecord.Variant |> RestVariantEnum.fromDomain }
