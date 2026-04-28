import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SurveyQuestionService } from '../../../core/services/survey-question.service';
import { QuestionService } from '../../../core/services/question.service';
import { SurveyQuestionModel, QuestionModel } from '../../../shared/models/survey-admin.models';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-survey-question',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './survey-question.component.html',
  styleUrls: ['./survey-question.component.scss']
})
export class SurveyQuestionComponent implements OnInit, OnChanges {
  @Input() surveyId!: number;
  @Output() fechar = new EventEmitter<void>();

  vinculados: SurveyQuestionModel[] = [];
  searchTerm = '';
  searchResults: QuestionModel[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  private searchSubject = new Subject<string>();

  constructor(
    private surveyQuestionService: SurveyQuestionService,
    private questionService: QuestionService
  ) {}

  ngOnInit(): void {
    this.loadVinculados();
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(term => {
        if (term.length < 3) {
          this.searchResults = [];
          return [];
        }
        return this.questionService.select({ question: term });
      })
    ).subscribe({
      next: results => this.searchResults = results as QuestionModel[],
      error: err => this.errorMessage = err.message
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['surveyId'] && !changes['surveyId'].firstChange) {
      this.loadVinculados();
    }
  }

  loadVinculados(): void {
    if (!this.surveyId) return;
    this.loading = true;
    this.surveyQuestionService.select({ surveyId: this.surveyId }).subscribe({
      next: data => {
        this.vinculados = data;
        this.loading = false;
      },
      error: err => {
        this.errorMessage = err.message;
        this.loading = false;
      }
    });
  }

  onSearch(term: string): void {
    this.searchSubject.next(term);
  }

  vincular(question: QuestionModel): void {
    const jaVinculado = this.vinculados.some(v => v.questionId === question.questionId);
    if (jaVinculado) {
      this.errorMessage = 'Esta pergunta já está vinculada a este survey.';
      return;
    }
    this.surveyQuestionService.create({ surveyId: this.surveyId, questionId: question.questionId }).subscribe({
      next: () => {
        this.successMessage = 'Pergunta vinculada com sucesso.';
        this.searchTerm = '';
        this.searchResults = [];
        this.loadVinculados();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }

  desvincular(id: number): void {
    if (!window.confirm('Confirma exclusão?')) return;
    this.surveyQuestionService.delete(id).subscribe({
      next: () => {
        this.successMessage = 'Vínculo removido.';
        this.loadVinculados();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }
}
