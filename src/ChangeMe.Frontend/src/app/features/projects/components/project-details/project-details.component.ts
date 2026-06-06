import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { ProjectMembersSectionComponent } from '@features/projects/components/project-members-section/project-members-section.component';
import { ProjectMembershipHistoryTabComponent } from '@features/projects/components/project-membership-history-tab/project-membership-history-tab.component';
import { ProjectOperationHistoryTabComponent } from '@features/projects/components/project-operation-history-tab/project-operation-history-tab.component';
import { ProjectDetailsDto } from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  formatDescription,
  getDeleteProjectConfirmMessage,
  getProjectRoleLabel,
  getProjectRoleSeverity,
  ProjectMessages
} from '@features/projects/utils/projects.utils';
import { TimeMessages } from '@features/time/utils/time.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tab, TabList, TabPanel, TabPanels, Tabs } from 'primeng/tabs';
import { Tag } from 'primeng/tag';

type ProjectDetailsTab = 'membership-history' | 'operations';

@Component({
  selector: 'app-project-details',
  imports: [
    RouterLink,
    BackButtonComponent,
    Card,
    Button,
    Message,
    Tag,
    Panel,
    ProgressSpinner,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
    ProjectMembersSectionComponent,
    ProjectMembershipHistoryTabComponent,
    ProjectOperationHistoryTabComponent
  ],
  templateUrl: './project-details.component.html'
})
export class ProjectDetailsComponent {
  readonly id = input.required<string>();

  private readonly projectsService = inject(ProjectsService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly ProjectMessages = ProjectMessages;
  readonly TimeMessages = TimeMessages;
  readonly formatDescription = formatDescription;
  readonly getProjectRoleLabel = getProjectRoleLabel;
  readonly getProjectRoleSeverity = getProjectRoleSeverity;

  readonly project = signal<ProjectDetailsDto | null>(null);
  readonly pageTitle = computed(() => this.project()?.name ?? 'Project details');
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly showSystemProjectEditBlockedMessage = signal(false);
  readonly activeTab = signal<ProjectDetailsTab>('membership-history');
  readonly membershipHistoryKey = signal(0);

  readonly canViewTimeReports = computed(() =>
    this.authService.hasPermission(PermissionCodes.timeViewReports)
  );

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const blocked = params.get('systemProjectEditBlocked') === '1';
        this.showSystemProjectEditBlockedMessage.set(blocked);

        if (blocked) {
          void this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { systemProjectEditBlocked: null },
            queryParamsHandling: 'merge',
            replaceUrl: true
          });
        }

        const tab = params.get('tab');
        this.activeTab.set(tab === 'operations' ? 'operations' : 'membership-history');
      });

    effect(() => {
      this.id();
      this.loadProject();
    });
  }

  onTabChange(tab: string | number | undefined): void {
    const value: ProjectDetailsTab =
      tab === 'operations' ? 'operations' : 'membership-history';
    if (this.activeTab() === value) {
      return;
    }

    this.activeTab.set(value);

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        tab: value === 'membership-history' ? null : value
      },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  onMembersChanged(): void {
    this.membershipHistoryKey.update((value) => value + 1);
    this.refreshProjectDetails();
  }

  refresh(): void {
    this.loadProject();
  }

  private refreshProjectDetails(): void {
    this.projectsService
      .getProjectById(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (details) => this.project.set(details),
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  confirmDeleteProject(): void {
    const current = this.project();
    if (!current) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Delete project',
      message: getDeleteProjectConfirmMessage(current.name),
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => {
        this.projectsService.deleteProject(current.id).subscribe({
          next: () => {
            this.toastService.success(ProjectMessages.projectDeleted);
            void this.router.navigateByUrl('/projects');
          },
          error: (error: Error) => this.toastService.error(error.message)
        });
      }
    });
  }

  private loadProject(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.projectsService.getProjectById(this.id()).subscribe({
      next: (details) => {
        this.project.set(details);
        this.isLoading.set(false);
      },
      error: (error: Error) => {
        this.errorMessage.set(error.message);
        this.isLoading.set(false);
      }
    });
  }
}
