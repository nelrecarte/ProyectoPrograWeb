import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Statistics } from '../models/statistics.model';

@Injectable({ providedIn: 'root' })
export class StatisticsService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  // Trae todo lo del dashboard del administrador en una sola llamada
  get(zoneId?: string): Observable<Statistics> {
    let params = new HttpParams();
    if (zoneId) {
      params = params.set('zoneId', zoneId);
    }
    return this.http.get<Statistics>(this.apiUrl + '/statistics', { params });
  }
}
