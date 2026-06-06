import {
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  formatUserName,
  formatUserReference
} from '@core/user/utils/user-display.utils';
import { IssueAssignableUserDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  ProjectMemberDto,
  ProjectMembersSearchParameters,
  ProjectRole
} from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  getProjectRoleLabel,
  getProjectRoleSeverity,
  getRemoveMemberConfirmMessage,
  projectDeleteMenuItemDangerClasses,
  ProjectMessages,
  projectRoles
} from '@features/projects/utils/projects.utils';
import {
  getAccountBadgeLabel,
  getAccountBadgeSeverity
} from '@features/users/utils/users.utils';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { debounceTime, distinctUntilChanged, forkJoin } from 'rxjs';

@Component({
  selector: 'app-project-members-section',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    Button,
    InputText,
    TableModule,
    Message,
    Tag,
    Menu,
    Paginator,
    Dialog,
    Select,
    Tooltip
  ],
  templateUrl: './project-members-section.component.html'
})
export class ProjectMembersSectionComponent {
  readonly projectId = input.required<string>();
  readonly projectName = input.required<string>();
  readonly canManageMembers = input.required<boolean>();
  readonly membersChanged = output<void>();

  private readonly projectsService = inject(ProjectsService);
  private readonly issuesService = inject(IssuesService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly formatMemberName = (member: ProjectMemberDto): string =>
    formatUserName({
      firstName: member.firstName ?? '',
      lastName: member.lastName ?? ''
    });

  readonly formatMemberReference = (member: ProjectMemberDto): string =>
    formatUserReference({
      firstName: member.firstName ?? '',
      lastName: member.lastName ?? '',
      email: member.email
    });
  readonly getProjectRoleLabel = getProjectRoleLabel;
  readonly getProjectRoleSeverity = getProjectRoleSeverity;
  readonly getAccountBadgeLabel = getAccountBadgeLabel;
  readonly getAccountBadgeSeverity = getAccountBadgeSeverity;
  readonly projectRoles = projectRoles;
  readonly ProjectMessages = ProjectMessages;

  readonly members = signal<ProjectMemberDto[]>([]);
  readonly membersPagination = signal<PaginationResult<ProjectMemberDto> | null>(null);
  readonly membersQuery = signal<ProjectMembersSearchParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'Name',
    ascending: true
  });
  readonly isLoadingMembers = signal(true);
  readonly hasLoadedMembers = signal(false);
  readonly membersError = signal<string | null>(null);
  readonly memberActionItems = signal<MenuItem[]>([]);
  private readonly memberActionsMenu = viewChild.required<Menu>('memberActionsMenu');

  readonly membersSearchControl = new FormControl('', { nonNullable: true });

  readonly showAddMemberDialog = signal(false);
  readonly showChangeRoleDialog = signal(false);
  readonly selectedMember = signal<ProjectMemberDto | null>(null);
  readonly addableUsers = signal<IssueAssignableUserDto[]>([]);
  readonly isLoadingAddableUsers = signal(false);
  readonly isSubmittingMemberAction = signal(false);
  readonly memberDialogError = signal<string | null>(null);

  readonly addMemberForm = new FormGroup({
    userId: new FormControl<string | null>(null, Validators.required),
    role: new FormControl(ProjectRole.MEMBER, {
      nonNullable: true,
      validators: [Validators.required]
    })
  });

  readonly changeRoleForm = new FormGroup({
    role: new FormControl(ProjectRole.MEMBER, {
      nonNullable: true,
      validators: [Validators.required]
    })
  });

  private lastLoadedProjectId: string | null = null;

  constructor() {
    effect(() => {
      const projectId = this.projectId();

      if (projectId === this.lastLoadedProjectId) {
        return;
      }

      this.lastLoadedProjectId = projectId;
      this.membersQuery.set({
        pageNumber: 1,
        pageSize: 10,
        sortField: 'Name',
        ascending: true
      });
      this.membersSearchControl.setValue('', { emitEvent: false });
      this.loadMembers();
    });

    this.membersSearchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.membersQuery.update((current) => ({
          ...current,
          pageNumber: 1,
          searchText: this.membersSearchControl.value.trim() || undefined
        }));
        this.loadMembers();
      });
  }

  onMembersPageChange(event: PaginatorState): void {
    this.membersQuery.update((current) => ({
      ...current,
      pageNumber: (event.page ?? 0) + 1,
      pageSize: event.rows ?? current.pageSize
    }));
    this.loadMembers();
  }

  openAddMemberDialog(): void {
    this.memberDialogError.set(null);
    this.addMemberForm.reset({
      userId: null,
      role: ProjectRole.MEMBER
    });
    this.isLoadingAddableUsers.set(true);
    this.showAddMemberDialog.set(true);

    forkJoin({
      assignableUsers: this.issuesService.getAssignableUsers(),
      existingMembers: this.projectsService.getProjectMembers(this.projectId(), {
        pageNumber: 1,
        pageSize: 1000,
        sortField: 'Name',
        ascending: true
      })
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ assignableUsers, existingMembers }) => {
          const existingIds = new Set(existingMembers.items.map((m) => m.userId));
          this.addableUsers.set(
            assignableUsers.filter((user) => !existingIds.has(user.id))
          );
          this.isLoadingAddableUsers.set(false);
        },
        error: (error: Error) => {
          this.memberDialogError.set(error.message);
          this.isLoadingAddableUsers.set(false);
        }
      });
  }

  submitAddMember(): void {
    this.memberDialogError.set(null);

    if (this.addMemberForm.invalid) {
      this.addMemberForm.markAllAsTouched();
      return;
    }

    const raw = this.addMemberForm.getRawValue();
    if (!raw.userId) {
      return;
    }

    this.isSubmittingMemberAction.set(true);

    this.projectsService
      .addProjectMember(this.projectId(), {
        userId: raw.userId,
        role: raw.role
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(ProjectMessages.memberAdded);
          this.showAddMemberDialog.set(false);
          this.isSubmittingMemberAction.set(false);
          this.loadMembers();
          this.membersChanged.emit();
        },
        error: (error: Error) => {
          this.memberDialogError.set(error.message);
          this.isSubmittingMemberAction.set(false);
        }
      });
  }

  openChangeRoleDialog(member: ProjectMemberDto): void {
    this.selectedMember.set(member);
    this.memberDialogError.set(null);
    this.changeRoleForm.patchValue({ role: member.role });
    this.showChangeRoleDialog.set(true);
  }

  submitChangeRole(): void {
    const member = this.selectedMember();
    if (!member || this.changeRoleForm.invalid) {
      this.changeRoleForm.markAllAsTouched();
      return;
    }

    this.memberDialogError.set(null);
    this.isSubmittingMemberAction.set(true);

    this.projectsService
      .changeProjectMemberRole(this.projectId(), member.userId, {
        role: this.changeRoleForm.controls.role.value
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(ProjectMessages.memberRoleUpdated);
          this.showChangeRoleDialog.set(false);
          this.isSubmittingMemberAction.set(false);
          this.loadMembers();
          this.membersChanged.emit();
        },
        error: (error: Error) => {
          this.memberDialogError.set(error.message);
          this.isSubmittingMemberAction.set(false);
        }
      });
  }

  openMemberActionsMenu(event: Event, member: ProjectMemberDto): void {
    const items: MenuItem[] = [
      {
        label: 'Change role',
        icon: 'pi pi-sync',
        command: () => this.openChangeRoleDialog(member)
      },
      { separator: true },
      {
        label: 'Remove member',
        icon: 'pi pi-user-minus',
        ...projectDeleteMenuItemDangerClasses,
        command: () => this.confirmRemoveMember(member)
      }
    ];

    this.memberActionItems.set(items);
    this.memberActionsMenu().toggle(event);
  }

  confirmRemoveMember(member: ProjectMemberDto): void {
    this.confirmationService.confirm({
      header: 'Remove member',
      message: getRemoveMemberConfirmMessage(
        this.formatMemberReference(member),
        this.projectName()
      ),
      ...createDestructiveConfirmationOptions('Remove'),
      accept: () => {
        this.projectsService
          .removeProjectMember(this.projectId(), member.userId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.toastService.success(ProjectMessages.memberRemoved);
              this.loadMembers();
              this.membersChanged.emit();
            },
            error: (error: Error) => this.toastService.error(error.message)
          });
      }
    });
  }

  private loadMembers(): void {
    this.isLoadingMembers.set(true);
    this.membersError.set(null);

    this.projectsService
      .getProjectMembers(this.projectId(), this.membersQuery())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.members.set(result.items);
          this.membersPagination.set(result);
          this.isLoadingMembers.set(false);
          this.hasLoadedMembers.set(true);
        },
        error: (error: Error) => {
          this.membersError.set(error.message);
          this.isLoadingMembers.set(false);
        }
      });
  }
}
