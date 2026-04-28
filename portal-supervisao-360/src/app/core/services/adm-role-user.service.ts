import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdmRoleUser } from '../../shared/models/adm.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdmRoleUserService {
  private base = `${environment.apiUrl}/AdmRoleUser`;

  constructor(private http: HttpClient) {}

  select(filters: { admRoleId?: number; userId?: number } = {}): Observable<AdmRoleUser[]> {
    let params = new HttpParams();
    if (filters.admRoleId != null) params = params.set('admRoleId', filters.admRoleId);
    if (filters.userId    != null) params = params.set('userId',    filters.userId);
    return this.http.get<AdmRoleUser[]>(`${this.base}/Select`, { params });
  }

  create(body: { admRoleId: number; userId: number }): Observable<any> {
    return this.http.post(`${this.base}/Create`, body);
  }

  delete(admRoleUserId: number): Observable<any> {
    return this.http.delete(`${this.base}/Delete/${admRoleUserId}`);
  }

  deleteByRoleUser(admRoleId: number, userId: number): Observable<any> {
    const params = new HttpParams().set('admRoleId', admRoleId).set('userId', userId);
    return this.http.delete(`${this.base}/DeleteByRoleUser`, { params });
  }
}
