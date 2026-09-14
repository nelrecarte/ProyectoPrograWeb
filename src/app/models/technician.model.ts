export interface Technician {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  zoneId: string;
  zoneName: string;
  isAvailable: boolean;
  isActive: boolean;
  // Cuántos reportes abiertos tiene asignados
  activeReportCount: number;
}

export interface CreateTechnicianRequest {
  userId: string;
  fullName: string;
  zoneId: string;
  isAvailable: boolean;
}

export interface UpdateTechnicianRequest {
  fullName: string;
  zoneId: string;
  isAvailable: boolean;
}
