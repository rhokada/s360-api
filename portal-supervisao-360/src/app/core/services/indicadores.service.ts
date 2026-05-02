import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashRow } from '../../shared/models/indicadores.interfaces';

@Injectable({
  providedIn: 'root'
})
export class IndicadoresService {
  private base = `${environment.apiUrl}/DashIndicadores`;

  constructor(private http: HttpClient) {}

  select(): Observable<DashRow[]> {
    return this.http.get<DashRow[]>(`${this.base}/Select`);
  }
}
