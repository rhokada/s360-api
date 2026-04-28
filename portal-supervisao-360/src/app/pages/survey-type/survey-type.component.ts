import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SurveyTypeService } from '../../core/services/survey-type.service';
import { SurveyTypeModel } from '../../shared/models/survey-admin.models';

@Component({
  selector: 'app-survey-type',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './survey-type.component.html',
  styleUrls: ['./survey-type.component.scss']
})
export class SurveyTypeComponent implements OnInit {
  items: SurveyTypeModel[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  createForm: FormGroup;
  editMap = new Map<number, SurveyTypeModel>();

  constructor(
    private service: SurveyTypeService,
    private fb: FormBuilder
  ) {
    this.createForm = this.fb.group({
      surveyTypeCd: ['', Validators.required],
      name: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';
    this.service.select().subscribe({
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
    this.service.create(this.createForm.value).subscribe({
      next: () => {
        this.successMessage = 'Tipo de survey criado com sucesso.';
        this.createForm.reset();
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }

  startEdit(item: SurveyTypeModel): void {
    this.editMap.set(item.surveyTypeId, { ...item });
  }

  cancelEdit(id: number): void {
    this.editMap.delete(id);
  }

  isEditing(id: number): boolean {
    return this.editMap.has(id);
  }

  getEdit(id: number): SurveyTypeModel {
    return this.editMap.get(id)!;
  }

  onUpdate(id: number): void {
    const model = this.getEdit(id);
    this.service.update(model).subscribe({
      next: () => {
        this.successMessage = 'Tipo de survey atualizado com sucesso.';
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
        this.successMessage = 'Tipo de survey removido.';
        this.load();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: err => this.errorMessage = err.message
    });
  }
}
