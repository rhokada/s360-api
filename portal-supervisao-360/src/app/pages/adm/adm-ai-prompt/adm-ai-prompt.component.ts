import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AiPromptService } from '../../../core/services/ai-prompt.service';
import { ToastService } from '../../../core/services/toast.service';
import { AiPromptItem } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';

@Component({
  selector: 'app-adm-ai-prompt',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SidePanelComponent],
  templateUrl: './adm-ai-prompt.component.html',
  styleUrls: ['./adm-ai-prompt.component.scss']
})
export class AdmAiPromptComponent implements OnInit, OnDestroy {
  prompts: AiPromptItem[] = [];
  isLoading = false;
  isSaving  = false;
  panelVisible = false;
  panelTitle   = '';
  isEditing    = false;
  editingId: number | null = null;
  form!: FormGroup;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private aiPromptService: AiPromptService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      aiProcessCd: ['', Validators.required],
      context:     [''],
      prompt:      ['', Validators.required],
      engine:      ['', Validators.required],
      log:         ['']
    });
    this.loadPrompts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPrompts(): void {
    this.isLoading = true;
    this.aiPromptService.select().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.prompts = data; this.isLoading = false; },
      error: () => { this.toastService.error('Erro ao carregar prompts.'); this.isLoading = false; }
    });
  }

  openCreate(): void {
    this.isEditing   = false;
    this.editingId   = null;
    this.form.reset();
    this.panelTitle  = 'Novo Prompt';
    this.panelVisible = true;
  }

  openEdit(item: AiPromptItem): void {
    this.isEditing  = true;
    this.editingId  = item.aiPromptId;
    this.form.patchValue({
      aiProcessCd: item.aiProcessCd,
      context:     item.context,
      prompt:      item.prompt,
      engine:      item.engine,
      log:         item.log
    });
    this.panelTitle  = 'Editar Prompt';
    this.panelVisible = true;
  }

  closePanel(): void { this.panelVisible = false; }

  save(): void {
    if (this.form.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, aiPromptId: this.editingId }
      : this.form.value;
    const request$ = this.isEditing
      ? this.aiPromptService.update(body)
      : this.aiPromptService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Prompt atualizado!' : 'Prompt criado!');
        this.isSaving     = false;
        this.panelVisible = false;
        this.loadPrompts();
      },
      error: () => {
        this.toastService.error('Erro ao salvar prompt.');
        this.isSaving = false;
      }
    });
  }

  truncate(text: string | null, len = 60): string {
    if (!text) return '-';
    return text.length > len ? text.substring(0, len) + '...' : text;
  }
}
