import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AdmUserService } from '../../../core/services/adm-user.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmUser } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-user',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent],
  templateUrl: './adm-user.component.html',
  styleUrls: ['./adm-user.component.scss']
})
export class AdmUserComponent implements OnInit, OnDestroy {
  users: AdmUser[] = [];
  allUsers: AdmUser[] = [];
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
    private admUserService: AdmUserService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({
      name: [''],
      email: [''],
      active: ['']
    });

    this.form = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      dddCell: [''],
      nrCell: [''],
      appId: [''],
      pbiLogin: [''],
      contractId: [null],
      active: [true]
    });

    this.loadUsers();

    this.filterForm.get('name')!.valueChanges.pipe(
      debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$)
    ).subscribe(() => this.applyFilters());

    this.filterForm.get('email')!.valueChanges.pipe(
      debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$)
    ).subscribe(() => this.applyFilters());

    this.filterForm.get('active')!.valueChanges.pipe(
      takeUntil(this.destroy$)
    ).subscribe(() => this.applyFilters());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.admUserService.select().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.allUsers = data;
        this.users = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar usuários.');
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    const { name, email, active } = this.filterForm.value;
    this.users = this.allUsers.filter(u => {
      const matchName = !name || u.name.toLowerCase().includes(name.toLowerCase());
      const matchEmail = !email || u.email.toLowerCase().includes(email.toLowerCase());
      const matchActive = active === '' || u.active === (active === 'true');
      return matchName && matchEmail && matchActive;
    });
  }

  openCreate(): void {
    this.isEditing = false;
    this.editingId = null;
    this.form.reset({ active: true });
    this.panelTitle = 'Novo Usuário';
    this.panelVisible = true;
  }

  openEdit(user: AdmUser): void {
    this.isEditing = true;
    this.editingId = user.userId;
    this.form.patchValue({
      name: user.name,
      email: user.email,
      dddCell: user.dddCell,
      nrCell: user.nrCell,
      appId: user.appId,
      pbiLogin: user.pbiLogin,
      contractId: user.contractId,
      active: user.active
    });
    this.panelTitle = 'Editar Usuário';
    this.panelVisible = true;
  }

  closePanel(): void {
    this.panelVisible = false;
  }

  save(): void {
    if (this.form.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, userId: this.editingId }
      : this.form.value;
    const request$ = this.isEditing
      ? this.admUserService.update(body)
      : this.admUserService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Usuário atualizado!' : 'Usuário criado!');
        this.isSaving = false;
        this.panelVisible = false;
        this.loadUsers();
      },
      error: () => {
        this.toastService.error('Erro ao salvar usuário.');
        this.isSaving = false;
      }
    });
  }

  confirmDelete(id: number, event: Event): void {
    event.stopPropagation();
    this.deleteTargetId = id;
    this.showConfirm = true;
  }

  onDeleteConfirmed(): void {
    if (this.deleteTargetId == null) return;
    this.admUserService.delete(this.deleteTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Usuário excluído!');
        this.showConfirm = false;
        this.deleteTargetId = null;
        this.loadUsers();
      },
      error: () => {
        this.toastService.error('Erro ao excluir usuário.');
        this.showConfirm = false;
      }
    });
  }

  onDeleteCancelled(): void {
    this.showConfirm = false;
    this.deleteTargetId = null;
  }

  formatPhone(ddd: string | null, nr: string | null): string {
    if (!ddd && !nr) return '—';
    return `(${ddd ?? ''}) ${nr ?? ''}`.trim();
  }
}
