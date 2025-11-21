namespace AttendanceRecord.Persistence.Dtos;

public record NamedPipeMessage(string Sender, string Content, DateTime Timestamp);
