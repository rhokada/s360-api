import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AdmRoleService } from '../../../core/services/adm-role.service';
import { AdmRolePermissionService } from '../../../core/services/adm-role-permission.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmRoleItem, AdmRolePermission } from '../../../shared/models/adm.interfaces';

@Component({
  selector: 'app-adm-role-permission',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './adm-role-permission.component.html',
  styleUrls: ['./adm-role-permission.component.scss']
})
export class AdmRolePermissionComponent implements OnInit, OnDestroy {
  role: AdmRoleItem | null = null;
  permissions: AdmRolePermission[] = [];
  isLoading = false;
  savingPageId: number | null = null;
  admRoleId = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private admRoleService: AdmRoleService,
    private admRolePermissionService: AdmRolePermissionService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.admRoleId = Number(this.route.snapshot.paramMap.get('roleId'));
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.isLoading = true;
    this.admRoleService.select({ admRoleId: this.admRoleId }).pipe(takeUntil(this.destroy$)).subscribe({
      next: (roles) => {
        this.role = roles[0] ?? null;
      }
    });
    this.admRolePermissionService.select(this.admRoleId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.permissions = data;
        this.isLoading   = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar permissões.');
        this.isLoading = false;
      }
    });
  }

  toggle(perm: AdmRolePermission, field: 'read' | 'create' | 'delete' | 'alter'): void {
    if (this.savingPageId === perm.admPageId) return;
    this.savingPageId = perm.admPageId;

    const updated = { ...perm, [field]: !perm[field] };

    this.admRolePermissionService.upsert({
      admRoleId: this.admRoleId,
      admPageId: perm.admPageId,
      read:      updated.read,
      create:    updated.create,
      delete:    updated.delete,
      alter:     updated.alter
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        const idx = this.permissions.findIndex(p => p.admPageId === perm.admPageId);
        if (idx >= 0) this.permissions[idx] = updated;
        this.savingPageId = null;
      },
      error: () => {
        this.toastService.error('Erro ao salvar permissão.');
        this.savingPageId = null;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/adm/role']);
  }
}
