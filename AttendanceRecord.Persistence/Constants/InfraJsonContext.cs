using System.Text.Json;
using System.Text.Json.Serialization;
using AttendanceRecord.Persistence.Dtos;

namespace AttendanceRecord.Persistence.Constants;

[JsonSerializable(typeof(IEnumerable<WorkRecordFileDto>))]
[JsonSerializable(typeof(AppConfigFileDto))]
[JsonSerializable(typeof(NamedPipeMessage))]
public partial class InfraJsonContext : JsonSerializerContext
{
    public static InfraJsonContext Intended { get; } =
        new(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        });
}
