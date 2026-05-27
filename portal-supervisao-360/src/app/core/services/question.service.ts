import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { QuestionModel, QuestionCreateModel, QuestionUpdateModel } from '../../shared/models/survey-admin.models';

export interface QuestionFilter {
  questionId?: number;
  isComplement?: boolean;
  rank?: number;
  question?: string;
  answerTypeCd?: string;
  group?: string;
  metric?: string;
  isFirstSurvey?: boolean;
  isFinalSurvey?: boolean;
  isCompetenceLevel?: boolean;
  isFinishEarly?: boolean;
  isStandardMetric?: boolean;
  isSglYesNoType?: boolean;
  isFeedback?: boolean;
  surveyTypeId?: number;
}

@Injectable({ providedIn: 'root' })
export class QuestionService {
  constructor(private api: ApiService) {}

  select(filtro: QuestionFilter = {}): Observable<QuestionModel[]> {
    let params = new HttpParams();
    Object.entries(filtro).forEach(([k, v]) => {
      if (v != null) {
        params = params.set(k, String(v));
      }
    });
    return this.api.getWithParams<QuestionModel[]>('/Question/Select', params);
  }

  create(model: QuestionCreateModel): Observable<unknown> {
    return this.api.post('/Question/Create', model);
  }

  update(model: QuestionUpdateModel): Observable<unknown> {
    return this.api.put('/Question/Update', model);
  }

  delete(id: number): Observable<unknown> {
    return this.api.delete(`/Question/Delete/${id}`);
  }
}
