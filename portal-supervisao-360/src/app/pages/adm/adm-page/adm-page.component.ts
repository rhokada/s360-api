import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AdmPageService } from '../../../core/services/adm-page.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmPage } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SidePanelComponent, ConfirmDialogComponent],
  templateUrl: './adm-page.component.html',
  styleUrls: ['./adm-page.component.scss']
})
export class AdmPageComponent implements OnInit, OnDestroy {
  pages: AdmPage[] = [];
  allPages: AdmPage[] = [];
  isLoading = false;
  isSaving = false;
  panelVisible = false;
  panelTitle = '';
  isEditing = false;
  editingId: number | null = null;
  showConfirm = false;
  deleteTargetId: number | null = null;
  filterForm!: FormGroup;
  form!: FormGroup;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private admPageService: AdmPageService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({ search: [''] });

    this.form = this.fb.group({
      slug: ['', Validators.required],
      menu: ['', Validators.required],
      icon: ['']
    });

    this.loadPages();

    this.filterForm.get('search')!.valueChanges.pipe(
      debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$)
    ).subscribe(() => this.applyFilters());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPages(): void {
    this.isLoading = true;
    this.admPageService.select().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.allPages = data;
        this.pages    = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar páginas.');
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    const term = (this.filterForm.value.search ?? '').toLowerCase();
    this.pages = this.allPages.filter(p =>
      !term ||
      p.slug.toLowerCase().includes(term) ||
      p.menu.toLowerCase().includes(term)
    );
  }

  openCreate(): void {
    this.isEditing    = false;
    this.editingId    = null;
    this.form.reset();
    this.panelTitle   = 'Nova Página';
    this.panelVisible = true;
  }

  openEdit(page: AdmPage): void {
    this.isEditing    = true;
    this.editingId    = page.admPageId;
    this.form.patchValue({ slug: page.slug, menu: page.menu, icon: page.icon ?? '' });
    this.panelTitle   = 'Editar Página';
    this.panelVisible = true;
  }

  closePanel(): void { this.panelVisible = false; }

  save(): void {
    if (this.form.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, admPageId: this.editingId }
      : this.form.value;
    const request$ = this.isEditing
      ? this.admPageService.update(body)
      : this.admPageService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Página atualizada!' : 'Página criada!');
        this.isSaving     = false;
        this.panelVisible = false;
        this.loadPages();
      },
      error: () => {
        this.toastService.error('Erro ao salvar página.');
        this.isSaving = false;
      }
    });
  }

  confirmDelete(id: number, event: Event): void {
    event.stopPropagation();
    this.deleteTargetId = id;
    this.showConfirm    = true;
  }

  onDeleteConfirmed(): void {
    if (this.deleteTargetId == null) return;
    this.admPageService.delete(this.deleteTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Página excluída!');
        this.showConfirm    = false;
        this.deleteTargetId = null;
        this.loadPages();
      },
      error: () => {
        this.toastService.error('Erro ao excluir página.');
        this.showConfirm = false;
      }
    });
  }

  onDeleteCancelled(): void {
    this.showConfirm    = false;
    this.deleteTargetId = null;
  }
}
