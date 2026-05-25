import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Customer } from '../../shared/models/adm.interfaces';

export interface CustomerPagedResponse {
  items: Customer[];
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class AdmCustomerService {
  private base = `${environment.apiUrl}/AdmCustomer`;

  constructor(private http: HttpClient) {}

  select(filters: Record<string, any> = {}): Observable<Customer[]> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([k, v]) => {
      if (v != null && v !== '') params = params.set(k, v);
    });
    return this.http.get<Customer[]>(`${this.base}/Select`, { params });
  }

  selectPaged(filters: Record<string, any> = {}): Observable<CustomerPagedResponse> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([k, v]) => {
      if (v != null && v !== '') params = params.set(k, v);
    });
    return this.http.get<Customer[]>(`${this.base}/Select`, { params }).pipe(
      map(rows => ({
        items:      rows,
        totalCount: rows[0]?.totalCount ?? 0
      }))
    );
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
