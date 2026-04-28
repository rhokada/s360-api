import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AdmCompanyService } from '../../../core/services/adm-company.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmCompany } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-company',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent],
  templateUrl: './adm-company.component.html',
  styleUrls: ['./adm-company.component.scss']
})
export class AdmCompanyComponent implements OnInit, OnDestroy {
  companies: AdmCompany[] = [];
  allCompanies: AdmCompany[] = [];
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
    private admCompanyService: AdmCompanyService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({
      name: [''],
      taxID: ['']
    });

    this.form = this.fb.group({
      name: ['', Validators.required],
      taxID: [''],
      logoUrl: [''],
      parentCompanyId: [null],
      addressId: [null]
    });

    this.loadCompanies();

    this.filterForm.get('name')!.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => this.applyFilters());

    this.filterForm.get('taxID')!.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => this.applyFilters());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCompanies(): void {
    this.isLoading = true;
    this.admCompanyService.select().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.allCompanies = data;
        this.companies = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar empresas.');
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    const { name, taxID } = this.filterForm.value;
    this.companies = this.allCompanies.filter(c => {
      const matchName = !name || c.name.toLowerCase().includes(name.toLowerCase());
      const matchTaxID = !taxID || (c.taxID ?? '').toLowerCase().includes(taxID.toLowerCase());
      return matchName && matchTaxID;
    });
  }

  openCreate(): void {
    this.isEditing = false;
    this.editingId = null;
    this.form.reset();
    this.panelTitle = 'Nova Empresa';
    this.panelVisible = true;
  }

  openEdit(company: AdmCompany): void {
    this.isEditing = true;
    this.editingId = company.companyId;
    this.form.patchValue({
      name: company.name,
      taxID: company.taxID,
      logoUrl: company.logoUrl,
      parentCompanyId: company.parentCompanyId,
      addressId: company.addressId
    });
    this.panelTitle = 'Editar Empresa';
    this.panelVisible = true;
  }

  closePanel(): void {
    this.panelVisible = false;
  }

  save(): void {
    if (this.form.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, companyId: this.editingId }
      : this.form.value;
    const request$ = this.isEditing
      ? this.admCompanyService.update(body)
      : this.admCompanyService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Empresa atualizada!' : 'Empresa criada!');
        this.isSaving = false;
        this.panelVisible = false;
        this.loadCompanies();
      },
      error: () => {
        this.toastService.error('Erro ao salvar empresa.');
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
    this.admCompanyService.delete(this.deleteTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Empresa excluída!');
        this.showConfirm = false;
        this.deleteTargetId = null;
        this.loadCompanies();
      },
      error: () => {
        this.toastService.error('Erro ao excluir empresa.');
        this.showConfirm = false;
      }
    });
  }

  onDeleteCancelled(): void {
    this.showConfirm = false;
    this.deleteTargetId = null;
  }

  goToDepts(companyId: number, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/adm/company', companyId, 'dept']);
  }

  getParentName(parentId: number | null): string {
    if (!parentId) return '—';
    const parent = this.allCompanies.find(c => c.companyId === parentId);
    return parent?.name ?? '—';
  }
}
