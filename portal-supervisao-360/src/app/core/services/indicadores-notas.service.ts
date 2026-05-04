import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashNotasRow } from '../../shared/models/indicadores-notas.interfaces';

@Injectable({
  providedIn: 'root'
})
export class IndicadoresNotasService {
  private base = `${environment.apiUrl}/DashIndicadoresNotas`;

  constructor(private http: HttpClient) {}

  select(): Observable<DashNotasRow[]> {
    return this.http.get<DashNotasRow[]>(`${this.base}/Select`);
  }
}
