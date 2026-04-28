import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { QuestionService } from '../../core/services/question.service';
import { QuestionModel } from '../../shared/models/survey-admin.models';
import { QuestionOptionComponent } from './question-option/question-option.component';

@Component({
  selector: 'app-question',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, QuestionOptionComponent],
  templateUrl: './question.component.html',
  styleUrls: ['./question.component.scss']
})
export class QuestionComponent implements OnInit {
  items: QuestionModel[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  // Filtros
  filterQuestion = '';
  filterAnswerTypeCd = '';
  filterGroup = '';
  filterIsComplement = '';
  filterIsFirstSurvey = '';
  filterIsFinalSurvey = '';
  filterIsFeedback = '';

  // Criação
  showCreateForm = false;
  createForm: FormGroup;

  // Edição (gaveta lateral)
  showEditPanel = false;
  editItem: QuestionModel | null = null;

  // Painel de opções
  showOptionsPanel = false;
  selectedQuestionId: number | null = null;

  constructor(private service: QuestionService, private fb: FormBuilder) {
    this.createForm = this.fb.group({
      rank: ['', [Validators.required, Validators.min(0)]],
      question: ['', Validators.required],
      description: [''],
      answerTypeCd: ['', Validators.required],
      group: [''],
      metric: [''],
      iconMetric: [''],
      isComplement: [false],
      isFirstSurvey: [false],
      isFinalSurvey: [false],
      isCompetenceLevel: [false],
      isFinishEarly: [false],
      isStandardMetric: [false],
      isSglYesNoType: [false],
      isFeedback: [false]
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';
    const filtro: Record<string, unknown> = {};
    if (this.filterQuestion) filtro['question'] = this.filterQuestion;
    if (this.filterAnswerTypeCd) filtro['answerTypeCd'] = this.filterAnswerTypeCd;
    if (this.filterGroup) filtro['group'] = this.filterGroup;
    if (this.filterIsComplement !== '') filtro['isComplement'] = this.filterIsComplement === 'true';
    if (this.filterIsFirstSurvey !== '') filtro['isFirstSurvey'] = this.filterIsFirstSurvey === 'true';
    if (this.filterIsFinalSurvey !== '') filtro['isFinalSurvey'] = this.filterIsFinalSurvey === 'true';
    if (this.filterIsFeedback !== '') filtro['isFeedback'] = this.filterIsFeedback === 'true';
    this.service.select(filtro).subscribe({
      next: data => { this.items = data; this.loading = false; },
      error: err => { this.errorMessage = err.message; this.loading = false; }
    });
  }

  onCreate(): void {
    if (this.createForm.invalid) return;
    const val = this.createForm.value;
    this.service.create({ ...val, rank: Number(val.rank) }).subscribe({
      next: () => {
        this.successMessage = 'Pergunta criada com sucesso.';
        this.createForm.reset({
          isComplement: false, isFirstSurvey: false, isFinalSurvey: false,
          isCompetenceLevel: false, isFinishEarly: false, isStandardMetric: false,
          isSglYesNoType: false, isFeedback: false
        });
        this.showCreateForm = false;
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }

  openEdit(item: QuestionModel): void {
    this.editItem = { ...item };
    this.showEditPanel = true;
  }

  closeEditPanel(): void {
    this.showEditPanel = false;
    this.editItem = null;
  }

  saveEdit(): void {
    if (!this.editItem) return;
    this.service.update(this.editItem).subscribe({
      next: () => {
        this.successMessage = 'Pergunta atualizada com sucesso.';
        this.closeEditPanel();
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
        this.successMessage = 'Pergunta removida.';
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }

  openOptions(questionId: number): void {
    this.selectedQuestionId = questionId;
    this.showOptionsPanel = true;
  }

  closePanel(): void {
    this.showOptionsPanel = false;
    this.selectedQuestionId = null;
  }

  truncate(text: string, max = 60): string {
    if (!text) return '';
    return text.length > max ? text.substring(0, max) + '...' : text;
  }
}
