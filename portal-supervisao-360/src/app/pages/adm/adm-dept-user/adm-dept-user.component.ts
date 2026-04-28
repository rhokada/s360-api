import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AdmDeptUserService } from '../../../core/services/adm-dept-user.service';
import { AdmCompanyService } from '../../../core/services/adm-company.service';
import { AdmCompanyDeptService } from '../../../core/services/adm-company-dept.service';
import { AdmUserService } from '../../../core/services/adm-user.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmDeptUser, AdmCompany, AdmCompanyDept, AdmUser } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-dept-user',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent],
  templateUrl: './adm-dept-user.component.html',
  styleUrls: ['./adm-dept-user.component.scss']
})
export class AdmDeptUserComponent implements OnInit, OnDestroy {
  companyId!: number;
  companyDeptId!: number;
  company: AdmCompany | null = null;
  dept: AdmCompanyDept | null = null;
  deptUsers: AdmDeptUser[] = [];
  isLoading = false;
  isSaving = false;
  panelVisible = false;
  panelTitle = '';
  isEditing = false;
  editingId: number | null = null;
  showConfirm = false;
  deleteTargetId: number | null = null;
  userSearchResults: AdmUser[] = [];
  showUserDropdown = false;
  selectedUserName = '';
  form!: FormGroup;
  private destroy$ = new Subject<void>();
  private userSearch$ = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private admDeptUserService: AdmDeptUserService,
    private admCompanyService: AdmCompanyService,
    private admCompanyDeptService: AdmCompanyDeptService,
    private admUserService: AdmUserService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.companyId = Number(this.route.snapshot.params['companyId']);
    this.companyDeptId = Number(this.route.snapshot.params['companyDeptId']);

    this.form = this.fb.group({
      userId: [null, Validators.required],
      title: [''],
      companyCodeUser: ['']
    });

    this.loadBreadcrumbData();
    this.loadDeptUsers();

    this.userSearch$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(name => {
      if (name && name.length >= 2) {
        this.admUserService.select({ name }).pipe(takeUntil(this.destroy$)).subscribe({
          next: (users) => {
            this.userSearchResults = users.slice(0, 10);
            this.showUserDropdown = this.userSearchResults.length > 0;
          },
          error: () => {}
        });
      } else {
        this.userSearchResults = [];
        this.showUserDropdown = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBreadcrumbData(): void {
    this.admCompanyService.select({ companyId: this.companyId }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.company = data[0] ?? null; },
      error: () => {}
    });
    this.admCompanyDeptService.select({ companyDeptId: this.companyDeptId }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.dept = data[0] ?? null; },
      error: () => {}
    });
  }

  loadDeptUsers(): void {
    this.isLoading = true;
    this.admDeptUserService.select({ companyDeptId: this.companyDeptId }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.deptUsers = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar funcionários.');
        this.isLoading = false;
      }
    });
  }

  onUserInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.selectedUserName = value;
    this.form.patchValue({ userId: null });
    this.userSearch$.next(value);
  }

  selectUser(user: AdmUser): void {
    this.form.patchValue({ userId: user.userId });
    this.selectedUserName = user.name;
    this.showUserDropdown = false;
    this.userSearchResults = [];
  }

  closeDropdown(): void {
    setTimeout(() => { this.showUserDropdown = false; }, 200);
  }

  openCreate(): void {
    this.isEditing = false;
    this.editingId = null;
    this.form.reset();
    this.selectedUserName = '';
    this.showUserDropdown = false;
    this.panelTitle = 'Adicionar Funcionário';
    this.panelVisible = true;
  }

  openEdit(deptUser: AdmDeptUser): void {
    this.isEditing = true;
    this.editingId = deptUser.deptUserId;
    this.selectedUserName = deptUser.userName;
    this.form.patchValue({
      userId: deptUser.userId,
      title: deptUser.title,
      companyCodeUser: deptUser.companyCodeUser
    });
    this.panelTitle = 'Editar Funcionário';
    this.panelVisible = true;
  }

  closePanel(): void {
    this.panelVisible = false;
  }

  save(): void {
    if (this.form.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, deptUserId: this.editingId, companyDeptId: this.companyDeptId }
      : { ...this.form.value, companyDeptId: this.companyDeptId };
    const request$ = this.isEditing
      ? this.admDeptUserService.update(body)
      : this.admDeptUserService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Funcionário atualizado!' : 'Funcionário adicionado!');
        this.isSaving = false;
        this.panelVisible = false;
        this.loadDeptUsers();
      },
      error: () => {
        this.toastService.error('Erro ao salvar funcionário.');
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
    this.admDeptUserService.delete(this.deleteTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Funcionário removido!');
        this.showConfirm = false;
        this.deleteTargetId = null;
        this.loadDeptUsers();
      },
      error: () => {
        this.toastService.error('Erro ao remover funcionário.');
        this.showConfirm = false;
      }
    });
  }

  onDeleteCancelled(): void {
    this.showConfirm = false;
    this.deleteTargetId = null;
  }
}
