import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CreateTechnicianRequest,
  Technician,
  UpdateTechnicianRequest,
} from '../models/technician.model';

@Injectable({ providedIn: 'root' })
export class TechniciansService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getAll(): Observable<Technician[]> {
    return this.http.get<Technician[]>(this.apiUrl + '/technicians');
  }

  getMe(): Observable<Technician> {
    return this.http.get<Technician>(this.apiUrl + '/technicians/me');
  }

  // Además de crear la ficha, sube al usuario al rol Tecnico
  create(data: CreateTechnicianRequest): Observable<Technician> {
    return this.http.post<Technician>(this.apiUrl + '/technicians', data);
  }

  update(id: string, data: UpdateTechnicianRequest): Observable<Technician> {
    return this.http.put<Technician>(this.apiUrl + '/technicians/' + id, data);
  }

  deactivate(id: string): Observable<Technician> {
    return this.http.patch<Technician>(this.apiUrl + '/technicians/' + id + '/deactivate', {});
  }

  activate(id: string): Observable<Technician> {
    return this.http.patch<Technician>(this.apiUrl + '/technicians/' + id + '/activate', {});
  }
}
