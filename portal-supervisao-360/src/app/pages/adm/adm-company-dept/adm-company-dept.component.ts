import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AdmCompanyDeptService } from '../../../core/services/adm-company-dept.service';
import { AdmCompanyService } from '../../../core/services/adm-company.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmCompanyDept, AdmCompany } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-company-dept',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent],
  templateUrl: './adm-company-dept.component.html',
  styleUrls: ['./adm-company-dept.component.scss']
})
export class AdmCompanyDeptComponent implements OnInit, OnDestroy {
  companyId!: number;
  company: AdmCompany | null = null;
  depts: AdmCompanyDept[] = [];
  allDepts: AdmCompanyDept[] = [];
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
    private route: ActivatedRoute,
    private router: Router,
    private admCompanyDeptService: AdmCompanyDeptService,
    private admCompanyService: AdmCompanyService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.companyId = Number(this.route.snapshot.params['companyId']);

    this.filterForm = this.fb.group({ name: [''] });

    this.form = this.fb.group({
      name: ['', Validators.required],
      profitCenter: [''],
      costCenter: [''],
      addressId: [null]
    });

    this.loadCompany();
    this.loadDepts();

    this.filterForm.get('name')!.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => this.applyFilters());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCompany(): void {
    this.admCompanyService.select({ companyId: this.companyId }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.company = data[0] ?? null; },
      error: () => {}
    });
  }

  loadDepts(): void {
    this.isLoading = true;
    this.admCompanyDeptService.select({ companyId: this.companyId }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.allDepts = data;
        this.depts = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar departamentos.');
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    const { name } = this.filterForm.value;
    this.depts = this.allDepts.filter(d =>
      !name || d.name.toLowerCase().includes(name.toLowerCase())
    );
  }

  openCreate(): void {
    this.isEditing = false;
    this.editingId = null;
    this.form.reset();
    this.panelTitle = 'Novo Departamento';
    this.panelVisible = true;
  }

  openEdit(dept: AdmCompanyDept): void {
    this.isEditing = true;
    this.editingId = dept.companyDeptId;
    this.form.patchValue({
      name: dept.name,
      profitCenter: dept.profitCenter,
      costCenter: dept.costCenter,
      addressId: dept.addressId
    });
    this.panelTitle = 'Editar Departamento';
    this.panelVisible = true;
  }

  closePanel(): void {
    this.panelVisible = false;
  }

  save(): void {
    if (this.form.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, companyDeptId: this.editingId, companyId: this.companyId }
      : { ...this.form.value, companyId: this.companyId };
    const request$ = this.isEditing
      ? this.admCompanyDeptService.update(body)
      : this.admCompanyDeptService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Departamento atualizado!' : 'Departamento criado!');
        this.isSaving = false;
        this.panelVisible = false;
        this.loadDepts();
      },
      error: () => {
        this.toastService.error('Erro ao salvar departamento.');
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
    this.admCompanyDeptService.delete(this.deleteTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Departamento excluído!');
        this.showConfirm = false;
        this.deleteTargetId = null;
        this.loadDepts();
      },
      error: () => {
        this.toastService.error('Erro ao excluir departamento.');
        this.showConfirm = false;
      }
    });
  }

  onDeleteCancelled(): void {
    this.showConfirm = false;
    this.deleteTargetId = null;
  }

  goToUsers(deptId: number, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/adm/company', this.companyId, 'dept', deptId, 'users']);
  }
}
