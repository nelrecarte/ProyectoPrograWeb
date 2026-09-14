import { ReportStatus } from './report.model';

export interface ZoneCount {
  zoneId: string;
  zoneName: string;
  total: number;
  resolved: number;
  resolvedPercentage: number;
  averageResolutionMinutes: number;
}

export interface StatusCount {
  status: ReportStatus;
  total: number;
}

export interface TechnicianStats {
  technicianId: string;
  technicianName: string;
  assigned: number;
  resolved: number;
  resolvedPercentage: number;
  averageResolutionMinutes: number;
}

export interface Statistics {
  totalReports: number;
  activeReports: number;
  resolvedReports: number;
  unverifiedReports: number;
  averageResolutionMinutes: number;
  reportsByZone: ZoneCount[];
  reportsByStatus: StatusCount[];
  technicianPerformance: TechnicianStats[];
}
