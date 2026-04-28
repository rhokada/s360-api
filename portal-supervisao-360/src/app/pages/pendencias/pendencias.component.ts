import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { StorageService, STORAGE_KEYS } from '../../core/services/storage.service';
import { FupItem } from '../../shared/models/interfaces';
import { showLoading, hideLoading } from '../../shared/components/loading/loading.component';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

@Component({
  selector: 'app-pendencias',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './pendencias.component.html',
  styleUrls: ['./pendencias.component.scss']
})
export class PendenciasComponent implements OnInit {
  fups: FupItem[] = [];
  filteredFups: FupItem[] = [];
  isLoading = false;
  error = '';
  successMessage = '';

  // Modal
  showModal = false;
  isEditing = false;
  editingId: string | null = null;
  isSaving = false;
  modalError = '';

  fupForm!: FormGroup;

  // Filtros
  filterStatus = '';
  filterPriority = '';
  filterCategory = '';

  categories = ['Geral', 'Vendas', 'Atendimento', 'Entrega', 'Produto', 'Administrativo'];

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private storage: StorageService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadFups();
  }

  initForm(fup?: FupItem): void {
    this.fupForm = this.fb.group({
      category: [fup?.category || '', Validators.required],
      description: [fup?.description || '', [Validators.required, Validators.minLength(5)]],
      dtExpectedConclusion: [
        fup?.dtExpectedConclusion ? fup.dtExpectedConclusion.split('T')[0] : '',
        Validators.required
      ],
      priority: [fup?.priority || 'medium', Validators.required],
      sellerCode: [fup?.sellerCode || ''],
      customerCode: [fup?.customerCode || ''],
      status: [fup?.status || 'pending']
    });
  }

  loadFups(): void {
    this.isLoading = true;
    this.error = '';
    showLoading();

    this.api.post<any>('/app/AppSupSellerFupList', {}).subscribe({
      next: (data) => {
        console.log('FUPs carregados:', data);
        this.fups = JSON.parse(data[0]?.JsonFup || '') as FupItem[];
        this.storage.set(STORAGE_KEYS.FUPS, this.fups);
        this.applyFilters();
        this.isLoading = false;
        hideLoading();
      },
      error: (err: Error) => {
        this.error = err.message;
        // Carrega do cache local
        this.fups = this.storage.get<FupItem[]>(STORAGE_KEYS.FUPS) || [];
        this.applyFilters();
        this.isLoading = false;
        hideLoading();
      }
    });
  }

  applyFilters(): void {
    this.filteredFups = this.fups.filter(f => {
      if (this.filterStatus && f.status !== this.filterStatus) return false;
      if (this.filterPriority && f.priority !== this.filterPriority) return false;
      if (this.filterCategory && f.category !== this.filterCategory) return false;
      return true;
    });
  }

  clearFilters(): void {
    this.filterStatus = '';
    this.filterPriority = '';
    this.filterCategory = '';
    this.applyFilters();
  }

  openCreateModal(): void {
    this.isEditing = false;
    this.editingId = null;
    this.modalError = '';
    this.initForm();
    this.showModal = true;
  }

  openEditModal(fup: FupItem): void {
    this.isEditing = true;
    this.editingId = fup.id;
    this.modalError = '';
    this.initForm(fup);
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.editingId = null;
    this.modalError = '';
  }

  saveFup(): void {
    if (this.fupForm.invalid) {
      this.fupForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.modalError = '';
    showLoading();

    const formValue = this.fupForm.value;
    const payload = {
      ...formValue,
      dtExpectedConclusion: new Date(formValue.dtExpectedConclusion).toISOString()
    };

    if (this.isEditing && this.editingId) {
      this.api.post('/app/UpdateFup', { ...payload, id: this.editingId }).subscribe({
        next: () => {
          const idx = this.fups.findIndex(f => f.id === this.editingId);
          if (idx !== -1) {
            this.fups[idx] = { ...this.fups[idx], ...payload };
          }
          this.storage.set(STORAGE_KEYS.FUPS, this.fups);
          this.applyFilters();
          this.closeModal();
          this.showSuccess('Pendência atualizada com sucesso!');
          this.isSaving = false;
          hideLoading();
        },
        error: (err: Error) => {
          this.modalError = err.message || 'Erro ao atualizar pendência.';
          this.isSaving = false;
          hideLoading();
        }
      });
    } else {
      this.api.post<FupItem>('/app/AddFup', payload).subscribe({
        next: (newFup) => {
          this.fups.unshift(newFup);
          this.storage.set(STORAGE_KEYS.FUPS, this.fups);
          this.applyFilters();
          this.closeModal();
          this.showSuccess('Pendência criada com sucesso!');
          this.isSaving = false;
          hideLoading();
        },
        error: (err: Error) => {
          this.modalError = err.message || 'Erro ao criar pendência.';
          this.isSaving = false;
          hideLoading();
        }
      });
    }
  }

  deleteFup(id: string): void {
    if (!window.confirm('Tem certeza que deseja excluir esta pendência?')) return;

    showLoading();
    this.api.delete(`/app/DeleteFup/${id}`).subscribe({
      next: () => {
        this.fups = this.fups.filter(f => f.id !== id);
        this.storage.set(STORAGE_KEYS.FUPS, this.fups);
        this.applyFilters();
        this.showSuccess('Pendência excluída.');
        hideLoading();
      },
      error: (err: Error) => {
        this.error = err.message || 'Erro ao excluir pendência.';
        hideLoading();
      }
    });
  }

  postponeFup(fup: FupItem): void {
    const newDate = prompt('Informe a nova data esperada (YYYY-MM-DD):', fup.dtExpectedConclusion.split('T')[0]);
    if (!newDate) return;

    showLoading();
    const updated = {
      ...fup,
      status: 'postponed' as const,
      dtExpectedConclusion: new Date(newDate).toISOString(),
      postponementCount: (fup.postponementCount || 0) + 1
    };

    this.api.post('/app/UpdateFup', updated).subscribe({
      next: () => {
        const idx = this.fups.findIndex(f => f.id === fup.id);
        if (idx !== -1) this.fups[idx] = updated;
        this.storage.set(STORAGE_KEYS.FUPS, this.fups);
        this.applyFilters();
        this.showSuccess('Pendência adiada.');
        hideLoading();
      },
      error: (err: Error) => {
        this.error = err.message;
        hideLoading();
      }
    });
  }

  completeFup(fup: FupItem): void {
    showLoading();
    const updated = {
      ...fup,
      status: 'completed' as const,
      dtConclusion: new Date().toISOString()
    };

    this.api.post('/app/UpdateFup', updated).subscribe({
      next: () => {
        const idx = this.fups.findIndex(f => f.id === fup.id);
        if (idx !== -1) this.fups[idx] = updated;
        this.storage.set(STORAGE_KEYS.FUPS, this.fups);
        this.applyFilters();
        this.showSuccess('Pendência concluída!');
        hideLoading();
      },
      error: (err: Error) => {
        this.error = err.message;
        hideLoading();
      }
    });
  }

  showSuccess(msg: string): void {
    this.successMessage = msg;
    setTimeout(() => this.successMessage = '', 4000);
  }

  formatDate(dateStr: string): string {
    try {
      return format(new Date(dateStr), "dd/MM/yyyy", { locale: ptBR });
    } catch {
      return dateStr;
    }
  }

  isOverdue(fup: FupItem): boolean {
    if (fup.status === 'completed') return false;
    return new Date(fup.dtExpectedConclusion) < new Date();
  }

  getStatusClass(status: string): string {
    const classes: { [key: string]: string } = {
      pending: 'badge-warning',
      completed: 'badge-success',
      postponed: 'badge-danger'
    };
    return classes[status] || 'badge-gray';
  }

  getStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      pending: 'Pendente',
      completed: 'Concluída',
      postponed: 'Adiada'
    };
    return labels[status] || status;
  }

  getPriorityClass(priority: string): string {
    const classes: { [key: string]: string } = {
      high: 'badge-danger',
      medium: 'badge-warning',
      low: 'badge-success'
    };
    return classes[priority] || 'badge-gray';
  }

  getPriorityLabel(priority: string): string {
    const labels: { [key: string]: string } = {
      high: 'Alta',
      medium: 'Média',
      low: 'Baixa'
    };
    return labels[priority] || priority;
  }

  hasFormError(field: string, error: string): boolean {
    const control = this.fupForm.get(field);
    return !!(control?.hasError(error) && control?.touched);
  }

  get pendingCount(): number { return this.fups.filter(f => f.status === 'pending').length; }
  get completedCount(): number { return this.fups.filter(f => f.status === 'completed').length; }
  get overdueCount(): number { return this.fups.filter(f => this.isOverdue(f)).length; }
}
