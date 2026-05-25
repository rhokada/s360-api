import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { AdmCustomerSellerService } from '../../../core/services/adm-customer-seller.service';
import { AdmCustomerService } from '../../../core/services/adm-customer.service';
import { AdmUserService } from '../../../core/services/adm-user.service';
import { ToastService } from '../../../core/services/toast.service';
import { Customer, CustomerSeller, AdmUser } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-adm-customer-seller',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent],
  templateUrl: './adm-customer-seller.component.html',
  styleUrls: ['./adm-customer-seller.component.scss']
})
export class AdmCustomerSellerComponent implements OnInit, OnDestroy {
  customerId!: number;
  customer: Customer | null = null;
  allSellers: CustomerSeller[] = [];
  sellers: CustomerSeller[] = [];
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
  sellerSearch = '';
  userSuggestions: AdmUser[] = [];
  showUserDropdown = false;
  private sellerSearch$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private location: Location,
    private admCustomerSellerService: AdmCustomerSellerService,
    private admCustomerService: AdmCustomerService,
    private admUserService: AdmUserService,
    private toastService: ToastService
  ) {}

  goBack(): void { this.location.back(); }

  ngOnInit(): void {
    this.customerId = Number(this.route.snapshot.params['customerId']);

    this.filterForm = this.fb.group({
      active: ['']
    });

    this.form = this.fb.group({
      sellerUserId: [null, Validators.required],
      active: [true]
    });

    this.admCustomerService.select({ customerId: this.customerId })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.customer = data[0] ?? null;
        }
      });

    this.loadSellers();

    this.sellerSearch$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(value => {
      if (!value || value.trim().length < 2) {
        this.userSuggestions = [];
        this.showUserDropdown = false;
        return;
      }
      this.admUserService.select({ name: value }).pipe(takeUntil(this.destroy$)).subscribe({
        next: (users) => {
          this.userSuggestions = users;
          this.showUserDropdown = users.length > 0;
        },
        error: () => {
          this.userSuggestions = [];
          this.showUserDropdown = false;
        }
      });
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadSellers(): void {
    this.isLoading = true;
    this.admCustomerSellerService.select({ customerId: this.customerId })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.allSellers = data;
          this.applyFilters();
          this.isLoading = false;
        },
        error: () => {
          this.toastService.error('Erro ao carregar vendedores.');
          this.isLoading = false;
        }
      });
  }

  applyFilters(): void {
    const { active } = this.filterForm.value;
    this.sellers = this.allSellers.filter(s => {
      if (active === 'true') return s.active === true;
      if (active === 'false') return s.active === false;
      return true;
    });
  }

  onSellerSearchInput(value: string): void {
    this.sellerSearch = value;
    this.sellerSearch$.next(value);
    if (!value) {
      this.form.patchValue({ sellerUserId: null });
    }
  }

  selectUser(user: AdmUser): void {
    this.form.patchValue({ sellerUserId: user.userId });
    this.sellerSearch = `${user.name} (${user.email})`;
    this.userSuggestions = [];
    this.showUserDropdown = false;
  }

  openCreate(): void {
    this.isEditing = false;
    this.editingId = null;
    this.sellerSearch = '';
    this.userSuggestions = [];
    this.showUserDropdown = false;
    this.form.reset({ active: true, sellerUserId: null });
    this.panelTitle = 'Novo Vendedor';
    this.panelVisible = true;
  }

  openEdit(seller: CustomerSeller): void {
    this.isEditing = true;
    this.editingId = seller.customerSellerId;
    this.sellerSearch = `${seller.sellerName} (${seller.sellerEmail})`;
    this.userSuggestions = [];
    this.showUserDropdown = false;
    this.form.patchValue({
      sellerUserId: seller.sellerUserId,
      active: seller.active
    });
    this.panelTitle = 'Editar Vendedor';
    this.panelVisible = true;
  }

  closePanel(): void {
    this.panelVisible = false;
  }

  save(): void {
    if (this.form.invalid || this.isSaving) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, customerSellerId: this.editingId, customerId: this.customerId }
      : { ...this.form.value, customerId: this.customerId };
    const request$ = this.isEditing
      ? this.admCustomerSellerService.update(body)
      : this.admCustomerSellerService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Vendedor atualizado!' : 'Vendedor adicionado!');
        this.isSaving = false;
        this.panelVisible = false;
        this.loadSellers();
      },
      error: () => {
        this.toastService.error('Erro ao salvar vendedor.');
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
    this.admCustomerSellerService.delete(this.deleteTargetId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Vendedor removido!');
        this.showConfirm = false;
        this.deleteTargetId = null;
        this.loadSellers();
      },
      error: () => {
        this.toastService.error('Erro ao remover vendedor.');
        this.showConfirm = false;
      }
    });
  }

  onDeleteCancelled(): void {
    this.showConfirm = false;
    this.deleteTargetId = null;
  }
}
