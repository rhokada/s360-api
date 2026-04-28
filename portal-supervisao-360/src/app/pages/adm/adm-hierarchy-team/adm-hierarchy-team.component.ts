import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AdmHierarchyService } from '../../../core/services/adm-hierarchy.service';
import { AdmHierarchyTeamService } from '../../../core/services/adm-hierarchy-team.service';
import { AdmUserService } from '../../../core/services/adm-user.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdmHierarchy, AdmHierarchyTeam, TreeNode, AdmUser } from '../../../shared/models/adm.interfaces';
import { SidePanelComponent } from '../../../shared/components/side-panel/side-panel.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AdmTreeNodeComponent } from '../adm-tree-node/adm-tree-node.component';

function buildTree(data: AdmHierarchyTeam[]): TreeNode[] {
  const bossIds = new Set(data.map(i => i.bossId));
  const userIds = new Set(data.map(i => i.userId));
  const rootBossIds = [...bossIds].filter(id => !userIds.has(id));

  function makeNode(bossId: number, visited = new Set<number>()): TreeNode | null {
    if (visited.has(bossId)) return null;
    visited.add(bossId);
    const directReports = data.filter(i => i.bossId === bossId);
    const bossRecord = directReports[0];
    const subBossIds = [...new Set(directReports.map(r => r.userId))].filter(uid => bossIds.has(uid));
    return {
      userId: bossId,
      name: bossRecord?.bossName ?? '',
      email: bossRecord?.bossEmail ?? '',
      title: null,
      companyName: bossRecord?.companyName ?? null,
      deptName: bossRecord?.deptName ?? null,
      isRoot: rootBossIds.includes(bossId),
      members: directReports.filter(r => !bossIds.has(r.userId)),
      children: subBossIds.map(id => makeNode(id, new Set(visited))).filter((n): n is TreeNode => n !== null)
    };
  }
  return rootBossIds.map(id => makeNode(id)).filter((n): n is TreeNode => n !== null);
}

@Component({
  selector: 'app-adm-hierarchy-team',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidePanelComponent, ConfirmDialogComponent, AdmTreeNodeComponent],
  templateUrl: './adm-hierarchy-team.component.html',
  styleUrls: ['./adm-hierarchy-team.component.scss']
})
export class AdmHierarchyTeamComponent implements OnInit, OnDestroy {
  hierarchies: AdmHierarchy[] = [];
  selectedHierarchyId: number | null = null;
  teamData: AdmHierarchyTeam[] = [];
  tree: TreeNode[] = [];
  showActiveOnly = true;
  isLoadingHierarchies = false;
  isLoadingTeam = false;
  isSaving = false;
  panelVisible = false;
  panelTitle = '';
  isEditing = false;
  editingTeamId: number | null = null;
  showConfirm = false;
  deleteTarget: AdmHierarchyTeam | null = null;
  userSearchResults: AdmUser[] = [];
  showUserDropdown = false;
  selectedUserName = '';
  bossPrefillId: number | null = null;
  form!: FormGroup;
  private destroy$ = new Subject<void>();
  private userSearch$ = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private admHierarchyService: AdmHierarchyService,
    private admHierarchyTeamService: AdmHierarchyTeamService,
    private admUserService: AdmUserService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      userId: [null, Validators.required],
      bossId: [null, Validators.required],
      active: [true]
    });

    this.loadHierarchies();

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

  loadHierarchies(): void {
    this.isLoadingHierarchies = true;
    this.admHierarchyService.select().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.hierarchies = data;
        this.isLoadingHierarchies = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar hierarquias.');
        this.isLoadingHierarchies = false;
      }
    });
  }

  onSelectHierarchy(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedHierarchyId = value ? Number(value) : null;
    if (this.selectedHierarchyId) this.loadTeam();
    else { this.teamData = []; this.tree = []; }
  }

  loadTeam(): void {
    if (!this.selectedHierarchyId) return;
    this.isLoadingTeam = true;
    const filters: Record<string, any> = { hierarchyId: this.selectedHierarchyId };
    if (this.showActiveOnly) filters['active'] = true;
    this.admHierarchyTeamService.select(filters).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => {
        this.teamData = data;
        this.tree = buildTree(data);
        this.isLoadingTeam = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar membros da hierarquia.');
        this.isLoadingTeam = false;
      }
    });
  }

  toggleActiveFilter(): void {
    this.showActiveOnly = !this.showActiveOnly;
    if (this.selectedHierarchyId) this.loadTeam();
  }

  onAddMember(event: { bossId: number }): void {
    this.isEditing = false;
    this.editingTeamId = null;
    this.bossPrefillId = event.bossId;
    this.selectedUserName = '';
    this.form.reset({ bossId: event.bossId, active: true });
    this.panelTitle = 'Adicionar Membro';
    this.panelVisible = true;
  }

  onEditMember(member: AdmHierarchyTeam): void {
    this.isEditing = true;
    this.editingTeamId = member.hierarchyTeamId;
    this.selectedUserName = member.userName;
    this.form.patchValue({
      userId: member.userId,
      bossId: member.bossId,
      active: member.active
    });
    this.panelTitle = 'Editar Membro';
    this.panelVisible = true;
  }

  onDeleteMember(member: AdmHierarchyTeam): void {
    this.deleteTarget = member;
    this.showConfirm = true;
  }

  closePanel(): void {
    this.panelVisible = false;
    this.showUserDropdown = false;
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

  save(): void {
    if (this.form.invalid || this.isSaving || !this.selectedHierarchyId) return;
    this.isSaving = true;
    const body = this.isEditing
      ? { ...this.form.value, hierarchyTeamId: this.editingTeamId, hierarchyId: this.selectedHierarchyId }
      : { ...this.form.value, hierarchyId: this.selectedHierarchyId };
    const request$ = this.isEditing
      ? this.admHierarchyTeamService.update(body)
      : this.admHierarchyTeamService.create(body);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(this.isEditing ? 'Membro atualizado!' : 'Membro adicionado!');
        this.isSaving = false;
        this.panelVisible = false;
        this.loadTeam();
      },
      error: () => {
        this.toastService.error('Erro ao salvar membro.');
        this.isSaving = false;
      }
    });
  }

  onDeleteConfirmed(): void {
    if (!this.deleteTarget) return;
    this.admHierarchyTeamService.delete(this.deleteTarget.hierarchyTeamId).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success('Membro removido!');
        this.showConfirm = false;
        this.deleteTarget = null;
        this.loadTeam();
      },
      error: () => {
        this.toastService.error('Erro ao remover membro.');
        this.showConfirm = false;
      }
    });
  }

  onDeleteCancelled(): void {
    this.showConfirm = false;
    this.deleteTarget = null;
  }
}
