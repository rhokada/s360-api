import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TreeNode, AdmHierarchyTeam } from '../../../shared/models/adm.interfaces';

export interface AddMemberEvent {
  bossId:   number;
  bossName: string;
}

@Component({
  selector: 'app-adm-tree-node',
  standalone: true,
  imports: [
    CommonModule,
    forwardRef(() => AdmTreeNodeComponent)
  ],
  templateUrl: './adm-tree-node.component.html',
  styleUrls: ['./adm-tree-node.component.scss']
})
export class AdmTreeNodeComponent {
  @Input() node!: TreeNode;
  @Input() hierarchyId!: number;
  @Output() addMember  = new EventEmitter<AddMemberEvent>();
  @Output() editMember = new EventEmitter<AdmHierarchyTeam>();
  @Output() deleteMember = new EventEmitter<AdmHierarchyTeam>();

  expanded = true;

  toggleExpand(): void { this.expanded = !this.expanded; }

  onAddMember(): void {
    this.addMember.emit({ bossId: this.node.userId, bossName: this.node.name });
  }

  onEditMember(member: AdmHierarchyTeam):   void { this.editMember.emit(member); }
  onDeleteMember(member: AdmHierarchyTeam): void { this.deleteMember.emit(member); }

  onChildAdd(event: AddMemberEvent):         void { this.addMember.emit(event); }
  onChildEdit(event: AdmHierarchyTeam):      void { this.editMember.emit(event); }
  onChildDelete(event: AdmHierarchyTeam):    void { this.deleteMember.emit(event); }
}
