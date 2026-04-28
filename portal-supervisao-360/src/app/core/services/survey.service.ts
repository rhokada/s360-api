import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { SurveyModel, SurveyCreateModel, SurveyUpdateModel } from '../../shared/models/survey-admin.models';

@Injectable({ providedIn: 'root' })
export class SurveyService {
  constructor(private api: ApiService) {}

  select(filtro: { surveyId?: number; surveyTypeId?: number; name?: string } = {}): Observable<SurveyModel[]> {
    let params = new HttpParams();
    Object.entries(filtro).forEach(([k, v]) => {
      if (v != null) {
        params = params.set(k, String(v));
      }
    });
    return this.api.getWithParams<SurveyModel[]>('/Survey/Select', params);
  }

  create(model: SurveyCreateModel): Observable<unknown> {
    return this.api.post('/Survey/Create', model);
  }

  update(model: SurveyUpdateModel): Observable<unknown> {
    return this.api.put('/Survey/Update', model);
  }

  delete(id: number): Observable<unknown> {
    return this.api.delete(`/Survey/Delete/${id}`);
  }
}
