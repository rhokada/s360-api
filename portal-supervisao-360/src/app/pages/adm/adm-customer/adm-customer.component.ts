import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, take } from 'rxjs/operators';
import { AdmCustomerService } from '../../../core/services/adm-customer.service';
import { ToastService } from '../../../core/services/toast.service';
import { Customer } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-customer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent],
  templateUrl: './adm-customer.component.html',
  styleUrls: ['./adm-customer.component.scss']
})
export class AdmCustomerComponent implements OnInit, OnDestroy {
  customers: Customer[] = [];
  totalCount  = 0;
  currentPage = 1;
  pageSize    = 20;
  isLoading   = false;
  isSaving    = false;
  panelVisible = false;
  panelTitle   = '';
  isEditing    = false;
  editingId: number | null = null;
  showConfirm  = false;
  deleteTargetId: number | null = null;
  filterForm!: FormGroup;
  form!: FormGroup;
  readonly pageSizeOptions = [10, 20, 50, 100];
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private admCustomerService: AdmCustomerService,
    private toastService: ToastService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({
      name:          [''],
      customerCode:  [''],
      city:          [''],
      state:         [''],
      toBeConfirmed: [''],
      sellerFilter:  ['']
    });

    this.form = this.fb.group({
      companyId:    [null, [Validators.required, Validators.min(1)]],
      name:         ['', Validators.required],
      customerCode: [''],
      cnpj:         [''],
      toBeConfirmed:[false],
      street:       [''],
      street2:      [''],
      neighborhood: [''],
      city:         [''],
      state:        [''],
      zipCode:      [''],
      originCd:     ['']
    });

    // Restaura estado a partir dos query params
    this.route.queryParams.pipe(take(1)).subscribe(params => {
      this.currentPage = Number(params['page']     || 1);
      this.pageSize    = Number(params['pageSize'] || 20);
      this.filterForm.patchValue({
        name:          params['name']          || '',
        customerCode:  params['customerCode']  || '',
        city:          params['city']          || '',
        state:         params['state']         || '',
        toBeConfirmed: params['toBeConfirmed'] || '',
        sellerFilter:  params['sellerFilter']  || ''
      });
      this.loadCustomers();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get pageEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalCount);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get pages(): number[] {
    const total = this.totalPages;
    const cur   = this.currentPage;
    const delta = 2;
    const range: number[] = [];
    for (let i = Math.max(1, cur - delta); i <= Math.min(total, cur + delta); i++) {
      range.push(i);
    }
    return range;
  }

  private buildFilters(): Record<string, any> {
    const { name, customerCode, city, state, toBeConfirmed, sellerFilter } = this.filterForm.value;
    return {
      ...(name          ? { name }          : {}),
      ...(customerCode  ? { customerCode }  : {}),
      ...(city          ? { city }          : {}),
      ...(state         ? { state }         : {}),
      ...(toBeConfirmed ? { toBeConfirmed } : {}),
      ...(sellerFilter  ? { sellerFilter }  : {}),
      pageNumber: this.currentPage,
      pageSize:   this.pageSize
    };
  }

  private syncQueryParams(): void {
    const { name, customerCode, city, state, toBeConfirmed, sellerFilter } = this.filterForm.value;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        ...(name          ? { name }          : {}),
        ...(customerCode  ? { customerCode }  : {}),
        ...(city          ? { city }          : {}),
        ...(state         ? { state }         : {}),
        ...(toBeConfirmed ? { toBeConfirmed } : {}),
        ...(sellerFilter  ? { sellerFilter }  : {}),
        page:     this.currentPage,
        pageSize: this.pageSize
      },
      replaceUrl: true
    });
  }

  loadCustomers(): void {
    this.isLoading = true;
    this.admCustomerService.selectPaged(this.buildFilters()).pipe(takeUntil(this.destroy$)).subscribe({
      next: ({ items, totalCount }) => {
        this.customers   = items;
        this.totalCount  = totalCount;
        this.isLoading   = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar clientes.');
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.syncQueryParams();
    this.loadCustomers();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.syncQueryParams();
    this.loadCustomers();
  }

  changePageSize(size: number): void {
    this.pageSize    = size;
    this.currentPage = 1;
    this.syncQueryParams();
    this.loadCustomers();
  }

  openCreate(): void {
    this.isEditing   = false;
    this.editingId   = null;
    this.form.reset({ toBeConfirmed: false, companyId: null });
    this.panelTitle  = 'Novo Cliente';
    this.panelVisible = true;
  }

  openEdit(customer: Customer): void {
    this.isEditing  = true;
    this.editingId  = customer.customerId;
    this.form.patchValue({
      companyId:    customer.companyId,
      name:         customer.name,
      customerCode: customer.customerCode,
      cnpj:         customer.cnpj,
      toBeConfirmed:customer.toBeConfirmed ?? false,
      street:       customer.street,
      street2:      customer.street2,
      neighborhood: customer.neighborhood,
      city:         customer.city,
      state:        customer.state,
      zipCode:      customer.zipCode,
      originCd:     customer.originCd
    });
    this.panelTitle  = 'Editar Cliente';
    this.panelVisible = true;
  }

  closePanel(): void { this.panelVisible = false; }

  save(): void {
    if (this.form.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, customerId: this.editingId }
      : this.form.value;
    const request$ = this.isEditing
      ? this.admCustomerService.update(body)
      : this.admCustomerService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Cliente atualizado!' : 'Cliente criado!');
        this.isSaving     = false;
        this.panelVisible = false;
        this.loadCustomers();
      },
      error: () => {
        this.toastService.error('Erro ao salvar cliente.');
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
    this.admCustomerService.delete(this.deleteTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Cliente excluído!');
        this.showConfirm    = false;
        this.deleteTargetId = null;
        this.loadCustomers();
      },
      error: () => {
        this.toastService.error('Erro ao excluir cliente.');
        this.showConfirm = false;
      }
    });
  }

  onDeleteCancelled(): void {
    this.showConfirm    = false;
    this.deleteTargetId = null;
  }

  goToSellers(customerId: number, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/adm/customers', customerId, 'sellers']);
  }
}
