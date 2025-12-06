namespace AttendanceRecord.Application.Dtos.Enums

open AttendanceRecord.Domain.Entities

[<RequireQualifiedAccess>]
type RestVariantEnum =
   | RegularRest = 0
   | PaidRest = 1

module RestVariantEnum =
   let all = [ RestVariantEnum.RegularRest; RestVariantEnum.PaidRest ]

   let fromDomain (variant: RestVariant) : RestVariantEnum =
      match variant with
      | RegularRest -> RestVariantEnum.RegularRest
      | PaidRest -> RestVariantEnum.PaidRest

   let toDomain (variant: RestVariantEnum) : RestVariant =
      match variant with
      | RestVariantEnum.RegularRest -> RegularRest
      | RestVariantEnum.PaidRest -> PaidRest
      | _ -> RegularRest

   let toDisplayString (variant: RestVariantEnum) : string =
      match variant with
      | RestVariantEnum.RegularRest -> "休憩"
      | RestVariantEnum.PaidRest -> "有給休暇"
      | _ -> "不明"
