import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { SurveyQuestionModel, SurveyQuestionCreateModel } from '../../shared/models/survey-admin.models';

@Injectable({ providedIn: 'root' })
export class SurveyQuestionService {
  constructor(private api: ApiService) {}

  select(filtro: { surveyQuestionId?: number; surveyId?: number; questionId?: number } = {}): Observable<SurveyQuestionModel[]> {
    let params = new HttpParams();
    Object.entries(filtro).forEach(([k, v]) => {
      if (v != null) {
        params = params.set(k, String(v));
      }
    });
    return this.api.getWithParams<SurveyQuestionModel[]>('/SurveyQuestion/Select', params);
  }

  create(model: SurveyQuestionCreateModel): Observable<unknown> {
    return this.api.post('/SurveyQuestion/Create', model);
  }

  delete(id: number): Observable<unknown> {
    return this.api.delete(`/SurveyQuestion/Delete/${id}`);
  }
}
