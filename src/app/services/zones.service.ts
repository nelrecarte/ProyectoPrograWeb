import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Zone, ZoneRequest } from '../models/zone.model';

@Injectable({ providedIn: 'root' })
export class ZonesService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getAll(onlyActive?: boolean): Observable<Zone[]> {
    let params = new HttpParams();
    if (onlyActive !== undefined) {
      params = params.set('onlyActive', onlyActive);
    }
    return this.http.get<Zone[]>(this.apiUrl + '/zones', { params });
  }

  getById(id: string): Observable<Zone> {
    return this.http.get<Zone>(this.apiUrl + '/zones/' + id);
  }

  create(data: ZoneRequest): Observable<Zone> {
    return this.http.post<Zone>(this.apiUrl + '/zones', data);
  }

  update(id: string, data: ZoneRequest & { isActive: boolean }): Observable<Zone> {
    return this.http.put<Zone>(this.apiUrl + '/zones/' + id, data);
  }

  // Baja lógica: la zona se queda con su historial
  delete(id: string): Observable<void> {
    return this.http.delete<void>(this.apiUrl + '/zones/' + id);
  }
}
