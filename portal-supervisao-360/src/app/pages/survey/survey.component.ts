import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SurveyService } from '../../core/services/survey.service';
import { SurveyTypeService } from '../../core/services/survey-type.service';
import { SurveyModel, SurveyTypeModel } from '../../shared/models/survey-admin.models';
import { SurveyQuestionComponent } from './survey-question/survey-question.component';
import { SurveySupComponent } from './survey-sup/survey-sup.component';

@Component({
  selector: 'app-survey',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    DatePipe,
    SurveyQuestionComponent,
    SurveySupComponent
  ],
  templateUrl: './survey.component.html',
  styleUrls: ['./survey.component.scss']
})
export class SurveyComponent implements OnInit {
  items: SurveyModel[] = [];
  surveyTypes: SurveyTypeModel[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  filterSurveyTypeId: number | null = null;

  createForm: FormGroup;
  editMap = new Map<number, SurveyModel>();

  showQuestionsPanel = false;
  showSupPanel = false;
  selectedSurveyId: number | null = null;

  constructor(
    private service: SurveyService,
    private surveyTypeService: SurveyTypeService,
    private fb: FormBuilder
  ) {
    this.createForm = this.fb.group({
      surveyTypeId: ['', Validators.required],
      name: ['', Validators.required],
      dtIni: ['', Validators.required],
      dtFin: ['']
    });
  }

  ngOnInit(): void {
    this.loadTypes();
    this.load();
  }

  loadTypes(): void {
    this.surveyTypeService.select().subscribe({
      next: data => this.surveyTypes = data,
      error: err => this.errorMessage = err.message
    });
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';
    const filtro: { surveyTypeId?: number } = {};
    if (this.filterSurveyTypeId) {
      filtro.surveyTypeId = this.filterSurveyTypeId;
    }
    this.service.select(filtro).subscribe({
      next: data => {
        this.items = data;
        this.loading = false;
      },
      error: err => {
        this.errorMessage = err.message;
        this.loading = false;
      }
    });
  }

  onCreate(): void {
    if (this.createForm.invalid) return;
    const val = this.createForm.value;
    const model = {
      surveyTypeId: Number(val.surveyTypeId),
      name: val.name,
      dtIni: val.dtIni,
      dtFin: val.dtFin || undefined
    };
    this.service.create(model).subscribe({
      next: () => {
        this.successMessage = 'Survey criado com sucesso.';
        this.createForm.reset();
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }

  startEdit(item: SurveyModel): void {
    this.editMap.set(item.surveyId, { ...item });
  }

  cancelEdit(id: number): void {
    this.editMap.delete(id);
  }

  isEditing(id: number): boolean {
    return this.editMap.has(id);
  }

  getEdit(id: number): SurveyModel {
    return this.editMap.get(id)!;
  }

  onUpdate(id: number): void {
    const model = this.getEdit(id);
    this.service.update(model).subscribe({
      next: () => {
        this.successMessage = 'Survey atualizado com sucesso.';
        this.editMap.delete(id);
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }

  onDelete(id: number): void {
    if (!window.confirm('Confirma exclusão?')) return;
    this.service.delete(id).subscribe({
      next: () => {
        this.successMessage = 'Survey removido.';
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }

  openQuestions(surveyId: number): void {
    this.selectedSurveyId = surveyId;
    this.showQuestionsPanel = true;
    this.showSupPanel = false;
  }

  openSup(surveyId: number): void {
    this.selectedSurveyId = surveyId;
    this.showSupPanel = true;
    this.showQuestionsPanel = false;
  }

  closePanel(): void {
    this.showQuestionsPanel = false;
    this.showSupPanel = false;
    this.selectedSurveyId = null;
  }

  getTypeName(id: number): string {
    const t = this.surveyTypes.find(x => x.surveyTypeId === id);
    return t ? t.name : String(id);
  }
}
