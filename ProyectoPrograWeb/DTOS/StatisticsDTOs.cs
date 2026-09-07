namespace ProyectoQ3Backend.DTOs;

/// <summary>Todo lo que necesita el dashboard del administrador, en una sola llamada.</summary>
public class StatisticsDto
{
    public int TotalReports { get; set; }
    public int ActiveReports { get; set; }
    public int ResolvedReports { get; set; }

    /// <summary>Reportes que siguen en estado "nuevo": nadie los ha verificado.</summary>
    public int UnverifiedReports { get; set; }

    /// <summary>Minutos promedio entre el inicio del corte y el restablecimiento.</summary>
    public double AverageResolutionMinutes { get; set; }

    /// <summary>Para el grafico de barras: cuantos cortes lleva cada zona.</summary>
    public List<ZoneCountDto> ReportsByZone { get; set; } = [];

    /// <summary>Para el grafico circular: como se reparten los reportes por estado.</summary>
    public List<StatusCountDto> ReportsByStatus { get; set; } = [];

    public List<TechnicianStatsDto> TechnicianPerformance { get; set; } = [];
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
