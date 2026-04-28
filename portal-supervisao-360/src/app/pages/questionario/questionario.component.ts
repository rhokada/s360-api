import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { StorageService, STORAGE_KEYS } from '../../core/services/storage.service';
import {
  Questions, QuestionResponse, PartialFormState,
  SurveyType, Alternative, SubmitAnswersPayload
} from '../../shared/models/interfaces';
import { showLoading, hideLoading } from '../../shared/components/loading/loading.component';

interface QuestionGroup {
  groupName: string;
  questions: Questions[];
}

@Component({
  selector: 'app-questionario',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './questionario.component.html',
  styleUrls: ['./questionario.component.scss']
})
export class QuestionarioComponent implements OnInit, OnDestroy {
  surveyType: SurveyType | null = null;
  allQuestions: Questions[] = [];
  questionGroups: QuestionGroup[] = [];
  currentGroupIndex = 0;
  partialState: PartialFormState | null = null;
  isSubmitting = false;
  submitError = '';
  submitSuccess = '';
  showConfirmSubmit = false;
  autoSaveTimer: ReturnType<typeof setInterval> | null = null;
  notesMap: { [questionId: string]: string } = {};

  get currentGroup(): QuestionGroup | null {
    return this.questionGroups[this.currentGroupIndex] || null;
  }

  get totalGroups(): number {
    return this.questionGroups.length;
  }

  get progress(): number {
    if (this.totalGroups === 0) return 0;
    return Math.round(((this.currentGroupIndex + 1) / this.totalGroups) * 100);
  }

  get isLastGroup(): boolean {
    return this.currentGroupIndex === this.totalGroups - 1;
  }

  get isFirstGroup(): boolean {
    return this.currentGroupIndex === 0;
  }

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private api: ApiService,
    private storage: StorageService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.surveyType = params['type'] as SurveyType;
      if (!this.surveyType) {
        this.router.navigate(['/avaliacoes']);
        return;
      }
      this.loadState();
      this.loadQuestions();
    });

    // Auto-save a cada 30 segundos
    this.autoSaveTimer = setInterval(() => this.savePartialState(), 30000);
  }

  ngOnDestroy(): void {
    if (this.autoSaveTimer) {
      clearInterval(this.autoSaveTimer);
    }
  }

  loadState(): void {
    this.partialState = this.storage.get<PartialFormState>(STORAGE_KEYS.PARTIAL_ANSWERS);
    if (!this.partialState) {
      this.partialState = {
        answers: {},
        SurveyType: this.surveyType || undefined,
        DtSurvey: new Date().toISOString()
      };
    }
    // Garante que answers existe
    if (!this.partialState.answers) {
      this.partialState.answers = {};
    }
  }

  loadQuestions(): void {
    const allStoredQuestions = this.storage.get<Questions[]>(STORAGE_KEYS.QUESTIONS) || [];
    this.allQuestions = allStoredQuestions.filter(q => q.SurveyType === this.surveyType);

    if (this.allQuestions.length === 0) {
      // Questões demo para desenvolvimento
      this.allQuestions = this.getDemoQuestions();
    }

    this.buildGroups();
  }

  buildGroups(): void {
    const groupMap = new Map<string, Questions[]>();

    this.allQuestions
      .filter(q => !q.QuestionIsComplement)
      .sort((a, b) => (a.Rank || 0) - (b.Rank || 0))
      .forEach(q => {
        const group = q.QuestionGroup || 'Geral';
        if (!groupMap.has(group)) {
          groupMap.set(group, []);
        }
        groupMap.get(group)!.push(q);
      });

    this.questionGroups = Array.from(groupMap.entries()).map(([groupName, questions]) => ({
      groupName,
      questions
    }));
  }

  getAnswer(questionId: string): QuestionResponse {
    if (!this.partialState!.answers[questionId]) {
      this.partialState!.answers[questionId] = {};
    }
    return this.partialState!.answers[questionId];
  }

  // SGL - Single choice
  selectSingle(questionId: string, alternative: Alternative): void {
    const answer = this.getAnswer(questionId);
    answer.alternativeId = alternative.id;
    answer.value = String(alternative.value || '');
    answer.text = alternative.text;
    this.savePartialState();

    // Verifica se deve finalizar cedo
    const question = this.findQuestion(questionId);
    if (question?.IsFinishEarly && alternative.triggersComplementQuestion) {
      this.showConfirmSubmit = true;
    }
  }

  isSelectedSingle(questionId: string, alternativeId: string): boolean {
    return this.getAnswer(questionId).alternativeId === alternativeId;
  }

  // MLT - Multiple choice
  toggleMultiple(questionId: string, alternative: Alternative): void {
    const answer = this.getAnswer(questionId);
    if (!answer.selectedMultipleAlternativesIds) {
      answer.selectedMultipleAlternativesIds = [];
      answer.selectedTexts = [];
    }

    const idx = answer.selectedMultipleAlternativesIds.indexOf(alternative.id);
    if (idx === -1) {
      answer.selectedMultipleAlternativesIds.push(alternative.id);
      answer.selectedTexts?.push(alternative.text);
    } else {
      answer.selectedMultipleAlternativesIds.splice(idx, 1);
      answer.selectedTexts?.splice(idx, 1);
    }
    this.savePartialState();
  }

  isSelectedMultiple(questionId: string, alternativeId: string): boolean {
    return !!(this.getAnswer(questionId).selectedMultipleAlternativesIds?.includes(alternativeId));
  }

  // VLR - Valor numérico
  setValueAnswer(questionId: string, value: string): void {
    this.getAnswer(questionId).value = value;
    this.savePartialState();
  }

  // TXT - Texto livre
  setTextAnswer(questionId: string, text: string): void {
    this.getAnswer(questionId).text = text;
    this.savePartialState();
  }

  setNotes(questionId: string, notes: string): void {
    this.getAnswer(questionId).notes = notes;
    this.notesMap[questionId] = notes;
    this.savePartialState();
  }

  findQuestion(questionId: string): Questions | undefined {
    return this.allQuestions.find(q => q.id === questionId);
  }

  savePartialState(): void {
    if (this.partialState) {
      this.storage.set(STORAGE_KEYS.PARTIAL_ANSWERS, this.partialState);
    }
  }

  nextGroup(): void {
    if (this.currentGroupIndex < this.totalGroups - 1) {
      this.savePartialState();
      this.currentGroupIndex++;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    } else {
      this.showConfirmSubmit = true;
    }
  }

  prevGroup(): void {
    if (this.currentGroupIndex > 0) {
      this.savePartialState();
      this.currentGroupIndex--;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  cancelSubmit(): void {
    this.showConfirmSubmit = false;
  }

  submitAnswers(): void {
    if (!this.partialState) return;

    this.isSubmitting = true;
    this.submitError = '';
    this.showConfirmSubmit = false;
    showLoading();

    const payload: SubmitAnswersPayload = {
      SurveyType: this.surveyType!,
      sellerId: this.partialState.sellerId,
      sellerCode: this.partialState.sellerCode,
      customerId: this.partialState.customerId,
      customerCode: this.partialState.customerCode,
      DtSurvey: this.partialState.DtSurvey || new Date().toISOString(),
      answers: this.partialState.answers
    };

    this.api.post('/app/SubmitAnswers', payload).subscribe({
      next: () => {
        // Salva localmente
        const submitted = this.storage.get<SubmitAnswersPayload[]>(STORAGE_KEYS.SUBMITTED_ANSWERS) || [];
        submitted.push({ ...payload, DtSurvey: new Date().toISOString() });
        this.storage.set(STORAGE_KEYS.SUBMITTED_ANSWERS, submitted);

        // Limpa estado parcial
        this.storage.remove(STORAGE_KEYS.PARTIAL_ANSWERS);

        this.submitSuccess = 'Avaliação enviada com sucesso!';
        this.isSubmitting = false;
        hideLoading();

        setTimeout(() => {
          this.router.navigate(['/resultados']);
        }, 2000);
      },
      error: (err: Error) => {
        this.submitError = err.message || 'Erro ao enviar avaliação. Tente novamente.';
        this.isSubmitting = false;
        hideLoading();
      }
    });
  }

  cancelSurvey(): void {
    const confirmed = window.confirm('Deseja cancelar a avaliação? Os dados parciais serão mantidos.');
    if (confirmed) {
      this.savePartialState();
      this.router.navigate(['/avaliacoes']);
    }
  }

  getDemoQuestions(): Questions[] {
    return [
      {
        id: 'q1', SurveyType: this.surveyType || 'CHECK_ROTA',
        text: 'O vendedor chegou no horário combinado?',
        type: 'SGL', QuestionIsComplement: false,
        QuestionGroup: 'Pontualidade', Metric: 'PONTUALIDADE', IconMetric: 'clock', Rank: 1,
        alternatives: [
          { id: 'a1', text: 'Sim, pontualmente', value: 10 },
          { id: 'a2', text: 'Não, com atraso', value: 0, triggersComplementQuestion: true }
        ]
      },
      {
        id: 'q2', SurveyType: this.surveyType || 'CHECK_ROTA',
        text: 'O vendedor apresentou o material de vendas adequadamente?',
        type: 'SGL', QuestionIsComplement: false,
        QuestionGroup: 'Apresentação', Metric: 'APRESENTACAO', IconMetric: 'presentation', Rank: 2,
        alternatives: [
          { id: 'a3', text: 'Ótima apresentação', value: 10 },
          { id: 'a4', text: 'Apresentação regular', value: 5 },
          { id: 'a5', text: 'Apresentação ruim', value: 0 }
        ]
      },
      {
        id: 'q3', SurveyType: this.surveyType || 'CHECK_ROTA',
        text: 'Quais materiais o vendedor utilizou?',
        type: 'MLT', QuestionIsComplement: false,
        QuestionGroup: 'Apresentação', Metric: 'MATERIAIS', IconMetric: 'file', Rank: 3,
        alternatives: [
          { id: 'a6', text: 'Tablet/smartphone' },
          { id: 'a7', text: 'Catálogo físico' },
          { id: 'a8', text: 'Amostras de produto' },
          { id: 'a9', text: 'Proposta personalizada' }
        ]
      },
      {
        id: 'q4', SurveyType: this.surveyType || 'CHECK_ROTA',
        text: 'Quantas visitas foram realizadas hoje?',
        type: 'VLR', QuestionIsComplement: false,
        QuestionGroup: 'Produtividade', Metric: 'VISITAS', IconMetric: 'users', Rank: 4
      },
      {
        id: 'q5', SurveyType: this.surveyType || 'CHECK_ROTA',
        text: 'Observações gerais sobre o desempenho do vendedor:',
        type: 'TXT', QuestionIsComplement: false,
        QuestionGroup: 'Observações', Metric: 'OBSERVACOES', IconMetric: 'note', Rank: 5
      }
    ];
  }
}
