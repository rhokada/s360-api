import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { SurveyTypeModel, SurveyTypeCreateModel, SurveyTypeUpdateModel } from '../../shared/models/survey-admin.models';

@Injectable({ providedIn: 'root' })
export class SurveyTypeService {
  constructor(private api: ApiService) {}

  select(filtro: { surveyTypeId?: number; surveyTypeCd?: string; name?: string } = {}): Observable<SurveyTypeModel[]> {
    let params = new HttpParams();
    Object.entries(filtro).forEach(([k, v]) => {
      if (v != null) {
        params = params.set(k, String(v));
      }
    });
    return this.api.getWithParams<SurveyTypeModel[]>('/SurveyType/Select', params);
  }

  create(model: SurveyTypeCreateModel): Observable<unknown> {
    return this.api.post('/SurveyType/Create', model);
  }

  update(model: SurveyTypeUpdateModel): Observable<unknown> {
    return this.api.put('/SurveyType/Update', model);
  }

  delete(id: number): Observable<unknown> {
    return this.api.delete(`/SurveyType/Delete/${id}`);
  }
}
