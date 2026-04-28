import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { SurveySupService } from '../../../core/services/survey-sup.service';
import { SurveySupModel } from '../../../shared/models/survey-admin.models';

@Component({
  selector: 'app-survey-sup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './survey-sup.component.html',
  styleUrls: ['./survey-sup.component.scss']
})
export class SurveySupComponent implements OnInit, OnChanges {
  @Input() surveyId!: number;
  @Output() fechar = new EventEmitter<void>();

  items: SurveySupModel[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  createForm: FormGroup;
  editMap = new Map<number, SurveySupModel>();

  constructor(
    private service: SurveySupService,
    private fb: FormBuilder
  ) {
    this.createForm = this.fb.group({
      supUserId: ['', [Validators.required, Validators.min(1)]],
      name: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.load();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['surveyId'] && !changes['surveyId'].firstChange) {
      this.load();
    }
  }

  load(): void {
    if (!this.surveyId) return;
    this.loading = true;
    this.service.select({ surveyId: this.surveyId }).subscribe({
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
    this.service.create({ supUserId: Number(val.supUserId), surveyId: this.surveyId, name: val.name }).subscribe({
      next: () => {
        this.successMessage = 'Supervisor adicionado com sucesso.';
        this.createForm.reset();
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }

  startEdit(item: SurveySupModel): void {
    this.editMap.set(item.surveySupId, { ...item });
  }

  cancelEdit(id: number): void {
    this.editMap.delete(id);
  }

  isEditing(id: number): boolean {
    return this.editMap.has(id);
  }

  getEdit(id: number): SurveySupModel {
    return this.editMap.get(id)!;
  }

  onUpdate(id: number): void {
    const model = this.getEdit(id);
    this.service.update(model).subscribe({
      next: () => {
        this.successMessage = 'Supervisor atualizado com sucesso.';
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
        this.successMessage = 'Supervisor removido.';
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }
}
