import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AdmUserService } from '../../../core/services/adm-user.service';
import { AdmRoleService } from '../../../core/services/adm-role.service';
import { AdmRoleUserService } from '../../../core/services/adm-role-user.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmUser } from '../../../shared/models/adm.interfaces';
import { AdmRoleItem, AdmRoleUser } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-user-permissions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent],
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
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({ name: [''], email: [''], active: [''] });

    this.editForm = this.fb.group({
      name:       ['', Validators.required],
      email:      ['', [Validators.required, Validators.email]],
      dddCell:    [''],
      nrCell:     [''],
      appId:      [''],
      pbiLogin:   [''],
      contractId: [null],
      active:     [true]
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
      const matchName   = !name   || u.name.toLowerCase().includes(name.toLowerCase());
      const matchEmail  = !email  || u.email.toLowerCase().includes(email.toLowerCase());
      const matchActive = active === '' || u.active === (active === 'true');
      return matchName && matchEmail && matchActive;
    });
  }

  // ── Edição de usuário ─────────────────────────────────────────────────────
  openEdit(user: AdmUser): void {
    this.isEditing = true;
    this.editingId = user.userId;
    this.editForm.patchValue({
      name: user.name, email: user.email, dddCell: user.dddCell,
      nrCell: user.nrCell, appId: user.appId, pbiLogin: user.pbiLogin,
      contractId: user.contractId, active: user.active
    });
    this.editPanelTitle = 'Editar Usuário';
    this.editPanelVisible = true;
  }

  openCreate(): void {
    this.isEditing = false;
    this.editingId = null;
    this.editForm.reset({ active: true });
    this.editPanelTitle = 'Novo Usuário';
    this.editPanelVisible = true;
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
