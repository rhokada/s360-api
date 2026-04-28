import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
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
  allCustomers: Customer[] = [];
  customers: Customer[] = [];
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
    private admCustomerService: AdmCustomerService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.filterForm = this.fb.group({
      name: [''],
      customerCode: [''],
      city: [''],
      state: [''],
      toBeConfirmed: ['']
    });

    this.form = this.fb.group({
      companyId: [null, [Validators.required, Validators.min(1)]],
      name: ['', Validators.required],
      customerCode: [''],
      cnpj: [''],
      toBeConfirmed: [false],
      street: [''],
      street2: [''],
      neighborhood: [''],
      city: [''],
      state: [''],
      zipCode: [''],
      originCd: ['']
    });

    this.loadCustomers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCustomers(): void {
    this.isLoading = true;
    this.admCustomerService.select({}).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.allCustomers = data;
        this.applyFilters();
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar clientes.');
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    const { name, customerCode, city, state, toBeConfirmed } = this.filterForm.value;
    this.customers = this.allCustomers.filter(c => {
      const matchName = !name || c.name.toLowerCase().includes(name.toLowerCase());
      const matchCode = !customerCode || (c.customerCode ?? '').toLowerCase().includes(customerCode.toLowerCase());
      const matchCity = !city || (c.city ?? '').toLowerCase().includes(city.toLowerCase());
      const matchState = !state || (c.state ?? '').toLowerCase().includes(state.toLowerCase());
      let matchConfirmed = true;
      if (toBeConfirmed === 'true') matchConfirmed = c.toBeConfirmed === true;
      else if (toBeConfirmed === 'false') matchConfirmed = c.toBeConfirmed === false;
      return matchName && matchCode && matchCity && matchState && matchConfirmed;
    });
  }

  openCreate(): void {
    this.isEditing = false;
    this.editingId = null;
    this.form.reset({ toBeConfirmed: false, companyId: null });
    this.panelTitle = 'Novo Cliente';
    this.panelVisible = true;
  }

  openEdit(customer: Customer): void {
    this.isEditing = true;
    this.editingId = customer.customerId;
    this.form.patchValue({
      companyId: customer.companyId,
      name: customer.name,
      customerCode: customer.customerCode,
      cnpj: customer.cnpj,
      toBeConfirmed: customer.toBeConfirmed ?? false,
      street: customer.street,
      street2: customer.street2,
      neighborhood: customer.neighborhood,
      city: customer.city,
      state: customer.state,
      zipCode: customer.zipCode,
      originCd: customer.originCd
    });
    this.panelTitle = 'Editar Cliente';
    this.panelVisible = true;
  }

  closePanel(): void {
    this.panelVisible = false;
  }

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
        this.isSaving = false;
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
    this.showConfirm = true;
  }

  onDeleteConfirmed(): void {
    if (this.deleteTargetId == null) return;
    this.admCustomerService.delete(this.deleteTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Cliente excluído!');
        this.showConfirm = false;
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
    this.showConfirm = false;
    this.deleteTargetId = null;
  }

  goToSellers(customerId: number, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/adm/customers', customerId, 'sellers']);
  }
}
