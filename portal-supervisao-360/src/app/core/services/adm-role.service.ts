import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdmRoleItem, AdmRolePermission } from '../../shared/models/adm.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdmRoleService {
  private base = `${environment.apiUrl}/AdmRole`;

  constructor(private http: HttpClient) {}

  select(filters: Record<string, any> = {}): Observable<AdmRoleItem[]> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([k, v]) => {
      if (v != null && v !== '') params = params.set(k, v);
    });
    return this.http.get<AdmRoleItem[]>(`${this.base}/Select`, { params });
  }

  getPermissions(admRoleId: number): Observable<AdmRolePermission[]> {
    return this.http.get<AdmRolePermission[]>(`${this.base}/Permissions/${admRoleId}`);
  }

  create(body: object): Observable<any> {
    return this.http.post(`${this.base}/Create`, body);
  }

  update(body: object): Observable<any> {
    return this.http.put(`${this.base}/Update`, body);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.base}/Delete/${id}`);
  }
}
