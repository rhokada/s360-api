import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdmRolePermission } from '../../shared/models/adm.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AdmRolePermissionService {
  private base = `${environment.apiUrl}/AdmRolePermission`;

  constructor(private http: HttpClient) {}

  select(admRoleId: number): Observable<AdmRolePermission[]> {
    return this.http.get<AdmRolePermission[]>(`${this.base}/Select/${admRoleId}`);
  }

  upsert(body: {
    admRoleId: number;
    admPageId: number;
    read: boolean;
    create: boolean;
    delete: boolean;
    alter: boolean;
  }): Observable<any> {
    return this.http.post(`${this.base}/Upsert`, body);
  }
}
