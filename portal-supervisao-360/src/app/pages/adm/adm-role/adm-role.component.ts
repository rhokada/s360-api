import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AdmRoleService } from '../../../core/services/adm-role.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmRoleItem } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-role',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent],
  templateUrl: './adm-role.component.html',
  styleUrls: ['./adm-role.component.scss']
})
export class AdmRoleComponent implements OnInit, OnDestroy {
  roles: AdmRoleItem[] = [];
  allRoles: AdmRoleItem[] = [];
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
    private admRoleService: AdmRoleService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({ name: [''] });

    this.form = this.fb.group({
      admRoleCd:   ['', Validators.required],
      admRoleName: ['', Validators.required]
    });

    this.loadRoles();

    this.filterForm.get('name')!.valueChanges.pipe(
      debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$)
    ).subscribe(() => this.applyFilters());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadRoles(): void {
    this.isLoading = true;
    this.admRoleService.select().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.allRoles = data;
        this.roles    = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar perfis.');
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    const { name } = this.filterForm.value;
    this.roles = this.allRoles.filter(r =>
      !name || r.admRoleName.toLowerCase().includes(name.toLowerCase())
    );
  }

  openCreate(): void {
    this.isEditing  = false;
    this.editingId  = null;
    this.form.reset();
    this.panelTitle = 'Novo Perfil';
    this.panelVisible = true;
  }

  openEdit(role: AdmRoleItem): void {
    this.isEditing  = true;
    this.editingId  = role.admRoleId;
    this.form.patchValue({ admRoleCd: role.admRoleCd, admRoleName: role.admRoleName });
    this.panelTitle = 'Editar Perfil';
    this.panelVisible = true;
  }

  closePanel(): void { this.panelVisible = false; }

  save(): void {
    if (this.form.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, admRoleId: this.editingId }
      : this.form.value;
    const request$ = this.isEditing
      ? this.admRoleService.update(body)
      : this.admRoleService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Perfil atualizado!' : 'Perfil criado!');
        this.isSaving = false;
        this.panelVisible = false;
        this.loadRoles();
      },
      error: () => {
        this.toastService.error('Erro ao salvar perfil.');
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
    this.admRoleService.delete(this.deleteTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Perfil excluído!');
        this.showConfirm    = false;
        this.deleteTargetId = null;
        this.loadRoles();
      },
      error: () => {
        this.toastService.error('Erro ao excluir perfil.');
        this.showConfirm = false;
      }
    });
  }

  onDeleteCancelled(): void {
    this.showConfirm    = false;
    this.deleteTargetId = null;
  }

  goToPermissions(role: AdmRoleItem, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/adm/role', role.admRoleId, 'permissions']);
  }
}
