import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { QuestionOptionService } from '../../../core/services/question-option.service';
import { QuestionOptionModel } from '../../../shared/models/survey-admin.models';

@Component({
  selector: 'app-question-option',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './question-option.component.html',
  styleUrls: ['./question-option.component.scss']
})
export class QuestionOptionComponent implements OnInit, OnChanges {
  @Input() questionId!: number;
  @Output() fechar = new EventEmitter<void>();

  items: QuestionOptionModel[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  createForm: FormGroup;

  // Edição por painel contextual (substitui inline)
  editItem: QuestionOptionModel | null = null;

  constructor(private service: QuestionOptionService, private fb: FormBuilder) {
    this.createForm = this.fb.group({
      rank: ['', [Validators.required, Validators.min(1)]],
      optionCd: ['', Validators.required],
      description: [''],
      complementQuestionId: [''],
      openMsgBox: [false],
      needNotes: [false]
    });
  }

  ngOnInit(): void { this.load(); }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['questionId'] && !changes['questionId'].firstChange) {
      this.load();
    }
  }

  load(): void {
    if (!this.questionId) return;
    this.loading = true;
    this.service.select({ questionId: this.questionId }).subscribe({
      next: data => { this.items = data; this.loading = false; },
      error: err => { this.errorMessage = err.message; this.loading = false; }
    });
  }

  onCreate(): void {
    if (this.createForm.invalid) return;
    const val = this.createForm.value;
    const model = {
      questionId: this.questionId,
      rank: Number(val.rank),
      optionCd: val.optionCd,
      description: val.description || undefined,
      complementQuestionId: val.complementQuestionId ? Number(val.complementQuestionId) : undefined,
      openMsgBox: val.openMsgBox,
      needNotes: val.needNotes
    };
    this.service.create(model).subscribe({
      next: () => {
        this.successMessage = 'Opção criada com sucesso.';
        this.createForm.reset({ openMsgBox: false, needNotes: false });
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }

  openEdit(item: QuestionOptionModel): void {
    this.editItem = { ...item };
  }

  cancelEdit(): void {
    this.editItem = null;
  }

  saveEdit(): void {
    if (!this.editItem) return;
    this.service.update(this.editItem).subscribe({
      next: () => {
        this.successMessage = 'Opção atualizada com sucesso.';
        this.editItem = null;
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
        this.successMessage = 'Opção removida.';
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }
}
