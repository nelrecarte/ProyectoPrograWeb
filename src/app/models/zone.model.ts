export interface Zone {
  id: string;
  name: string;
  sector: string;
  description: string;
  isActive: boolean;
  // Id del corte abierto de la zona, si tiene uno
  activeReportId: string | null;
}

export interface ZoneRequest {
  name: string;
  sector: string;
  description: string;
}
