import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { QuestionOptionModel, QuestionOptionCreateModel, QuestionOptionUpdateModel } from '../../shared/models/survey-admin.models';

@Injectable({ providedIn: 'root' })
export class QuestionOptionService {
  constructor(private api: ApiService) {}

  select(filtro: {
    questionOptionId?: number; questionId?: number; complementQuestionId?: number;
    rank?: number; optionCd?: string; description?: string; openMsgBox?: boolean; needNotes?: boolean;
  } = {}): Observable<QuestionOptionModel[]> {
    let params = new HttpParams();
    Object.entries(filtro).forEach(([k, v]) => {
      if (v != null) {
        params = params.set(k, String(v));
      }
    });
    return this.api.getWithParams<QuestionOptionModel[]>('/QuestionOption/Select', params);
  }

  create(model: QuestionOptionCreateModel): Observable<unknown> {
    return this.api.post('/QuestionOption/Create', model);
  }

  update(model: QuestionOptionUpdateModel): Observable<unknown> {
    return this.api.put('/QuestionOption/Update', model);
  }

  delete(id: number): Observable<unknown> {
    return this.api.delete(`/QuestionOption/Delete/${id}`);
  }
}
