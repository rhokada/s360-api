import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AdmUserService } from '../../../core/services/adm-user.service';
import { AdmRoleService } from '../../../core/services/adm-role.service';
import { AdmRoleUserService } from '../../../core/services/adm-role-user.service';
import { AdmDeptUserService } from '../../../core/services/adm-dept-user.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmUser, AdmDeptUser } from '../../../shared/models/adm.interfaces';
import { AdmRoleItem, AdmRoleUser } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-user-permissions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent],
  templateUrl: './adm-user-permissions.component.html',
  styleUrls: ['./adm-user-permissions.component.scss']
})
export class AdmUserPermissionsComponent implements OnInit, OnDestroy {
  users: AdmUser[] = [];
  allUsers: AdmUser[] = [];
  isLoading = false;
  filterForm!: FormGroup;

  // painel de edição de usuário
  editPanelVisible = false;
  editPanelTitle = '';
  isEditing = false;
  editingId: number | null = null;
  isSaving = false;
  editForm!: FormGroup;
  showConfirm = false;
  deleteTargetId: number | null = null;

  // dept users no painel de edição
  userDeptUsers: AdmDeptUser[] = [];
  isLoadingDeptUsers = false;
  editingDeptUserId: number | null = null;
  deptUserEditTitle = '';
  deptUserEditCompanyCodeUser = '';
  isSavingDeptUser = false;
  showConfirmDeptUser = false;
  deleteDeptUserTargetId: number | null = null;

  // painel de permissões
  permPanelVisible = false;
  permPanelUser: AdmUser | null = null;
  allRoles: AdmRoleItem[] = [];
  userRoles: AdmRoleUser[] = [];
  isLoadingRoles = false;
  isSavingRole = false;

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private admUserService: AdmUserService,
    private admRoleService: AdmRoleService,
    private admRoleUserService: AdmRoleUserService,
    private admDeptUserService: AdmDeptUserService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({ name: [''], email: [''], active: [''] });

    this.editForm = this.fb.group({
      name:    ['', Validators.required],
      email:   ['', [Validators.required, Validators.email]],
      dddCell: [''],
      nrCell:  [''],
      active:  [true]
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
    ).subscribe((active: string) => this.applyFilters(active));
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

  applyFilters(overrideActive?: string): void {
    const { name, email, active: formActive } = this.filterForm.value;
    const active = overrideActive !== undefined ? overrideActive : formActive;
    this.users = this.allUsers.filter(u => {
      const matchName   = !name   || u.name.toLowerCase().includes(name.toLowerCase());
      const matchEmail  = !email  || u.email.toLowerCase().includes(email.toLowerCase());
      const matchActive = active === ''
        || (active === 'true' && !!u.active)
        || (active === 'false' && !u.active);
      return matchName && matchEmail && matchActive;
    });
  }

  // ── Edição de usuário ─────────────────────────────────────────────────────
  openEdit(user: AdmUser): void {
    this.isEditing = true;
    this.editingId = user.userId;
    this.editForm.patchValue({
      name: user.name, email: user.email, dddCell: user.dddCell,
      nrCell: user.nrCell, active: user.active
    });
    this.editPanelTitle = 'Editar Usuário';
    this.editPanelVisible = true;
    this.editingDeptUserId = null;
    this.loadUserDeptUsers(user.userId);
  }

  openCreate(): void {
    this.isEditing = false;
    this.editingId = null;
    this.editForm.reset({ active: true });
    this.editPanelTitle = 'Novo Usuário';
    this.editPanelVisible = true;
    this.userDeptUsers = [];
  }

  closeEditPanel(): void { this.editPanelVisible = false; }

  save(): void {
    if (this.editForm.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.editForm.value, userId: this.editingId }
      : this.editForm.value;
    const request$ = this.isEditing
      ? this.admUserService.update(body)
      : this.admUserService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Usuário atualizado!' : 'Usuário criado!');
        this.isSaving = false;
        this.editPanelVisible = false;
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

  // ── Dept Users no painel de edição ───────────────────────────────────────
  loadUserDeptUsers(userId: number): void {
    this.isLoadingDeptUsers = true;
    this.admDeptUserService.select({ userId }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.userDeptUsers = data; this.isLoadingDeptUsers = false; },
      error: () => { this.isLoadingDeptUsers = false; }
    });
  }

  openEditDeptUser(du: AdmDeptUser): void {
    this.editingDeptUserId = du.deptUserId;
    this.deptUserEditTitle = du.title ?? '';
    this.deptUserEditCompanyCodeUser = du.companyCodeUser ?? '';
  }

  cancelEditDeptUser(): void {
    this.editingDeptUserId = null;
  }

  saveDeptUser(du: AdmDeptUser): void {
    if (this.isSavingDeptUser) return;
    this.isSavingDeptUser = true;
    const body = {
      deptUserId: du.deptUserId,
      userId: du.userId,
      companyDeptId: du.companyDeptId,
      title: this.deptUserEditTitle,
      companyCodeUser: this.deptUserEditCompanyCodeUser
    };
    this.admDeptUserService.update(body).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Departamento atualizado!');
        this.isSavingDeptUser = false;
        this.editingDeptUserId = null;
        this.loadUserDeptUsers(du.userId);
      },
      error: () => {
        this.toastService.error('Erro ao atualizar departamento.');
        this.isSavingDeptUser = false;
      }
    });
  }

  confirmDeleteDeptUser(id: number): void {
    this.deleteDeptUserTargetId = id;
    this.showConfirmDeptUser = true;
  }

  onDeleteDeptUserConfirmed(): void {
    if (this.deleteDeptUserTargetId == null) return;
    this.admDeptUserService.delete(this.deleteDeptUserTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Departamento removido!');
        this.showConfirmDeptUser = false;
        this.deleteDeptUserTargetId = null;
        if (this.editingId) this.loadUserDeptUsers(this.editingId);
      },
      error: () => {
        this.toastService.error('Erro ao remover departamento.');
        this.showConfirmDeptUser = false;
      }
    });
  }

  onDeleteDeptUserCancelled(): void {
    this.showConfirmDeptUser = false;
    this.deleteDeptUserTargetId = null;
  }

  // ── Painel de permissões ──────────────────────────────────────────────────
  openPermissions(user: AdmUser, event: Event): void {
    event.stopPropagation();
    this.permPanelUser = user;
    this.permPanelVisible = true;
    this.loadPermissions(user.userId);
  }

  closePermPanel(): void { this.permPanelVisible = false; }

  private loadPermissions(userId: number): void {
    this.isLoadingRoles = true;
    forkJoin({
      roles:     this.admRoleService.select(),
      userRoles: this.admRoleUserService.select({ userId })
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: ({ roles, userRoles }) => {
        this.allRoles   = roles;
        this.userRoles  = userRoles;
        this.isLoadingRoles = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar perfis.');
        this.isLoadingRoles = false;
      }
    });
  }

  hasRole(admRoleId: number): boolean {
    return this.userRoles.some(r => r.admRoleId === admRoleId);
  }

  toggleRole(role: AdmRoleItem): void {
    if (!this.permPanelUser || this.isSavingRole) return;
    const userId     = this.permPanelUser.userId;
    const existing   = this.userRoles.find(r => r.admRoleId === role.admRoleId);

    this.isSavingRole = true;

    if (existing) {
      this.admRoleUserService.delete(existing.admRoleUserId).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.userRoles = this.userRoles.filter(r => r.admRoleUserId !== existing.admRoleUserId);
          this.isSavingRole = false;
        },
        error: () => {
          this.toastService.error('Erro ao remover perfil.');
          this.isSavingRole = false;
        }
      });
    } else {
      this.admRoleUserService.create({ admRoleId: role.admRoleId, userId }).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.admRoleUserService.select({ userId }).pipe(takeUntil(this.destroy$)).subscribe({
            next: (data) => { this.userRoles = data; this.isSavingRole = false; },
            error: () => { this.isSavingRole = false; }
          });
        },
        error: () => {
          this.toastService.error('Erro ao adicionar perfil.');
          this.isSavingRole = false;
        }
      });
    }
  }

  formatPhone(ddd: string | null, nr: string | null): string {
    if (!ddd && !nr) return '—';
    return `(${ddd ?? ''}) ${nr ?? ''}`.trim();
  }
}
