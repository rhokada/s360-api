import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AiPromptItem } from '../../shared/models/adm.interfaces';

@Injectable({
  providedIn: 'root'
})
export class AiPromptService {
  private base = `${environment.apiUrl}/AiPrompt`;

  constructor(private http: HttpClient) {}

  select(): Observable<AiPromptItem[]> {
    return this.http.get<AiPromptItem[]>(`${this.base}/Select`);
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
