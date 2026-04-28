import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { SurveySupModel, SurveySupCreateModel, SurveySupUpdateModel } from '../../shared/models/survey-admin.models';

@Injectable({ providedIn: 'root' })
export class SurveySupService {
  constructor(private api: ApiService) {}

  select(filtro: { surveySupId?: number; supUserId?: number; surveyId?: number; name?: string } = {}): Observable<SurveySupModel[]> {
    let params = new HttpParams();
    Object.entries(filtro).forEach(([k, v]) => {
      if (v != null) {
        params = params.set(k, String(v));
      }
    });
    return this.api.getWithParams<SurveySupModel[]>('/SurveySup/Select', params);
  }

  create(model: SurveySupCreateModel): Observable<unknown> {
    return this.api.post('/SurveySup/Create', model);
  }

  update(model: SurveySupUpdateModel): Observable<unknown> {
    return this.api.put('/SurveySup/Update', model);
  }

  delete(id: number): Observable<unknown> {
    return this.api.delete(`/SurveySup/Delete/${id}`);
  }
}
