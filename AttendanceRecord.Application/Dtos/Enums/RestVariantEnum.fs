namespace AttendanceRecord.Application.Dtos.Enums

open AttendanceRecord.Domain.Entities

[<RequireQualifiedAccess>]
type RestVariantEnum =
    | RegularRest = 0
    | PaidRest = 1

module RestVariantEnum =
    let fromDomain (variant: RestVariant) : RestVariantEnum =
        match variant with
        | RegularRest -> RestVariantEnum.RegularRest
        | PaidRest -> RestVariantEnum.PaidRest

    let toDomain (variant: RestVariantEnum) : RestVariant =
        match variant with
        | RestVariantEnum.RegularRest -> RegularRest
        | RestVariantEnum.PaidRest -> PaidRest
        | _ -> RegularRest
