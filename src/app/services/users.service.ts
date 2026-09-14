import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Role } from '../models/auth.model';
import { UpdateProfileRequest, UserProfile } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  getMe(): Observable<UserProfile> {
    return this.http.get<UserProfile>(this.apiUrl + '/users/me');
  }

  updateMe(data: UpdateProfileRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(this.apiUrl + '/users/me', data);
  }

  getAll(role?: Role): Observable<UserProfile[]> {
    let params = new HttpParams();
    if (role) {
      params = params.set('role', role);
    }
    return this.http.get<UserProfile[]>(this.apiUrl + '/users', { params });
  }

  changeRole(id: string, role: Role): Observable<UserProfile> {
    return this.http.put<UserProfile>(this.apiUrl + '/users/' + id + '/role', { role });
  }
}
