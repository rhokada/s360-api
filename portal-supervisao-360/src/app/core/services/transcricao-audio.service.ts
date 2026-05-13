import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TranscricaoAudioItem } from '../../shared/models/adm.interfaces';

@Injectable({
  providedIn: 'root'
})
export class TranscricaoAudioService {
  private base = `${environment.apiUrl}/TranscricaoAudio`;

  constructor(private http: HttpClient) {}

  select(): Observable<TranscricaoAudioItem[]> {
    return this.http.get<TranscricaoAudioItem[]>(`${this.base}/Select`);
  }
}
