namespace AttendanceRecord.Persistence.Dtos;

public record WorkEndAlarmConfigFileDto(bool IsEnabled, double BeforeEndMinutes, double SnoozeMinutes);

public record RestStartAlarmConfigFileDto(bool IsEnabled, double BeforeStartMinutes, double SnoozeMinutes);

public record AppConfigFileDto(
    string ThemeMode,
    double StandardWorkMinutes,
    WorkEndAlarmConfigFileDto WorkEndAlarm,
    RestStartAlarmConfigFileDto RestStartAlarm,
    string WorkStatusFormat
);
