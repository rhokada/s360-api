import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AdmHierarchyService } from '../../../core/services/adm-hierarchy.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmHierarchy } from '../../../shared/models/adm.interfaces';

@Component({
  selector: 'app-adm-hierarchy',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './adm-hierarchy.component.html',
  styleUrls: ['./adm-hierarchy.component.scss']
})
export class AdmHierarchyComponent implements OnInit, OnDestroy {
  hierarchies: AdmHierarchy[] = [];
  isLoading = false;
  isSaving = false;
  isCreating = false;
  newHierarchyName = '';
  editingId: number | null = null;
  editingName = '';
  private destroy$ = new Subject<void>();

  constructor(
    private admHierarchyService: AdmHierarchyService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadHierarchies();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadHierarchies(): void {
    this.isLoading = true;
    this.admHierarchyService.select().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.hierarchies = data;
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar hierarquias.');
        this.isLoading = false;
      }
    });
  }

  startEdit(h: AdmHierarchy): void {
    this.editingId = h.hierarchyId;
    this.editingName = h.name;
  }

  cancelEdit(): void {
    this.editingId = null;
    this.editingName = '';
  }

  saveEdit(h: AdmHierarchy): void {
    if (!this.editingName.trim() || this.isSaving) return;
    this.isSaving = true;
    this.admHierarchyService.update({ ...h, name: this.editingName.trim() }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Hierarquia atualizada!');
        this.isSaving = false;
        this.editingId = null;
        this.loadHierarchies();
      },
      error: () => {
        this.toastService.error('Erro ao atualizar hierarquia.');
        this.isSaving = false;
      }
    });
  }

  showCreateRow(): void {
    this.isCreating = true;
    this.newHierarchyName = '';
  }

  cancelCreate(): void {
    this.isCreating = false;
    this.newHierarchyName = '';
  }

  create(): void {
    if (!this.newHierarchyName.trim() || this.isSaving) return;
    this.isSaving = true;
    this.admHierarchyService.create({ name: this.newHierarchyName.trim() }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Hierarquia criada!');
        this.isSaving = false;
        this.isCreating = false;
        this.newHierarchyName = '';
        this.loadHierarchies();
      },
      error: () => {
        this.toastService.error('Erro ao criar hierarquia.');
        this.isSaving = false;
      }
    });
  }

  confirmDelete(id: number): void {
    if (!confirm('Tem certeza que deseja excluir esta hierarquia?')) return;
    this.admHierarchyService.delete(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Hierarquia excluída!');
        this.loadHierarchies();
      },
      error: () => {
        this.toastService.error('Erro ao excluir hierarquia.');
      }
    });
  }
}
