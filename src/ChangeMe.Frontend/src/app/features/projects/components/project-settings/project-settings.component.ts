import { DatePipe } from '@angular/common';
import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { IssueAssignableUserDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  ProjectDetailsDto,
  ProjectMemberDto,
  ProjectMemberRole,
  ProjectStatus,
  ProjectVisibility,
  UpdateProjectRequest
} from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  canManageProjectResource,
  getProjectMemberRoleLabel,
  getRemoveProjectMemberConfirmMessage,
  normalizeProjectKey,
  ProjectConstraints,
  projectMemberRoles,
  ProjectMessages,
  projectStatuses,
  projectVisibilities
} from '@features/projects/utils/projects.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Textarea } from 'primeng/textarea';
import { map, switchMap } from 'rxjs';

@Component({
  selector: 'app-project-settings',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    DatePipe,
    Card,
    Button,
    InputText,
    Textarea,
    Select,
    Message,
    Panel,
    TableModule,
    Tag
  ],
  providers: [ConfirmationService],
  templateUrl: './project-settings.component.html'
})
export class ProjectSettingsComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly projectsService = inject(ProjectsService);
  private readonly issuesService = inject(IssuesService);
  private readonly toastService = inject(ToastService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly projectConstraints = ProjectConstraints;
  readonly projectStatuses = projectStatuses;
  readonly projectVisibilities = projectVisibilities;
  readonly projectMemberRoles = projectMemberRoles;
  readonly getProjectMemberRoleLabel = getProjectMemberRoleLabel;
  readonly ProjectMessages = ProjectMessages;

  readonly canManageProject = computed(() =>
    canManageProjectResource(this.project()?.currentUserRole)
  );

  readonly project = signal<ProjectDetailsDto | null>(null);
  readonly assignableUsers = signal<IssueAssignableUserDto[]>([]);
  readonly memberActionUserId = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly submitError = signal<string | null>(null);
  readonly memberError = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly isAddingMember = signal(false);

  readonly availableUsers = computed(() => {
    const currentProject = this.project();
    if (!currentProject) {
      return [];
    }

    const memberIds = new Set(currentProject.members.map((member) => member.userId));
    return this.assignableUsers().filter((user) => !memberIds.has(user.id));
  });

  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    key: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    description: new FormControl('', { nonNullable: true }),
    visibility: new FormControl(ProjectVisibility.INTERNAL, { nonNullable: true }),
    status: new FormControl(ProjectStatus.ACTIVE, { nonNullable: true }),
    color: new FormControl<string>(ProjectConstraints.DEFAULT_COLOR, {
      nonNullable: true
    })
  });

  readonly addMemberForm = new FormGroup({
    userId: new FormControl<string | null>(null, Validators.required),
    role: new FormControl(ProjectMemberRole.MEMBER, {
      nonNullable: true,
      validators: [Validators.required]
    })
  });

  constructor() {
    const parentRoute = this.route.parent;
    const projectRoute = parentRoute ?? this.route;

    projectRoute.paramMap
      .pipe(
        map((params) => params.get('projectId') ?? ''),
        switchMap((projectId) => this.projectsService.getProjectById(projectId)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (project) => {
          this.project.set(project);
          this.form.patchValue({
            name: project.name,
            key: project.key,
            description: project.description ?? '',
            visibility: project.visibility,
            status: project.status,
            color: project.color
          });

          if (!this.canManageProject()) {
            this.form.disable();
          } else {
            this.loadAssignableUsers();
          }

          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  private loadAssignableUsers(): void {
    this.issuesService
      .getAssignableUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (users) => this.assignableUsers.set(users),
        error: (error: Error) => this.memberError.set(error.message)
      });
  }

  onKeyInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.form.controls.key.setValue(normalizeProjectKey(input.value), {
      emitEvent: false
    });
  }

  onSubmit(): void {
    const project = this.project();
    if (!project || !this.canManageProject()) {
      return;
    }

    this.submitError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: UpdateProjectRequest = {
      id: project.id,
      name: this.form.controls.name.value.trim(),
      key: normalizeProjectKey(this.form.controls.key.value),
      description: this.form.controls.description.value.trim() || null,
      visibility: this.form.controls.visibility.value,
      status: this.form.controls.status.value,
      color: this.form.controls.color.value.trim() || null
    };

    this.isSubmitting.set(true);

    this.projectsService
      .updateProject(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedProject) => {
          this.project.set(updatedProject);
          this.isSubmitting.set(false);
          this.toastService.success(ProjectMessages.projectUpdated);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }

  onAddMember(): void {
    const project = this.project();
    if (!project || !this.canManageProject()) {
      return;
    }

    this.memberError.set(null);

    if (this.addMemberForm.invalid) {
      this.addMemberForm.markAllAsTouched();
      return;
    }

    const userId = this.addMemberForm.controls.userId.value;
    if (!userId) {
      return;
    }

    this.isAddingMember.set(true);

    this.projectsService
      .addProjectMember(project.id, {
        userId,
        role: this.addMemberForm.controls.role.value
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedProject) => {
          this.project.set(updatedProject);
          this.addMemberForm.reset({
            userId: null,
            role: ProjectMemberRole.MEMBER
          });
          this.isAddingMember.set(false);
          this.toastService.success(ProjectMessages.memberAdded);
        },
        error: (error: Error) => {
          this.memberError.set(error.message);
          this.isAddingMember.set(false);
        }
      });
  }

  onMemberRoleChange(member: ProjectMemberDto, role: ProjectMemberRole): void {
    const project = this.project();
    if (!project || !this.canManageProject() || member.role === role) {
      return;
    }

    this.memberError.set(null);
    this.memberActionUserId.set(member.userId);

    this.projectsService
      .updateProjectMemberRole(project.id, member.userId, { role })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedProject) => {
          this.project.set(updatedProject);
          this.memberActionUserId.set(null);
          this.toastService.success(ProjectMessages.memberRoleUpdated);
        },
        error: (error: Error) => {
          this.memberError.set(error.message);
          this.memberActionUserId.set(null);
        }
      });
  }

  confirmRemoveMember(member: ProjectMemberDto): void {
    const project = this.project();
    if (!project || !this.canManageProject()) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Remove member',
      message: getRemoveProjectMemberConfirmMessage(member.displayLabel, project.name),
      accept: () => {
        this.memberError.set(null);
        this.memberActionUserId.set(member.userId);

        this.projectsService
          .removeProjectMember(project.id, member.userId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (updatedProject) => {
              this.project.set(updatedProject);
              this.memberActionUserId.set(null);
              this.toastService.success(ProjectMessages.memberRemoved);
            },
            error: (error: Error) => {
              this.memberError.set(error.message);
              this.memberActionUserId.set(null);
            }
          });
      }
    });
  }

  isMemberActionLoading(userId: string): boolean {
    return this.memberActionUserId() === userId;
  }
}
