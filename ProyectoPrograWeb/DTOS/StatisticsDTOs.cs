namespace ProyectoQ3Backend.DTOs;

public class StatisticsDto
{
    public int TotalReports { get; set; }
    public int ActiveReports { get; set; }
    public int ResolvedReports { get; set; }

    public int UnverifiedReports { get; set; }

    public double AverageResolutionMinutes { get; set; }

    public List<ZoneCountDto> ReportsByZone { get; set; } = [];

    public List<StatusCountDto> ReportsByStatus { get; set; } = [];

    public List<TechnicianStatsDto> TechnicianPerformance { get; set; } = [];

    public List<TrendPointDto> WeeklyTrend { get; set; } = [];

    public List<TrendPointDto> MonthlyTrend { get; set; } = [];

    public List<CauseStatsDto> ResolutionsByCause { get; set; } = [];
}

public class TrendPointDto
{
    public string Period { get; set; } = string.Empty;
    public int Reported { get; set; }
    public int Resolved { get; set; }
}

public class CauseStatsDto
{
    public string Cause { get; set; } = string.Empty;
    public int Total { get; set; }
    public double AverageResolutionMinutes { get; set; }
}

public class ZoneCountDto
{
    public string ZoneId { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Resolved { get; set; }
    public double ResolvedPercentage { get; set; }
    public double AverageResolutionMinutes { get; set; }
}

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class TechnicianStatsDto
{
    public string TechnicianId { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public int Assigned { get; set; }
    public int Resolved { get; set; }
    public double ResolvedPercentage { get; set; }
    public double AverageResolutionMinutes { get; set; }
}
